using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;

namespace BuscadorParaleloEspeculativo.UI.Models
{
    // Define la estructura para almacenar información sobre los procesadores.
    public class AnalisisProcesadores
    {
        public int ProcessorsDisponibles { get; set; }
        public int ProcessorsRecomendados { get; set; }
        public int ProcessorsOptimos { get; set; }
        public string TipoProcesador { get; set; } = string.Empty;
        public string RecomendacionHardware { get; set; } = string.Empty;
        public double CargaCPU { get; set; }
        public string JustificacionRecomendacion { get; set; } = string.Empty;
    }

    // Estructura para almacenar métricas de rendimiento más detalladas.
    public class MetricasEspecificas
    {
        public double ArchivosPromedioSegundo { get; set; }
        public double EficienciaMemoria { get; set; }
        public double ThroughputPalabras { get; set; }
        public int PicosMaximosParalelismo { get; set; }
        public double TiempoRespuestaPromedio { get; set; }
        public double LatenciaInicial { get; set; }
        public string RendimientoGeneral { get; set; } = string.Empty;
    }

    // Estructura para guardar el resultado de una prueba de rendimiento individual.
    public class ResultadoPrueba
    {
        public int NumeroProcessors { get; set; }
        public string NombrePrueba { get; set; } = string.Empty;
        public long TiempoMs { get; set; }
        public double Speedup { get; set; }
        public double Eficiencia { get; set; }
        public int ArchivosTotal { get; set; }
        public string ParametrosPrueba { get; set; } = string.Empty;
        public DateTime FechaPrueba { get; set; }
        public long MemoriaUtilizada { get; set; }
    }

    // Estructura que detalla de dónde proviene una palabra.
    public class OrigenPalabra
    {
        public string ArchivoOrigen { get; set; } = string.Empty;
        public int Frecuencia { get; set; }
        public DateTime FechaProcesamiento { get; set; }
        public string UbicacionEnTexto { get; set; } = string.Empty;
    }

    // Estructura que resume los resultados de un procesamiento completo (secuencial o paralelo).
    public class ResultadoProcesamiento
    {
        public string Metodo { get; set; } = string.Empty;
        public long TiempoMs { get; set; }
        public double TiempoSeg => TiempoMs / 1000.0;
        public int ArchivosProcesados { get; set; }
        public int PalabrasUnicas { get; set; }
        public int PalabrasTotal { get; set; }
        public double PalabrasPorSegundo => TiempoSeg > 0 ? PalabrasTotal / TiempoSeg : 0;
        public Dictionary<string, List<OrigenPalabra>> PalabrasConOrigen { get; set; } = new Dictionary<string, List<OrigenPalabra>>();
        public DateTime FechaEjecucion { get; set; }
        public long MemoriaUtilizada { get; set; }
    }

    // Clase que consolida todas las métricas y resultados para el informe final.
    public class MetricasProcesamiento
    {
        public int ArchivosTotal { get; set; }
        public int ArchivosProcesados { get; set; }
        public int PalabrasUnicas { get; set; }
        public int PalabrasTotal { get; set; }
        public long TiempoSecuencialMs { get; set; }
        public long TiempoParaleloMs { get; set; }
        public double TiempoSecuencialSeg => TiempoSecuencialMs / 1000.0;
        public double TiempoParaleloSeg => TiempoParaleloMs / 1000.0;
        public double Speedup => TiempoParaleloMs > 0 ? (double)TiempoSecuencialMs / TiempoParaleloMs : 0;
        public double Eficiencia => Speedup / Environment.ProcessorCount;
        public double PalabrasSecuencialPorSeg => TiempoSecuencialSeg > 0 ? PalabrasTotal / TiempoSecuencialSeg : 0;
        public double PalabrasParaleloPorSeg => TiempoParaleloSeg > 0 ? PalabrasTotal / TiempoParaleloSeg : 0;
        public string EvaluacionSpeedup
        {
            get
            {
                if (Speedup >= 3.5) return "Excelente";
                if (Speedup >= 2.5) return "Muy Bueno";
                if (Speedup >= 1.5) return "Bueno";
                if (Speedup >= 1.1) return "Aceptable";
                return "Pobre";
            }
        }
        public List<EstadoArchivo> EstadoArchivos { get; set; } = new List<EstadoArchivo>();
        public AnalisisProcesadores AnalisisProcesadores { get; set; } = new AnalisisProcesadores();
        public MetricasEspecificas MetricasEspecificas { get; set; } = new MetricasEspecificas();
        public List<ResultadoPrueba> ResultadosPruebas { get; set; } = new List<ResultadoPrueba>();
        public DateTime FechaAnalisis { get; set; }
        public string VersionSistema { get; set; } = string.Empty;
        public long MemoriaTotal { get; set; }
        public string ConfiguracionOptima { get; set; } = string.Empty;
    }

