using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;

namespace BuscadorParaleloEspeculativo.UI.Models
{
    // Estructura simple para el resultado de cada prueba
    public class ResultadoPrueba
    {
        public int Cores { get; set; }
        public long TiempoMs { get; set; }
        public double Speedup { get; set; }
        public double Eficiencia { get; set; }
        public int ArchivosTotal { get; set; }
        public int PalabrasTotal { get; set; }
        public DateTime FechaPrueba { get; set; }
        public long MemoriaUtilizada { get; set; }
    }

    // Estructura simplificada para las métricas finales
    public class MetricasFinales
    {
        public DateTime FechaAnalisis { get; set; }
        public int TotalArchivos { get; set; }
        public int ArchivosProcesados { get; set; }
        public int PalabrasUnicas { get; set; }
        public int PalabrasTotal { get; set; }
        public List<ResultadoPrueba> Pruebas { get; set; } = new List<ResultadoPrueba>();
        public int MejorConfiguracion { get; set; }
        public double MejorSpeedup { get; set; }
        public string EvaluacionGeneral { get; set; } = string.Empty;
    }

    public class ProcesadorArchivos
    {
        private readonly ConcurrentDictionary<string, int> _palabras;
        private readonly List<string> _archivosEstado;
        private readonly object _lockEstados = new object();

        // Configuraciones específicas que quieres probar
        private static readonly int[] ConfiguracionesCores = { 1, 2, 4, 6, 8, 10, 12, 16 };

        private static readonly HashSet<string> PalabrasVacias = new HashSet<string>
        {
            "a", "ante", "bajo", "con", "de", "el", "la", "los", "las", "un", "una", "y", "o", "que",
            "es", "son", "se", "su", "del", "al", "más", "pero", "muy", "hay", "está", "todo", "también"
        };

        public ProcesadorArchivos()
        {
            _palabras = new ConcurrentDictionary<string, int>();
            _archivosEstado = new List<string>();
        }

        // Método principal simplificado
        public async Task<MetricasFinales> ProcesarYGenerarJsonAsync(IFormFile[] archivosSubidos)
        {
            // Guardar archivos temporalmente
            var rutasArchivos = await GuardarArchivosTemporalesAsync(archivosSubidos);
            if (rutasArchivos.Count == 0)
                return new MetricasFinales { TotalArchivos = 0 };

            var metricas = new MetricasFinales
            {
                FechaAnalisis = DateTime.Now,
                TotalArchivos = rutasArchivos.Count
            };

            Console.WriteLine($"Procesando {rutasArchivos.Count} archivos...");

            // Procesar con cada configuración de cores
            long tiempoSecuencial = 0;

            foreach (var cores in ConfiguracionesCores)
            {
                // Solo procesar si el sistema tiene suficientes cores
                if (cores > Environment.ProcessorCount && cores > 8) continue;

                Console.WriteLine($"Probando con {cores} cores...");

                LimpiarDatos();
                var inicioMemoria = GC.GetTotalMemory(false);
                var resultado = await ProcesarConCoresAsync(rutasArchivos, cores);
                var finMemoria = GC.GetTotalMemory(false);

                // Guardar tiempo secuencial como referencia
                if (cores == 1) tiempoSecuencial = resultado.TiempoMs;

                var prueba = new ResultadoPrueba
                {
                    Cores = cores,
                    TiempoMs = resultado.TiempoMs,
                    Speedup = tiempoSecuencial > 0 ? (double)tiempoSecuencial / resultado.TiempoMs : 1.0,
                    Eficiencia = cores > 0 && tiempoSecuencial > 0 ?
                        ((double)tiempoSecuencial / resultado.TiempoMs) / cores : 0,
                    ArchivosTotal = rutasArchivos.Count,
                    PalabrasTotal = _palabras.Sum(x => x.Value),
                    FechaPrueba = DateTime.Now,
                    MemoriaUtilizada = finMemoria - inicioMemoria
                };

                metricas.Pruebas.Add(prueba);
                Console.WriteLine($"  Tiempo: {prueba.TiempoMs}ms | Speedup: {prueba.Speedup:F2}x");
            }

            // Completar métricas finales
            metricas.ArchivosProcesados = rutasArchivos.Count;
            metricas.PalabrasUnicas = _palabras.Count;
            metricas.PalabrasTotal = _palabras.Sum(x => x.Value);

            var mejorPrueba = metricas.Pruebas.OrderByDescending(p => p.Speedup).FirstOrDefault();
            if (mejorPrueba != null)
            {
                metricas.MejorConfiguracion = mejorPrueba.Cores;
                metricas.MejorSpeedup = mejorPrueba.Speedup;
                metricas.EvaluacionGeneral = mejorPrueba.Speedup >= 3.0 ? "Excelente" :
                                           mejorPrueba.Speedup >= 2.0 ? "Muy Bueno" :
                                           mejorPrueba.Speedup >= 1.5 ? "Bueno" : "Regular";
            }

            // Guardar solo el JSON
            await GuardarJsonAsync(metricas);

            // Limpiar archivos temporales
            LimpiarArchivosTemporales(rutasArchivos);

            return metricas;
        }

