using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
    // Datos de dónde viene cada palabra procesada
    public class OrigenPalabra
    {
        public string ArchivoOrigen { get; set; }
        public int Frecuencia { get; set; }
        public DateTime FechaProcesamiento { get; set; }
        public string UbicacionEnTexto { get; set; }
    }

    // Resultados de procesamiento (secuencial o paralelo)
    public class ResultadoProcesamiento
    {
        public string Metodo { get; set; }
        public long TiempoMs { get; set; }
        public double TiempoSeg => TiempoMs / 1000.0;
        public int ArchivosProcesados { get; set; }
        public int PalabrasUnicas { get; set; }
        public int PalabrasTotal { get; set; }
        public double PalabrasPorSegundo => TiempoSeg > 0 ? PalabrasTotal / TiempoSeg : 0;
        public Dictionary<string, List<OrigenPalabra>> PalabrasConOrigen { get; set; }
        public DateTime FechaEjecucion { get; set; }
    }

    // Métricas de rendimiento comparando secuencial vs paralelo
    public class MetricasProcesamiento
    {
        public int ArchivosTotal { get; set; }
        public int ArchivosProcesados { get; set; }
        public int PalabrasUnicas { get; set; }
        public int PalabrasTotal { get; set; }
        public long TiempoSecuencialMs { get; set; }
        public long TiempoParaleloMs { get; set; }

        // Cálculos automáticos de rendimiento
        public double TiempoSecuencialSeg => TiempoSecuencialMs / 1000.0;
        public double TiempoParaleloSeg => TiempoParaleloMs / 1000.0;
        public double Speedup => TiempoParaleloMs > 0 ? (double)TiempoSecuencialMs / TiempoParaleloMs : 0;
        public double Eficiencia => Speedup / Environment.ProcessorCount;
        public double PalabrasSecuencialPorSeg => TiempoSecuencialSeg > 0 ? PalabrasTotal / TiempoSecuencialSeg : 0;
        public double PalabrasParaleloPorSeg => TiempoParaleloSeg > 0 ? PalabrasTotal / TiempoParaleloSeg : 0;

        // Evaluación del speedup obtenido
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
    }

    // Estado individual de procesamiento de cada archivo
    public class EstadoArchivo
    {
        public string NombreArchivo { get; set; }
        public string RutaCompleta { get; set; }
        public string Estado { get; set; }
        public int PalabrasProcesadas { get; set; }
        public long TamañoBytes { get; set; }
        public string TamañoLegible => FormatearTamaño(TamañoBytes);

        // Convierte bytes a formato legible (KB, MB)
        private string FormatearTamaño(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }

    // PARA ANGEL: Contexto de palabras para modelo predictivo
    public class ContextoPalabra
    {
        public string PalabraActual { get; set; }
        public string PalabraAnterior { get; set; }
        public string ArchivoOrigen { get; set; }
        public int Posicion { get; set; }
    }

    , "siempre", "ejemplo", "tiempo", "casos"
        };