    // Estructura para registrar el estado de cada archivo procesado.
    public class EstadoArchivo
    {
        public string NombreArchivo { get; set; } = string.Empty;
        public string RutaCompleta { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int PalabrasProcesadas { get; set; }
        public long TamañoBytes { get; set; }
        public string TamañoLegible => FormatearTamaño(TamañoBytes);

        private string FormatearTamaño(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }

    // Estructura para el contexto de una palabra, útil para modelos de lenguaje.
    public class ContextoPalabra
    {
        public string PalabraActual { get; set; } = string.Empty;
        public string PalabraAnterior { get; set; } = string.Empty;
        public string ArchivoOrigen { get; set; } = string.Empty;
        public int Posicion { get; set; }
    }

    // Clase simplificada para manejar solo el guardado JSON
    public static class GuardadorMetricas
    {
        private static string EncontrarCarpetaRaizProyecto()
        {
            var directorioActual = new DirectoryInfo(Directory.GetCurrentDirectory());

            Console.WriteLine($"Buscando carpeta raíz desde: {directorioActual.FullName}");

            while (directorioActual != null)
            {
                var carpetaMetrics = Path.Combine(directorioActual.FullName, "metrics");
                if (Directory.Exists(carpetaMetrics))
                {
                    Console.WriteLine($"Carpeta raíz encontrada: {directorioActual.FullName}");
                    return directorioActual.FullName;
                }

                if (Directory.Exists(Path.Combine(directorioActual.FullName, "src")))
                {
                    var metricsEnRaiz = Path.Combine(directorioActual.FullName, "metrics");
                    if (Directory.Exists(metricsEnRaiz))
                    {
                        Console.WriteLine($"Carpeta raíz encontrada por src: {directorioActual.FullName}");
                        return directorioActual.FullName;
                    }
                }

                if (directorioActual.Name.ToLower().Contains("buscador"))
                {
                    var metricsAqui = Path.Combine(directorioActual.FullName, "metrics");
                    if (Directory.Exists(metricsAqui))
                    {
                        Console.WriteLine($"Carpeta raíz encontrada por nombre: {directorioActual.FullName}");
                        return directorioActual.FullName;
                    }
                }

                directorioActual = directorioActual.Parent;
            }

            var carpetaActual = Directory.GetCurrentDirectory();
            Console.WriteLine($"Usando directorio actual: {carpetaActual}");
            return carpetaActual;
        }

        private static readonly Lazy<string> _carpetaMetricas = new Lazy<string>(() =>
        {
            var raiz = EncontrarCarpetaRaizProyecto();
            var carpetaMetricas = Path.Combine(raiz, "metrics");

            if (!Directory.Exists(carpetaMetricas))
            {
                Directory.CreateDirectory(carpetaMetricas);
                Console.WriteLine($"Carpeta metrics creada: {carpetaMetricas}");
            }
            else
            {
                Console.WriteLine($"Usando carpeta metrics: {carpetaMetricas}");
            }

            return carpetaMetricas;
        });

        private static string CarpetaMetricas => _carpetaMetricas.Value;

        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Método simplificado que solo guarda JSON
        public static async Task GuardarMetricasAsync(MetricasProcesamiento metricas)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"analisis_rendimiento_{timestamp}.json";
                var rutaCompleta = Path.Combine(CarpetaMetricas, nombreArchivo);

                var jsonMetricas = JsonSerializer.Serialize(metricas, OpcionesJson);
                await File.WriteAllTextAsync(rutaCompleta, jsonMetricas, Encoding.UTF8);

                Console.WriteLine($"\n✅ MÉTRICAS JSON GUARDADAS EXITOSAMENTE");
                Console.WriteLine($"📁 Carpeta: {CarpetaMetricas}");
                Console.WriteLine($"📄 Archivo: {nombreArchivo}");
                Console.WriteLine($"🔗 Ruta completa: {rutaCompleta}");
                Console.WriteLine($"💾 Tamaño: {new FileInfo(rutaCompleta).Length / 1024.0:F1} KB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ ERROR GUARDANDO MÉTRICAS:");
                Console.WriteLine($"   Mensaje: {ex.Message}");
                Console.WriteLine($"   Ruta intentada: {CarpetaMetricas}");
            }
        }