        private async Task<List<string>> GuardarArchivosTemporalesAsync(IFormFile[] archivos)
        {
            var rutasGuardadas = new List<string>();
            var carpetaTemp = "temp_uploads";
            Directory.CreateDirectory(carpetaTemp);

            foreach (var archivo in archivos)
            {
                if (archivo.Length > 0 && EsArchivoValido(archivo.FileName))
                {
                    var ruta = Path.Combine(carpetaTemp, LimpiarNombreArchivo(archivo.FileName));
                    using var stream = new FileStream(ruta, FileMode.Create);
                    await archivo.CopyToAsync(stream);
                    rutasGuardadas.Add(ruta);
                }
            }

            return rutasGuardadas;
        }

        private async Task<(long TiempoMs, int Palabras)> ProcesarConCoresAsync(List<string> archivos, int maxCores)
        {
            var sw = Stopwatch.StartNew();
            int totalPalabras = 0;

            if (maxCores == 1)
            {
                // Procesamiento secuencial
                foreach (var archivo in archivos)
                {
                    var palabras = await ExtraerPalabrasAsync(archivo);
                    foreach (var palabra in palabras)
                        _palabras.AddOrUpdate(palabra.Key, palabra.Value, (k, v) => v + palabra.Value);
                    totalPalabras += palabras.Values.Sum();
                }
            }
            else
            {
                // Procesamiento paralelo
                await Task.Run(() =>
                {
                    Parallel.ForEach(archivos, new ParallelOptions { MaxDegreeOfParallelism = maxCores }, archivo =>
                    {
                        try
                        {
                            var palabras = ExtraerPalabrasAsync(archivo).Result;
                            foreach (var palabra in palabras)
                                _palabras.AddOrUpdate(palabra.Key, palabra.Value, (k, v) => v + palabra.Value);

                            Interlocked.Add(ref totalPalabras, palabras.Values.Sum());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error procesando {archivo}: {ex.Message}");
                        }
                    });
                });
            }

            return (sw.ElapsedMilliseconds, totalPalabras);
        }

        private async Task<Dictionary<string, int>> ExtraerPalabrasAsync(string archivo)
        {
            var extension = Path.GetExtension(archivo).ToLower();
            string contenido = "";

            try
            {
                contenido = extension switch
                {
                    ".txt" => await File.ReadAllTextAsync(archivo, Encoding.UTF8),
                    ".docx" => await ExtraerDeDocxAsync(archivo),
                    ".pdf" => await ExtraerDePdfAsync(archivo),
                    _ => string.Empty
                };

                return TokenizarTexto(contenido);
            }
            catch
            {
                return new Dictionary<string, int>();
            }
        }

        private async Task<string> ExtraerDeDocxAsync(string archivo) => await Task.Run(() =>
        {
            try
            {
                using var doc = WordprocessingDocument.Open(archivo, false);
                return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
            }
            catch { return string.Empty; }
        });