// CLASE PRINCIPAL - CANDY: Procesamiento paralelo de archivos de texto
    public class ProcesadorArchivos
    {
        // Estructuras thread-safe para procesamiento paralelo
        private readonly ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>> _palabrasConOrigen;
        private readonly ConcurrentBag<ContextoPalabra> _contextoPalabras;
        private readonly List<EstadoArchivo> _estadoArchivos;
        private readonly object _lockEstados = new object();
        ConcurrentDictionary<string, List<string>> indice = new ConcurrentDictionary<string, List<string>>();


        // Lista de palabras que no aportan valor semántico (stop words en español)
        private static readonly HashSet<string> PalabrasVacias = new HashSet<string>
        {
             "a", "ante", "bajo", "cabe", "con", "contra", "de", "desde", "durante",
            "en", "entre", "hacia", "hasta", "mediante", "para", "por",
            "según", "sin", "so", "sobre", "tras",
            "el", "la", "los", "las", "un", "una", "unos", "unas",
            "y", "o", "u", "que", "como", "es", "son", "ser", "fue", "fueron",
            "este", "esta", "estos", "estas", "se", "su", "sus", "lo", "le", "les", "del", "al",
            "más", "pero", "sus", "les", "una", "los", "las", "son", "han", "muy", "hay",
            "sin", "está", "todo", "también", "donde", "cuando", "sobre", "mientras",
            "otro", "otros", "otra", "otras", "mismo", "misma", "será", "pueden",
            "solo", "cada", "tiene", "hacer", "después", "forma", "bien", "aquí",
            "tanto", "estado"
        // Constructor: inicializa todas las estructuras de datos
        public ProcesadorArchivos()
        {
            _palabrasConOrigen = new ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>>();
            _contextoPalabras = new ConcurrentBag<ContextoPalabra>();
            _estadoArchivos = new List<EstadoArchivo>();
            _frecuenciaPalabras = new ConcurrentDictionary<string, int>();
        }

        // PARA JASON: Método principal para procesar archivos subidos desde la web
        public async Task<MetricasProcesamiento> ProcesarArchivosSubidosAsync(IFormFile[] archivosSubidos, string carpetaTemporal = "uploads")
        {
            try
            {
                // Crear carpeta temporal si no existe
                if (!Directory.Exists(carpetaTemporal))
                    Directory.CreateDirectory(carpetaTemporal);

                var rutasGuardadas = new List<string>();

                // Guardar todos los archivos válidos al disco
                foreach (var archivo in archivosSubidos)
                {
                    if (archivo.Length > 0 && EsArchivoValido(archivo.FileName))
                    {
                        var nombreSeguro = LimpiarNombreArchivo(archivo.FileName);
                        var rutaArchivo = Path.Combine(carpetaTemporal, nombreSeguro);

                        using (var stream = new FileStream(rutaArchivo, FileMode.Create))
                        {
                            await archivo.CopyToAsync(stream);
                        }

                        rutasGuardadas.Add(rutaArchivo);
                    }
                }

                // Si no hay archivos válidos, retornar métricas vacías
                if (rutasGuardadas.Count == 0)
                {
                    return new MetricasProcesamiento { ArchivosTotal = 0 };
                }

                // Procesar archivos y comparar rendimiento secuencial vs paralelo
                var metricas = await ProcesarCarpetaCompletaAsync(rutasGuardadas.ToArray());

                // Mostrar análisis detallado en consola
                MostrarAnalisisRendimiento(metricas);

                return metricas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando archivos: {ex.Message}");
                return new MetricasProcesamiento { ArchivosTotal = 0 };
            }
        }

        // Comparativa completa: ejecuta procesamiento secuencial y paralelo
        public async Task<MetricasProcesamiento> ProcesarCarpetaCompletaAsync(string[] archivos)
        {
            if (archivos.Length == 0)
                return new MetricasProcesamiento { ArchivosTotal = 0 };

            Console.WriteLine($"\nIniciando análisis con {archivos.Length} archivos...\n");

            // Primera pasada: procesamiento secuencial (uno por uno)
            Console.WriteLine("PROCESAMIENTO SECUENCIAL...");
            var resultadoSecuencial = await ProcesarSecuencialAsync(archivos);

            // Limpiar estructuras para segunda prueba
            LimpiarDatos();

            // Segunda pasada: procesamiento paralelo (todos los cores)
            Console.WriteLine("PROCESAMIENTO PARALELO...");
            var resultadoParalelo = await ProcesarParaleloAsync(archivos);

            return GenerarMetricas(resultadoSecuencial, resultadoParalelo);
        }

        // Procesamiento secuencial: procesa archivos uno por uno
        private async Task<ResultadoProcesamiento> ProcesarSecuencialAsync(string[] rutasArchivos)
        {
            var stopwatch = Stopwatch.StartNew();
            var palabrasSecuencial = new Dictionary<string, List<OrigenPalabra>>();
            int archivosProcesados = 0;
            int palabrasTotal = 0;

            // Procesar cada archivo en orden secuencial
            foreach (var rutaArchivo in rutasArchivos)
            {
                try
                {
                    ActualizarEstadoArchivo(rutaArchivo, "Procesando...");

                    // Extraer palabras del archivo actual
                    var resultado = await ExtraerPalabrasDelArchivoAsync(rutaArchivo);
                    var nombreArchivo = Path.GetFileName(rutaArchivo);

                    // Guardar palabras con su origen
                    foreach (var palabra in resultado.Palabras)
                    {
                        if (!palabrasSecuencial.ContainsKey(palabra.Key))
                            palabrasSecuencial[palabra.Key] = new List<OrigenPalabra>();

                        palabrasSecuencial[palabra.Key].Add(new OrigenPalabra
                        {
                            ArchivoOrigen = nombreArchivo,
                            Frecuencia = palabra.Value,
                            FechaProcesamiento = DateTime.Now,
                            UbicacionEnTexto = "Procesamiento secuencial"
                        });
                    }

                    palabrasTotal += resultado.Palabras.Values.Sum();
                    archivosProcesados++;
                    ActualizarEstadoArchivo(rutaArchivo, "Procesado", resultado.Palabras.Values.Sum());
                }
                catch (Exception ex)
                {
                    ActualizarEstadoArchivo(rutaArchivo, $"Error: {ex.Message}");
                }
            }

            stopwatch.Stop();

            return new ResultadoProcesamiento
            {
                Metodo = "Secuencial",
                TiempoMs = stopwatch.ElapsedMilliseconds,
                ArchivosProcesados = archivosProcesados,
                PalabrasUnicas = palabrasSecuencial.Count,
                PalabrasTotal = palabrasTotal,
                PalabrasConOrigen = palabrasSecuencial,
                FechaEjecucion = DateTime.Now
            };
        }

        // Procesamiento paralelo: usa todos los cores del procesador simultáneamente
        private async Task<ResultadoProcesamiento> ProcesarParaleloAsync(string[] rutasArchivos)
        {
            var stopwatch = Stopwatch.StartNew();
            _palabrasConOrigen.Clear();
            int archivosProcesados = 0;
            int palabrasTotal = 0;

            // Configurar paralelismo máximo según cores disponibles
            var paralelismoOpciones = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Task.Run(() =>
            {
                // Procesar todos los archivos en paralelo
                Parallel.ForEach(rutasArchivos, paralelismoOpciones, rutaArchivo =>
                {
                    try
                    {
                        ActualizarEstadoArchivo(rutaArchivo, "Procesando...");

                        // Extraer palabras del archivo actual
                        var resultado = ExtraerPalabrasDelArchivoAsync(rutaArchivo).Result;
                        var nombreArchivo = Path.GetFileName(rutaArchivo);

                        // Agregar palabras a estructuras thread-safe
                        // Agregar palabras a estructuras thread-safe, filtrando stopwords
                        foreach (var palabra in resultado.Palabras
                            .Where(p => !PalabrasVacias.Contains(p.Key) && p.Key.Length > 1))
                        {
                            _palabrasConOrigen.AddOrUpdate(
                                palabra.Key,
                                new ConcurrentBag<OrigenPalabra>
                                {
                                    new OrigenPalabra
                                    {
                                        ArchivoOrigen = nombreArchivo,
                                        Frecuencia = palabra.Value,
                                        FechaProcesamiento = DateTime.Now,
                                        UbicacionEnTexto = "Procesamiento paralelo"
                                    }
                                },
                                (key, existingBag) =>
                                {
                                    existingBag.Add(new OrigenPalabra
                                    {
                                        ArchivoOrigen = nombreArchivo,
                                        Frecuencia = palabra.Value,
                                        FechaProcesamiento = DateTime.Now,
                                        UbicacionEnTexto = "Procesamiento paralelo"
                                    });
                                    return existingBag;
                                });

                            // Actualizar frecuencias totales de forma thread-safe
                            _frecuenciaPalabras.AddOrUpdate(palabra.Key, palabra.Value,
                                (key, oldValue) => oldValue + palabra.Value);
                        }


                        // Agregar contextos para ANGEL
                        foreach (var contexto in resultado.Contextos)
                        {
                            _contextoPalabras.Add(contexto);
                        }

                        // Actualizar contadores de forma thread-safe
                        Interlocked.Add(ref palabrasTotal, resultado.Palabras.Values.Sum());
                        Interlocked.Increment(ref archivosProcesados);

                        ActualizarEstadoArchivo(rutaArchivo, "Procesado", resultado.Palabras.Values.Sum());
                    }
                    catch (Exception ex)
                    {
                        ActualizarEstadoArchivo(rutaArchivo, $"Error: {ex.Message}");
                    }
                });
            });

            stopwatch.Stop();

            // Convertir estructuras concurrentes a formato estándar
            var palabrasConOrigenDict = _palabrasConOrigen.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToList()
            );

            var todasLasPalabras = palabrasConOrigenDict.Keys.ToList();
            var modelo = new ModeloPrediccion();
            modelo.EntrenarModelo(todasLasPalabras);

            var sugerencias = modelo.PredecirSiguientePalabra("la programación", 3);
            Console.WriteLine("Sugerencias: " + string.Join(", ", sugerencias));

            return new ResultadoProcesamiento
            {
                Metodo = "Paralelo",
                TiempoMs = stopwatch.ElapsedMilliseconds,
                ArchivosProcesados = archivosProcesados,
                PalabrasUnicas = palabrasConOrigenDict.Count,
                PalabrasTotal = palabrasTotal,
                PalabrasConOrigen = palabrasConOrigenDict,
                FechaEjecucion = DateTime.Now
            };
        }

        // Extrae texto de diferentes tipos de archivo (TXT, DOCX, PDF)
        private async Task<(Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos)> ExtraerPalabrasDelArchivoAsync(string rutaArchivo)
        {
            var extension = Path.GetExtension(rutaArchivo).ToLower();
            string contenido = "";

            try
            {
                // Procesar según el tipo de archivo
                switch (extension)
                {
                    case ".txt":
                        contenido = await File.ReadAllTextAsync(rutaArchivo, Encoding.UTF8);
                        break;
                    case ".docx":
                        contenido = await ExtraerTextoDeDocxAsync(rutaArchivo);
                        break;
                    case ".pdf":
                        contenido = await ExtraerTextoDePdfAsync(rutaArchivo);
                        break;
                    default:
                        throw new NotSupportedException($"Formato no soportado: {extension}");
                }

                // Tokenizar y crear contextos
                var resultado = TokenizarYContarPalabrasConContexto(contenido, Path.GetFileName(rutaArchivo));
                return (resultado.Palabras, resultado.Contextos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando {Path.GetFileName(rutaArchivo)}: {ex.Message}");
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());
            }
        }

        // Extrae texto de archivos Word (.docx)
        private async Task<string> ExtraerTextoDeDocxAsync(string rutaArchivo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var doc = WordprocessingDocument.Open(rutaArchivo, false);
                    var body = doc.MainDocumentPart?.Document?.Body;
                    return body?.InnerText ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en DOCX: {ex.Message}");
                    return string.Empty;
                }
            });
        }

        // Extrae texto de archivos PDF
        private async Task<string> ExtraerTextoDePdfAsync(string rutaArchivo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var pdfReader = new PdfReader(rutaArchivo);
                    using var pdfDocument = new PdfDocument(pdfReader);
                    var contenido = new StringBuilder();

                    // Procesar cada página del PDF
                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                        var pagina = pdfDocument.GetPage(i);
                        var textoPagina = PdfTextExtractor.GetTextFromPage(pagina);
                        contenido.AppendLine(textoPagina);
                    }

                    return contenido.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en PDF: {ex.Message}");
                    return string.Empty;
                }
            });
        }

        // Convierte texto en palabras limpias y crea contextos para predicción
        private (Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos) TokenizarYContarPalabrasConContexto(string texto, string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());

            // Limpiar texto: solo letras españolas y espacios
            var textoLimpio = Regex.Replace(texto.ToLower(), @"[^a-záéíóúüñ\s]", " ");

            // Dividir en palabras válidas (mínimo 3 caracteres, no stop words)
            var palabras = texto
            .ToLower()
            .Split(new[] { ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '\"', '(', ')', '[', ']', '{', '}' },
            StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !PalabrasVacias.Contains(p) && p.Length > 1);


            // Contar frecuencias de cada palabra
            var conteos = new Dictionary<string, int>();
            foreach (var palabra in palabras)
            {
                conteos[palabra] = conteos.ContainsKey(palabra) ? conteos[palabra] + 1 : 1;
            }

            // Crear contextos palabra-anterior → palabra-actual para ANGEL
            var contextos = new List<ContextoPalabra>();
            for (int i = 1; i < palabras.Length; i++)
            {
                contextos.Add(new ContextoPalabra
                {
                    PalabraAnterior = palabras[i - 1],
                    PalabraActual = palabras[i],
                    ArchivoOrigen = nombreArchivo,
                    Posicion = i
                });
            }

            return (conteos, contextos);
        }

        // Muestra análisis detallado de rendimiento en consola
        private void MostrarAnalisisRendimiento(MetricasProcesamiento metricas)
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("     ANÁLISIS DE RENDIMIENTO PARALELO");
            Console.WriteLine(new string('=', 50));

            Console.WriteLine($"Tiempo Secuencial: {metricas.TiempoSecuencialSeg:F2}s");
            Console.WriteLine($"Tiempo Paralelo: {metricas.TiempoParaleloSeg:F2}s");
            Console.WriteLine($"Speedup: {metricas.Speedup:F2}x  {metricas.EvaluacionSpeedup}");
            Console.WriteLine($"Eficiencia: {metricas.Eficiencia * 100:F1}%");
            Console.WriteLine($"Palabras/seg: {metricas.PalabrasSecuencialPorSeg:N0} → {metricas.PalabrasParaleloPorSeg:N0}");
            Console.WriteLine($"Archivos: {metricas.ArchivosProcesados}/{metricas.ArchivosTotal} procesados");
            Console.WriteLine($"Palabras: {metricas.PalabrasUnicas:N0} únicas, {metricas.PalabrasTotal:N0} total");

            Console.WriteLine(new string('=', 50) + "\n");
        }

        // =============== MÉTODOS PARA COMPAÑEROS ===============

        // PARA ANGEL: Contextos de palabras para el modelo predictivo
        public List<ContextoPalabra> ObtenerContextosPalabras()
        {
            return _contextoPalabras.ToList();
        }

        // PARA ANGEL: Frecuencias totales de todas las palabras
        public Dictionary<string, int> ObtenerFrecuenciaPalabras()
        {
            return _frecuenciaPalabras.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        // PARA JASON: Datos formateados para mostrar en la interfaz web
        public object ObtenerDatosParaInterfaz()
        {
            var totalPalabras = _palabrasConOrigen.Sum(p => p.Value.Sum(o => o.Frecuencia));

            return new
            {
                TotalPalabrasUnicas = _palabrasConOrigen.Count,
                TotalPalabras = totalPalabras,
                TotalContextos = _contextoPalabras.Count,
                ArchivosEstado = _estadoArchivos.Select(a => new {
                    Nombre = a.NombreArchivo,
                    Estado = a.Estado,
                    Palabras = a.PalabrasProcesadas,
                    Tamaño = a.TamañoLegible
                }),
                PalabrasMasComunes = _frecuenciaPalabras
                    .OrderByDescending(kvp => kvp.Value)
                    .Take(20)
                    .Select(kvp => new {
                        Palabra = kvp.Key,
                        Frecuencia = kvp.Value,
                        Origenes = _palabrasConOrigen.TryGetValue(kvp.Key, out var origenes)
                            ? origenes.Select(o => o.ArchivoOrigen).Distinct().ToList()
                            : new List<string>()
                    })
            };
        }

        // PARA JASON: Buscar palabras por prefijo para autocompletado
        public List<(string Palabra, string ArchivoOrigen, int Frecuencia)> BuscarPalabrasPorPrefijo(string prefijo, int limite = 10)
        {
            if (string.IsNullOrWhiteSpace(prefijo))
                return new List<(string, string, int)>();

            var prefijoLimpio = prefijo.ToLower().Trim();

            return _palabrasConOrigen
                .Where(p => p.Key.StartsWith(prefijoLimpio))
                .SelectMany(p => p.Value.Select(o => (p.Key, o.ArchivoOrigen, o.Frecuencia)))
                .OrderByDescending(x => x.Frecuencia)
                .Take(limite)
                .ToList();
        }

        // PARA JASON: Obtener archivos de origen de una palabra específica
        public List<string> ObtenerOrigenDePalabra(string palabra)
        {
            if (_palabrasConOrigen.TryGetValue(palabra.ToLower(), out var origenes))
            {
                return origenes
                    .Select(o => $"{o.ArchivoOrigen} ({o.Frecuencia} veces)")
                    .Distinct()
                    .ToList();
            }
            return new List<string>();
        }

        // PARA JASON: Todas las palabras procesadas con sus orígenes
        public Dictionary<string, List<OrigenPalabra>> ObtenerTodasLasPalabras()
        {
            return _palabrasConOrigen.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToList()
            );
        }

        // PARA ISMER: Estado actual de todos los archivos (para testing)
        public List<EstadoArchivo> ObtenerEstadoArchivos()
        {
            lock (_lockEstados)
            {
                return new List<EstadoArchivo>(_estadoArchivos);
            }
        }

        // =============== MÉTODOS AUXILIARES ===============

        // Actualiza el estado de procesamiento de un archivo específico
        private void ActualizarEstadoArchivo(string rutaArchivo, string estado, int palabrasProcesadas = 0)
        {
            var nombreArchivo = Path.GetFileName(rutaArchivo);
            var tamañoBytes = File.Exists(rutaArchivo) ? new FileInfo(rutaArchivo).Length : 0;

            lock (_lockEstados)
            {
                var estadoExistente = _estadoArchivos.FirstOrDefault(a => a.NombreArchivo == nombreArchivo);

                if (estadoExistente == null)
                {
                    _estadoArchivos.Add(new EstadoArchivo
                    {
                        NombreArchivo = nombreArchivo,
                        RutaCompleta = rutaArchivo,
                        Estado = estado,
                        PalabrasProcesadas = palabrasProcesadas,
                        TamañoBytes = tamañoBytes
                    });
                }
                else
                {
                    estadoExistente.Estado = estado;
                    estadoExistente.PalabrasProcesadas = palabrasProcesadas;
                    estadoExistente.TamañoBytes = tamañoBytes;
                }
            }
        }

        // Genera métricas combinando resultados secuencial y paralelo
        private MetricasProcesamiento GenerarMetricas(ResultadoProcesamiento secuencial, ResultadoProcesamiento paralelo)
        {
            return new MetricasProcesamiento
            {
                ArchivosTotal = _estadoArchivos.Count,
                ArchivosProcesados = _estadoArchivos.Count(a => a.Estado == "Procesado"),
                PalabrasUnicas = paralelo?.PalabrasUnicas ?? 0,
                PalabrasTotal = paralelo?.PalabrasTotal ?? 0,
                TiempoSecuencialMs = secuencial?.TiempoMs ?? 0,
                TiempoParaleloMs = paralelo?.TiempoMs ?? 0,
                EstadoArchivos = new List<EstadoArchivo>(_estadoArchivos)
            };
        }

        // Verifica si una palabra está en la lista de stop words
        private bool EsPalabraVacia(string palabra)
        {
            return PalabrasVacias.Contains(palabra);
        }

        // Verifica si el archivo tiene extensión válida
        private bool EsArchivoValido(string nombreArchivo)
        {
            var extensionesValidas = new[] { ".txt", ".pdf", ".docx" };
            return extensionesValidas.Contains(Path.GetExtension(nombreArchivo).ToLower());a
        }

        // Limpia nombres de archivo de caracteres peligrosos
        private string LimpiarNombreArchivo(string nombreArchivo)
        {
            var caracteresPeligrosos = new[] { "<", ">", ":", "\"", "|", "?", "*" };
            var nombreLimpio = nombreArchivo;

            foreach (var caracter in caracteresPeligrosos)
            {
                nombreLimpio = nombreLimpio.Replace(caracter, "_");
            }

            return nombreLimpio;
        }

        // Limpia todas las estructuras de datos para nueva ejecución
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
    }
}
// Fin de la clase ProcesadorArchivos