        public static async Task<List<string>> ListarMetricasGuardasAsync()
        {
            try
            {
                if (!Directory.Exists(CarpetaMetricas))
                    return new List<string>();

                var archivos = Directory.GetFiles(CarpetaMetricas, "analisis_rendimiento_*.json")
                                       .OrderByDescending(f => File.GetCreationTime(f))
                                       .ToList();

                return archivos.Select(Path.GetFileName).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static async Task<MetricasProcesamiento?> CargarMetricasAsync(string nombreArchivo)
        {
            try
            {
                var rutaCompleta = Path.Combine(CarpetaMetricas, nombreArchivo);
                if (!File.Exists(rutaCompleta))
                    return null;

                var json = await File.ReadAllTextAsync(rutaCompleta, Encoding.UTF8);
                return JsonSerializer.Deserialize<MetricasProcesamiento>(json, OpcionesJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando métricas: {ex.Message}");
                return null;
            }
        }
    }

    // CLASE PRINCIPAL: Contiene toda la lógica del procesamiento.
    public class ProcesadorArchivos
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>> _palabrasConOrigen;
        private readonly ConcurrentBag<ContextoPalabra> _contextoPalabras;
        private readonly List<EstadoArchivo> _estadoArchivos;
        private readonly ConcurrentDictionary<string, int> _frecuenciaPalabras;
        private readonly object _lockEstados = new object();

        private static readonly HashSet<string> PalabrasVacias = new HashSet<string>
        {
            "a", "ante", "bajo", "cabe", "con", "contra", "de", "desde", "durante", "en", "entre",
            "hacia", "hasta", "mediante", "para", "por", "según", "sin", "so", "sobre", "tras",
            "el", "la", "los", "las", "un", "una", "unos", "unas", "y", "o", "u", "que", "como",
            "es", "son", "ser", "fue", "fueron", "este", "esta", "estos", "estas", "se", "su", "sus",
            "lo", "le", "les", "del", "al", "más", "pero", "muy", "hay", "está", "todo", "también",
            "donde", "cuando", "mientras", "otro", "otros", "otra", "otras", "mismo", "misma", "será",
            "pueden", "solo", "cada", "tiene", "hacer", "después", "forma", "bien", "aquí", "tanto",
            "estado", "siempre", "ejemplo", "tiempo", "casos"
        };

        public ProcesadorArchivos()
        {
            _palabrasConOrigen = new ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>>();
            _contextoPalabras = new ConcurrentBag<ContextoPalabra>();
            _estadoArchivos = new List<EstadoArchivo>();
            _frecuenciaPalabras = new ConcurrentDictionary<string, int>();
        }

        public async Task<MetricasProcesamiento> ProcesarArchivosSubidosAsync(IFormFile[] archivosSubidos, string carpetaTemporal = "uploads")
        {
            try
            {
                Directory.CreateDirectory(carpetaTemporal);
                var rutasGuardadas = new List<string>();

                foreach (var archivo in archivosSubidos)
                {
                    if (archivo.Length > 0 && EsArchivoValido(archivo.FileName))
                    {
                        var rutaArchivo = Path.Combine(carpetaTemporal, LimpiarNombreArchivo(archivo.FileName));
                        using var stream = new FileStream(rutaArchivo, FileMode.Create);
                        await archivo.CopyToAsync(stream);
                        rutasGuardadas.Add(rutaArchivo);
                    }
                }

                return rutasGuardadas.Count == 0
                    ? new MetricasProcesamiento { ArchivosTotal = 0 }
                    : await AnalisisCompletoRendimientoAsync(rutasGuardadas.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando archivos: {ex.Message}");
                return new MetricasProcesamiento { ArchivosTotal = 0 };
            }
        }

        public async Task<MetricasProcesamiento> AnalisisCompletoRendimientoAsync(string[] archivos)
        {
            if (archivos.Length == 0) return new MetricasProcesamiento { ArchivosTotal = 0 };

            Console.WriteLine($"Analizando {archivos.Length} archivos con {Environment.ProcessorCount} cores");
            var metricas = new MetricasProcesamiento
            {
                ArchivosTotal = archivos.Length,
                FechaAnalisis = DateTime.Now,
                VersionSistema = Environment.OSVersion.ToString()
            };

            metricas.AnalisisProcesadores = AnalizarProcesadores();

            // Ejecuta el procesamiento secuencial como baseline
            Console.WriteLine("Ejecutando baseline secuencial...");
            var secuencial = await ProcesarSecuencialAsync(archivos);

            LimpiarDatos();

            // Usa las mismas configuraciones de cores que en tu output
            var configuraciones = new List<int> { 1, 2, 4, 6, 8, 10 };
            Console.WriteLine($"Probando con {configuraciones.Count} configuraciones de cores...");

            foreach (var cores in configuraciones)
            {
                Console.WriteLine($"Probando con {cores} cores...");
                var inicioMemoria = GC.GetTotalMemory(false);
                var paralelo = await ProcesarParaleloAsync(archivos, cores);
                var finMemoria = GC.GetTotalMemory(false);

                var prueba = new ResultadoPrueba
                {
                    NumeroProcessors = cores,
                    NombrePrueba = $"Paralelo-{cores}cores",
                    TiempoMs = paralelo.TiempoMs,
                    FechaPrueba = DateTime.Now,
                    MemoriaUtilizada = finMemoria - inicioMemoria,
                    Speedup = secuencial.TiempoMs > 0 ? (double)secuencial.TiempoMs / paralelo.TiempoMs : 0,
                    Eficiencia = cores > 0 ? ((double)secuencial.TiempoMs / paralelo.TiempoMs) / cores : 0,
                    ArchivosTotal = archivos.Length,
                    ParametrosPrueba = $"MaxDegreeOfParallelism={cores}"
                };

                metricas.ResultadosPruebas.Add(prueba);

                Console.WriteLine($"  Speedup: {prueba.Speedup:F2}x | Eficiencia: {prueba.Eficiencia * 100:F1}%");

                LimpiarDatos();
            }

            var mejorPrueba = metricas.ResultadosPruebas.OrderByDescending(p => p.Speedup).First();
            Console.WriteLine($"\nMejor configuración: {mejorPrueba.NumeroProcessors} cores con speedup de {mejorPrueba.Speedup:F2}x");

            var resultadoFinal = await ProcesarParaleloAsync(archivos, mejorPrueba.NumeroProcessors);

            metricas = CompletarMetricas(metricas, secuencial, resultadoFinal);
            metricas.ConfiguracionOptima = $"{mejorPrueba.NumeroProcessors} cores (Speedup: {mejorPrueba.Speedup:F2}x)";

            // Solo guarda el JSON
            await GuardadorMetricas.GuardarMetricasAsync(metricas);

            MostrarAnalisisRendimiento(metricas);
            return metricas;
        }

        private async Task<ResultadoProcesamiento> ProcesarSecuencialAsync(string[] archivos)
        {
            var sw = Stopwatch.StartNew();
            var inicioMemoria = GC.GetTotalMemory(false);
            var palabras = new Dictionary<string, List<OrigenPalabra>>();
            int procesados = 0, totalPalabras = 0;

            foreach (var archivo in archivos)
            {
                try
                {
                    ActualizarEstado(archivo, "Procesando...");
                    var resultado = await ExtraerPalabrasAsync(archivo);

                    foreach (var palabra in resultado.Palabras)
                    {
                        if (!palabras.ContainsKey(palabra.Key))
                            palabras[palabra.Key] = new List<OrigenPalabra>();

                        palabras[palabra.Key].Add(new OrigenPalabra
                        {
                            ArchivoOrigen = Path.GetFileName(archivo),
                            Frecuencia = palabra.Value,
                            FechaProcesamiento = DateTime.Now
                        });
                    }

                    totalPalabras += resultado.Palabras.Values.Sum();
                    procesados++;
                    ActualizarEstado(archivo, "Procesado", resultado.Palabras.Values.Sum());
                }
                catch (Exception ex)
                {
                    ActualizarEstado(archivo, $"Error: {ex.Message}");
                }
            }

            var finMemoria = GC.GetTotalMemory(false);

            return new ResultadoProcesamiento
            {
                Metodo = "Secuencial",
                TiempoMs = sw.ElapsedMilliseconds,
                ArchivosProcesados = procesados,
                PalabrasUnicas = palabras.Count,
                PalabrasTotal = totalPalabras,
                PalabrasConOrigen = palabras,
                FechaEjecucion = DateTime.Now,
                MemoriaUtilizada = finMemoria - inicioMemoria
            };
        }

        private async Task<ResultadoProcesamiento> ProcesarParaleloAsync(string[] archivos, int maxCores)
        {
            var sw = Stopwatch.StartNew();
            var inicioMemoria = GC.GetTotalMemory(false);
            _palabrasConOrigen.Clear();
            int procesados = 0, totalPalabras = 0;

            await Task.Run(() =>
            {
                Parallel.ForEach(archivos, new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxCores
                }, archivo =>
                {
                    try
                    {
                        ActualizarEstado(archivo, "Procesando...");
                        var resultado = ExtraerPalabrasAsync(archivo).Result;

                        foreach (var palabra in resultado.Palabras.Where(p => !PalabrasVacias.Contains(p.Key) && p.Key.Length > 1))
                        {
                            _palabrasConOrigen.AddOrUpdate(palabra.Key,
                                new ConcurrentBag<OrigenPalabra>
                                {
                                    new OrigenPalabra
                                    {
                                        ArchivoOrigen = Path.GetFileName(archivo),
                                        Frecuencia = palabra.Value,
                                        FechaProcesamiento = DateTime.Now
                                    }
                                },
                                (key, bag) =>
                                {
                                    bag.Add(new OrigenPalabra
                                    {
                                        ArchivoOrigen = Path.GetFileName(archivo),
                                        Frecuencia = palabra.Value,
                                        FechaProcesamiento = DateTime.Now
                                    });
                                    return bag;
                                });

                            _frecuenciaPalabras.AddOrUpdate(palabra.Key, palabra.Value, (k, v) => v + palabra.Value);
                        }

                        foreach (var contexto in resultado.Contextos)
                            _contextoPalabras.Add(contexto);

                        Interlocked.Add(ref totalPalabras, resultado.Palabras.Values.Sum());
                        Interlocked.Increment(ref procesados);
                        ActualizarEstado(archivo, "Procesado", resultado.Palabras.Values.Sum());
                    }
                    catch (Exception ex)
                    {
                        ActualizarEstado(archivo, $"Error: {ex.Message}");
                    }
                });
            });

            var finMemoria = GC.GetTotalMemory(false);

            return new ResultadoProcesamiento
            {
                Metodo = $"Paralelo-{maxCores}cores",
                TiempoMs = sw.ElapsedMilliseconds,
                ArchivosProcesados = procesados,
                PalabrasUnicas = _palabrasConOrigen.Count,
                PalabrasTotal = totalPalabras,
                PalabrasConOrigen = _palabrasConOrigen.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList()),
                FechaEjecucion = DateTime.Now,
                MemoriaUtilizada = finMemoria - inicioMemoria
            };
        }

        private async Task<(Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos)> ExtraerPalabrasAsync(string archivo)
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
                    _ => throw new NotSupportedException($"Formato {extension} no soportado")
                };