        private async Task<string> ExtraerDePdfAsync(string archivo) => await Task.Run(() =>
        {
            try
            {
                using var reader = new PdfReader(archivo);
                using var pdf = new PdfDocument(reader);
                var contenido = new StringBuilder();

                for (int i = 1; i <= pdf.GetNumberOfPages(); i++)
                    contenido.AppendLine(PdfTextExtractor.GetTextFromPage(pdf.GetPage(i)));

                return contenido.ToString();
            }
            catch { return string.Empty; }
        });

        private Dictionary<string, int> TokenizarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return new Dictionary<string, int>();

            var palabras = texto.ToLower()
                .Split(new[] { ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '"', '(', ')', '[', ']' },
                       StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !PalabrasVacias.Contains(p) && p.Length > 1)
                .ToArray();

            var conteos = new Dictionary<string, int>();
            foreach (var palabra in palabras)
                conteos[palabra] = conteos.GetValueOrDefault(palabra, 0) + 1;

            return conteos;
        }

        private async Task GuardarJsonAsync(MetricasFinales metricas)
        {
            try
            {
                var carpetaMetricas = "metrics";
                Directory.CreateDirectory(carpetaMetricas);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"rendimiento_{timestamp}.json";
                var rutaCompleta = Path.Combine(carpetaMetricas, nombreArchivo);

                var opciones = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(metricas, opciones);
                await File.WriteAllTextAsync(rutaCompleta, json, Encoding.UTF8);

                Console.WriteLine($"\n✅ Métricas guardadas en: {rutaCompleta}");
                Console.WriteLine($"📊 Mejor configuración: {metricas.MejorConfiguracion} cores");
                Console.WriteLine($"⚡ Speedup máximo: {metricas.MejorSpeedup:F2}x");
                Console.WriteLine($"🎯 Evaluación: {metricas.EvaluacionGeneral}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error guardando JSON: {ex.Message}");
            }
        }

        private void LimpiarArchivosTemporales(List<string> rutas)
        {
            foreach (var ruta in rutas)
            {
                try
                {
                    if (File.Exists(ruta)) File.Delete(ruta);
                }
                catch { }
            }

            try
            {
                if (Directory.Exists("temp_uploads"))
                    Directory.Delete("temp_uploads", true);
            }
            catch { }
        }

        private bool EsArchivoValido(string nombre) =>
            new[] { ".txt", ".pdf", ".docx" }.Contains(Path.GetExtension(nombre).ToLower());

        private string LimpiarNombreArchivo(string nombre)
        {
            foreach (var c in new[] { "<", ">", ":", "\"", "|", "?", "*" })
                nombre = nombre.Replace(c, "_");
            return nombre;
        }

        public void LimpiarDatos()
        {
            _palabras.Clear();
            lock (_lockEstados)
            {
                _archivosEstado.Clear();
            }
        }

        // Método público simplificado para usar desde el controlador
        public async Task<string> ProcesarArchivosAsync(IFormFile[] archivos)
        {
            var metricas = await ProcesarYGenerarJsonAsync(archivos);

            if (metricas.TotalArchivos == 0)
                return "No se pudieron procesar archivos";

            var resultado = new StringBuilder();
            resultado.AppendLine($"Procesados: {metricas.ArchivosProcesados} archivos");
            resultado.AppendLine($"Palabras: {metricas.PalabrasTotal:N0} total, {metricas.PalabrasUnicas:N0} únicas");
            resultado.AppendLine($"Mejor configuración: {metricas.MejorConfiguracion} cores");
            resultado.AppendLine($"Speedup máximo: {metricas.MejorSpeedup:F2}x");
            resultado.AppendLine($"Evaluación: {metricas.EvaluacionGeneral}");

            return resultado.ToString();
        }

        // Método para obtener el último archivo JSON generado
        public async Task<string?> ObtenerUltimoJsonAsync()
        {
            try
            {
                var carpetaMetricas = "metrics";
                if (!Directory.Exists(carpetaMetricas)) return null;

                var archivos = Directory.GetFiles(carpetaMetricas, "rendimiento_*.json")
                                      .OrderByDescending(f => File.GetCreationTime(f))
                                      .FirstOrDefault();

                if (archivos == null) return null;

                return await File.ReadAllTextAsync(archivos, Encoding.UTF8);
            }
            catch
            {
                return null;
            }
        }
    }
}