                return TokenizarConContexto(contenido, Path.GetFileName(archivo));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en {Path.GetFileName(archivo)}: {ex.Message}");
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());
            }
        }

        private async Task<string> ExtraerDeDocxAsync(string archivo) => await Task.Run(() =>
        {
            try
            {
                using var doc = WordprocessingDocument.Open(archivo, false);
                return doc.MainDocumentPart?.Document?.Body?.InnerText ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
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
            catch
            {
                return string.Empty;
            }
        });

        private (Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos) TokenizarConContexto(string texto, string archivo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());

            var palabras = texto.ToLower()
                .Split(new[] { ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '\"', '(', ')', '[', ']', '{', '}' },
                       StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !PalabrasVacias.Contains(p) && p.Length > 1)
                .ToArray();

            var conteos = new Dictionary<string, int>();
            foreach (var palabra in palabras)
                conteos[palabra] = conteos.GetValueOrDefault(palabra, 0) + 1;

            var contextos = new List<ContextoPalabra>();
            for (int i = 1; i < palabras.Length; i++)
            {
                contextos.Add(new ContextoPalabra
                {
                    PalabraAnterior = palabras[i - 1],
                    PalabraActual = palabras[i],
                    ArchivoOrigen = archivo,
                    Posicion = i
                });
            }

            return (conteos, contextos);
        }

        private AnalisisProcesadores AnalizarProcesadores()
        {
            var cores = Environment.ProcessorCount;
            return new AnalisisProcesadores
            {
                ProcessorsDisponibles = cores,
                ProcessorsRecomendados = Math.Max(1, (int)(cores * 0.75)),
                ProcessorsOptimos = cores,
                TipoProcesador = cores >= 16 ? "Servidor/Workstation" :
                                cores >= 8 ? "Desktop High-End" :
                                cores >= 4 ? "Desktop Standard" : "Low-End/Mobile",
                RecomendacionHardware = cores >= 8 ? "Excelente para paralelismo" :
                                       cores >= 4 ? "Adecuado para paralelismo" : "Limitado",
                JustificacionRecomendacion = $"Para I/O + CPU intensivo, usar {Math.Max(1, (int)(cores * 0.75))} de {cores} cores evita contención."
            };
        }

        private MetricasProcesamiento CompletarMetricas(MetricasProcesamiento metricas, ResultadoProcesamiento secuencial, ResultadoProcesamiento paralelo)
        {
            metricas.ArchivosProcesados = paralelo.ArchivosProcesados;
            metricas.PalabrasUnicas = paralelo.PalabrasUnicas;
            metricas.PalabrasTotal = paralelo.PalabrasTotal;
            metricas.TiempoSecuencialMs = secuencial.TiempoMs;
            metricas.TiempoParaleloMs = paralelo.TiempoMs;
            metricas.EstadoArchivos = new List<EstadoArchivo>(_estadoArchivos);
            metricas.MemoriaTotal = GC.GetTotalMemory(false);

            var archivosCount = metricas.ArchivosTotal;
            var maxParalelismo = metricas.ResultadosPruebas.Max(p => p.NumeroProcessors);

            metricas.MetricasEspecificas = new MetricasEspecificas
            {
                ArchivosPromedioSegundo = paralelo.TiempoSeg > 0 ? archivosCount / paralelo.TiempoSeg : 0,
                ThroughputPalabras = paralelo.PalabrasPorSegundo,
                EficienciaMemoria = paralelo.PalabrasTotal > 0 ? (double)paralelo.PalabrasUnicas / paralelo.PalabrasTotal * 100 : 0,
                TiempoRespuestaPromedio = archivosCount > 0 ? paralelo.TiempoMs / (double)archivosCount : 0,
                LatenciaInicial = paralelo.TiempoMs * 0.1,
                PicosMaximosParalelismo = maxParalelismo,
                RendimientoGeneral = metricas.Speedup >= 3.5 ? "Excelente" :
                                   metricas.Speedup >= 2.5 ? "Muy Bueno" :
                                   metricas.Speedup >= 1.5 ? "Bueno" : "Mejorable"
            };

            return metricas;
        }

        private void MostrarAnalisisRendimiento(MetricasProcesamiento metricas)
        {
            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("ANÁLISIS DE RENDIMIENTO DEL SISTEMA");
            Console.WriteLine(new string('=', 60));

            var proc = metricas.AnalisisProcesadores;
            Console.WriteLine($"PROCESADOR: {proc.TipoProcesador}");
            Console.WriteLine($"   Cores disponibles: {proc.ProcessorsDisponibles}");
            Console.WriteLine($"   Cores recomendados: {proc.ProcessorsRecomendados}");
            Console.WriteLine($"   Recomendación: {proc.RecomendacionHardware}");

            Console.WriteLine("\n" + new string('-', 40));
            Console.WriteLine("MÉTRICAS PRINCIPALES:");
            Console.WriteLine($"   Tiempo Secuencial: {metricas.TiempoSecuencialSeg:F2}s");
            Console.WriteLine($"   Tiempo Paralelo: {metricas.TiempoParaleloSeg:F2}s");
            Console.WriteLine($"   Speedup: {metricas.Speedup:F2}x ({metricas.EvaluacionSpeedup})");
            Console.WriteLine($"   Eficiencia: {metricas.Eficiencia * 100:F1}%");
            Console.WriteLine($"   Configuración óptima: {metricas.ConfiguracionOptima}");

            Console.WriteLine("\n" + new string('-', 40));
            Console.WriteLine("RESUMEN DE PRUEBAS:");
            foreach (var prueba in metricas.ResultadosPruebas.OrderBy(p => p.NumeroProcessors))
            {
                var estado = prueba.Speedup >= 2.0 ? "EXCELENTE" : prueba.Speedup >= 1.5 ? "BUENO" : "REGULAR";
                Console.WriteLine($"   {estado} - {prueba.NombrePrueba}: {prueba.Speedup:F2}x speedup en {prueba.TiempoMs}ms");
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine($"ANÁLISIS COMPLETADO - {metricas.ArchivosProcesados} archivos procesados");
            Console.WriteLine($"Palabras: {metricas.PalabrasTotal:N0} total, {metricas.PalabrasUnicas:N0} únicas");
            Console.WriteLine(new string('=', 60));
        }

        // Métodos para acceder a los datos
        public List<ContextoPalabra> ObtenerContextosPalabras() => _contextoPalabras.ToList();

        public Dictionary<string, int> ObtenerFrecuenciaPalabras() => _frecuenciaPalabras.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public Dictionary<string, List<OrigenPalabra>> ObtenerTodasLasPalabras() =>
            _palabrasConOrigen.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());

        public List<EstadoArchivo> ObtenerEstadoArchivos()
        {
            lock (_lockEstados)
            {
                return new List<EstadoArchivo>(_estadoArchivos);
            }
        }

        private void ActualizarEstado(string ruta, string estado, int palabras = 0)
        {
            var nombre = Path.GetFileName(ruta);
            var tamaño = File.Exists(ruta) ? new FileInfo(ruta).Length : 0;

            lock (_lockEstados)
            {
                var existente = _estadoArchivos.FirstOrDefault(a => a.NombreArchivo == nombre);
                if (existente == null)
                {
                    _estadoArchivos.Add(new EstadoArchivo
                    {
                        NombreArchivo = nombre,
                        RutaCompleta = ruta,
                        Estado = estado,
                        PalabrasProcesadas = palabras,
                        TamañoBytes = tamaño
                    });
                }
                else
                {
                    existente.Estado = estado;
                    existente.PalabrasProcesadas = palabras;
                }
            }
        }

        public MetricasProcesamiento ObtenerDatosParaInterfaz()
        {
            return new MetricasProcesamiento
            {
                EstadoArchivos = ObtenerEstadoArchivos(),
                PalabrasTotal = _frecuenciaPalabras.Sum(x => x.Value),
                PalabrasUnicas = _frecuenciaPalabras.Count
            };
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
            _palabrasConOrigen.Clear();
            _contextoPalabras.Clear();
            _frecuenciaPalabras.Clear();
            lock (_lockEstados)
            {
                _estadoArchivos.Clear();
            }
        }

        // Métodos adicionales para el manejo de métricas guardadas
        public async Task<List<string>> ObtenerHistorialMetricasAsync()
        {
            return await GuardadorMetricas.ListarMetricasGuardasAsync();
        }

        public async Task<MetricasProcesamiento?> CargarMetricasAnterioresAsync(string nombreArchivo)
        {
            return await GuardadorMetricas.CargarMetricasAsync(nombreArchivo);
        }

        public async Task CompararRendimientoAsync(string archivoMetricas1, string archivoMetricas2)
        {
            var metricas1 = await GuardadorMetricas.CargarMetricasAsync(archivoMetricas1);
            var metricas2 = await GuardadorMetricas.CargarMetricasAsync(archivoMetricas2);

            if (metricas1 == null || metricas2 == null)
            {
                Console.WriteLine("No se pudieron cargar las métricas para comparar.");
                return;
            }

            Console.WriteLine("\n" + new string('=', 60));
            Console.WriteLine("COMPARACIÓN DE RENDIMIENTO");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Análisis 1: {metricas1.FechaAnalisis:yyyy-MM-dd HH:mm}");
            Console.WriteLine($"Análisis 2: {metricas2.FechaAnalisis:yyyy-MM-dd HH:mm}");
            Console.WriteLine(new string('-', 40));
            Console.WriteLine($"Speedup: {metricas1.Speedup:F2}x vs {metricas2.Speedup:F2}x");
            Console.WriteLine($"Eficiencia: {metricas1.Eficiencia * 100:F1}% vs {metricas2.Eficiencia * 100:F1}%");
            Console.WriteLine($"Throughput: {metricas1.PalabrasParaleloPorSeg:N0} vs {metricas2.PalabrasParaleloPorSeg:N0} palabras/seg");
            Console.WriteLine(new string('=', 60));
        }
    }
}
