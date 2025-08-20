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
        // Número total de procesadores lógicos disponibles en el sistema.
        public int ProcessorsDisponibles { get; set; }
        // Número de procesadores recomendados para la tarea. Un valor común es el 75% del total para dejar recursos libres.
        public int ProcessorsRecomendados { get; set; }
        // Número óptimo de procesadores, generalmente igual a los disponibles, para máxima paralelización.
        public int ProcessorsOptimos { get; set; }
        // Número máximo de procesadores para pruebas intensivas
        public int ProcessorsMaximoPrueba { get; set; }
        // Clasificación del procesador basada en el número de núcleos.
        public string TipoProcesador { get; set; } = string.Empty;
        // Recomendación general del hardware en base a los procesadores.
        public string RecomendacionHardware { get; set; } = string.Empty;
        // No se utiliza directamente en el código, pero podría usarse para mostrar la carga actual del CPU.
        public double CargaCPU { get; set; }
        // Razón detrás de la recomendación de cores.
        public string JustificacionRecomendacion { get; set; } = string.Empty;
    }

    // Estructura para almacenar métricas de rendimiento más detalladas.
    public class MetricasEspecificas
    {
        // Velocidad de procesamiento en archivos por segundo.
        public double ArchivosPromedioSegundo { get; set; }
        // Porcentaje de palabras únicas con respecto al total, una medida de la redundancia del texto.
        public double EficienciaMemoria { get; set; }
        // Tasa de procesamiento de palabras por segundo.
        public double ThroughputPalabras { get; set; }
        // Un valor que podría capturar el número máximo de hilos activos al mismo tiempo.
        public int PicosMaximosParalelismo { get; set; }
        // Tiempo promedio que tarda en procesar un solo archivo.
        public double TiempoRespuestaPromedio { get; set; }
        // Tiempo que tarda la primera parte del proceso en ejecutarse.
        public double LatenciaInicial { get; set; }
        // Resumen cualitativo del rendimiento.
        public string RendimientoGeneral { get; set; } = string.Empty;
    }

    // Estructura para guardar el resultado de una prueba de rendimiento individual.
    public class ResultadoPrueba
    {
        // Número de procesadores utilizados en la prueba.
        public int NumeroProcessors { get; set; }
        // Nombre descriptivo de la prueba.
        public string NombrePrueba { get; set; } = string.Empty;
        // Tiempo de ejecución en milisegundos.
        public long TiempoMs { get; set; }
        // La mejora de velocidad (speedup) en comparación con la versión secuencial.
        public double Speedup { get; set; }
        // La eficiencia del paralelismo. Speedup / Número de cores. Idealmente 1.
        public double Eficiencia { get; set; }
        // Cantidad total de archivos procesados en esta prueba.
        public int ArchivosTotal { get; set; }
        // Parámetros adicionales de la prueba.
        public string ParametrosPrueba { get; set; } = string.Empty;
        // Marca de tiempo de la prueba
        public DateTime FechaPrueba { get; set; }
        // Uso de memoria durante la prueba
        public long MemoriaUtilizada { get; set; }
    }

    // Estructura que detalla de dónde proviene una palabra.
    public class OrigenPalabra
    {
        // Nombre del archivo de donde se extrajo la palabra.
        public string ArchivoOrigen { get; set; } = string.Empty;
        // La frecuencia de la palabra dentro de ese archivo.
        public int Frecuencia { get; set; }
        // La fecha y hora en que se procesó el archivo.
        public DateTime FechaProcesamiento { get; set; }
        // Posiblemente la ubicación en el texto (no implementado en este código).
        public string UbicacionEnTexto { get; set; } = string.Empty;
    }

    // Estructura que resume los resultados de un procesamiento completo (secuencial o paralelo).
    public class ResultadoProcesamiento
    {
        // El método de procesamiento, ej. "Secuencial" o "Paralelo".
        public string Metodo { get; set; } = string.Empty;
        // El tiempo total de procesamiento en milisegundos.
        public long TiempoMs { get; set; }
        // Propiedad calculada que convierte el tiempo a segundos.
        public double TiempoSeg => TiempoMs / 1000.0;
        // El número de archivos que se procesaron exitosamente.
        public int ArchivosProcesados { get; set; }
        // El número de palabras únicas encontradas.
        public int PalabrasUnicas { get; set; }
        // El número total de palabras encontradas, incluyendo repeticiones.
        public int PalabrasTotal { get; set; }
        // Propiedad calculada que mide el rendimiento en palabras por segundo.
        public double PalabrasPorSegundo => TiempoSeg > 0 ? PalabrasTotal / TiempoSeg : 0;
        // Un diccionario que asocia cada palabra única con una lista de sus orígenes.
        public Dictionary<string, List<OrigenPalabra>> PalabrasConOrigen { get; set; } = new Dictionary<string, List<OrigenPalabra>>();
        // La fecha y hora en que se realizó la ejecución.
        public DateTime FechaEjecucion { get; set; }
        // Memoria utilizada durante el procesamiento
        public long MemoriaUtilizada { get; set; }
    }

    // Clase que consolida todas las métricas y resultados para el informe final.
    public class MetricasProcesamiento
    {
        // Total de archivos iniciales.
        public int ArchivosTotal { get; set; }
        // Archivos procesados con éxito.
        public int ArchivosProcesados { get; set; }
        // Total de palabras únicas.
        public int PalabrasUnicas { get; set; }
        // Total de palabras (incluyendo repeticiones).
        public int PalabrasTotal { get; set; }
        // Tiempo de ejecución de la prueba secuencial.
        public long TiempoSecuencialMs { get; set; }
        // Tiempo de ejecución de la prueba paralela.
        public long TiempoParaleloMs { get; set; }
        // Propiedades calculadas para los tiempos en segundos.
        public double TiempoSecuencialSeg => TiempoSecuencialMs / 1000.0;
        public double TiempoParaleloSeg => TiempoParaleloMs / 1000.0;
        // El factor de mejora de velocidad.
        public double Speedup => TiempoParaleloMs > 0 ? (double)TiempoSecuencialMs / TiempoParaleloMs : 0;
        // Eficiencia del paralelismo.
        public double Eficiencia => Speedup / Environment.ProcessorCount;
        // Tasas de procesamiento en palabras por segundo para ambos métodos.
        public double PalabrasSecuencialPorSeg => TiempoSecuencialSeg > 0 ? PalabrasTotal / TiempoSecuencialSeg : 0;
        public double PalabrasParaleloPorSeg => TiempoParaleloSeg > 0 ? PalabrasTotal / TiempoParaleloSeg : 0;
        // Propiedad que evalúa cualitativamente el speedup obtenido.
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
        // Listas para almacenar los estados de los archivos y los resultados de las pruebas.
        public List<EstadoArchivo> EstadoArchivos { get; set; } = new List<EstadoArchivo>();
        public AnalisisProcesadores AnalisisProcesadores { get; set; } = new AnalisisProcesadores();
        public MetricasEspecificas MetricasEspecificas { get; set; } = new MetricasEspecificas();
        public List<ResultadoPrueba> ResultadosPruebas { get; set; } = new List<ResultadoPrueba>();
        // Información adicional para las métricas
        public DateTime FechaAnalisis { get; set; }
        public string VersionSistema { get; set; } = string.Empty;
        public long MemoriaTotal { get; set; }
        public string ConfiguracionOptima { get; set; } = string.Empty;
    }

    // Estructura para registrar el estado de cada archivo procesado.
    public class EstadoArchivo
    {
        // Nombre del archivo.
        public string NombreArchivo { get; set; } = string.Empty;
        // Ruta completa del archivo en el sistema.
        public string RutaCompleta { get; set; } = string.Empty;
        // Estado actual del procesamiento del archivo.
        public string Estado { get; set; } = string.Empty;
        // Número de palabras procesadas.
        public int PalabrasProcesadas { get; set; }
        // Tamaño del archivo en bytes.
        public long TamañoBytes { get; set; }
        // Propiedad calculada que devuelve el tamaño en un formato legible (KB, MB).
        public string TamañoLegible => FormatearTamaño(TamañoBytes);

        // Método auxiliar para convertir bytes a un formato legible.
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
        // La palabra que se está analizando.
        public string PalabraActual { get; set; } = string.Empty;
        // La palabra que la precede.
        public string PalabraAnterior { get; set; } = string.Empty;
        // Archivo de origen.
        public string ArchivoOrigen { get; set; } = string.Empty;
        // Posición de la palabra en el texto.
        public int Posicion { get; set; }
    }

    // Clase para manejar el guardado de métricas
    public static class GuardadorMetricas
    {
        // Método para encontrar la carpeta raíz del proyecto donde está la carpeta metrics existente
        private static string EncontrarCarpetaRaizProyecto()
        {
            var directorioActual = new DirectoryInfo(Directory.GetCurrentDirectory());

            Console.WriteLine($"Buscando carpeta raíz desde: {directorioActual.FullName}");

            // Buscar hacia arriba hasta encontrar una carpeta que ya tenga la carpeta "metrics"
            while (directorioActual != null)
            {
                var carpetaMetrics = Path.Combine(directorioActual.FullName, "metrics");
                if (Directory.Exists(carpetaMetrics))
                {
                    Console.WriteLine($"Carpeta raíz encontrada por carpeta metrics existente: {directorioActual.FullName}");
                    return directorioActual.FullName;
                }

                // Si encontramos una carpeta src Y hay una carpeta metrics al mismo nivel
                if (Directory.Exists(Path.Combine(directorioActual.FullName, "src")))
                {
                    var metricsEnRaiz = Path.Combine(directorioActual.FullName, "metrics");
                    if (Directory.Exists(metricsEnRaiz))
                    {
                        Console.WriteLine($"Carpeta raíz encontrada por src + metrics: {directorioActual.FullName}");
                        return directorioActual.FullName;
                    }
                }

                // Si el directorio actual contiene "buscador" en su nombre Y tiene metrics
                if (directorioActual.Name.ToLower().Contains("buscador"))
                {
                    var metricsAqui = Path.Combine(directorioActual.FullName, "metrics");
                    if (Directory.Exists(metricsAqui))
                    {
                        Console.WriteLine($"Carpeta raíz encontrada por nombre buscador + metrics: {directorioActual.FullName}");
                        return directorioActual.FullName;
                    }
                }

                directorioActual = directorioActual.Parent;
            }

            // Si no encontramos la carpeta metrics existente, crear en el directorio actual
            var carpetaActual = Directory.GetCurrentDirectory();
            Console.WriteLine($"No se encontró carpeta metrics existente. Usando directorio actual: {carpetaActual}");
            return carpetaActual;
        }

        // Ruta de métricas que usa la carpeta existente
        private static readonly Lazy<string> _carpetaMetricas = new Lazy<string>(() =>
        {
            var raiz = EncontrarCarpetaRaizProyecto();
            var carpetaMetricas = Path.Combine(raiz, "metrics");

            // Solo crear la carpeta si no existe (no debería pasar si la lógica de arriba funciona)
            if (!Directory.Exists(carpetaMetricas))
            {
                Directory.CreateDirectory(carpetaMetricas);
                Console.WriteLine($"Carpeta metrics creada: {carpetaMetricas}");
            }
            else
            {
                Console.WriteLine($"Usando carpeta metrics existente: {carpetaMetricas}");
            }

            return carpetaMetricas;
        });

        private static string CarpetaMetricas => _carpetaMetricas.Value;

        private static readonly JsonSerializerOptions OpcionesJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static async Task GuardarMetricasAsync(MetricasProcesamiento metricas)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var nombreArchivo = $"analisis_rendimiento_{timestamp}.json";
                var rutaCompleta = Path.Combine(CarpetaMetricas, nombreArchivo);

                // Serializar y guardar las métricas completas
                var jsonMetricas = JsonSerializer.Serialize(metricas, OpcionesJson);
                await File.WriteAllTextAsync(rutaCompleta, jsonMetricas, Encoding.UTF8);

                // Crear también un resumen en CSV
                await GuardarResumenCsvAsync(metricas, timestamp);

                // Crear archivo de configuración recomendada
                await GuardarConfiguracionRecomendadaAsync(metricas, timestamp);

                Console.WriteLine($"Métricas JSON guardadas: {rutaCompleta}");
                Console.WriteLine($"Resumen CSV guardado: {Path.Combine(CarpetaMetricas, $"resumen_{timestamp}.csv")}");
                Console.WriteLine($"Configuración guardada: {Path.Combine(CarpetaMetricas, $"config_recomendada_{timestamp}.txt")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando métricas: {ex.Message}");
                Console.WriteLine($"Ruta intentada: {CarpetaMetricas}");
            }
        }

        private static async Task GuardarResumenCsvAsync(MetricasProcesamiento metricas, string timestamp)
        {
            var csvPath = Path.Combine(CarpetaMetricas, $"resumen_{timestamp}.csv");
            var csv = new StringBuilder();

            csv.AppendLine("Cores,TiempoMs,Speedup,Eficiencia,ArchivosTotal,PalabrasPorSegundo");

            foreach (var prueba in metricas.ResultadosPruebas.OrderBy(p => p.NumeroProcessors))
            {
                var palabrasPorSeg = prueba.TiempoMs > 0 ? (metricas.PalabrasTotal / (prueba.TiempoMs / 1000.0)) : 0;
                csv.AppendLine($"{prueba.NumeroProcessors},{prueba.TiempoMs},{prueba.Speedup:F2},{prueba.Eficiencia:F3},{prueba.ArchivosTotal},{palabrasPorSeg:F0}");
            }

            await File.WriteAllTextAsync(csvPath, csv.ToString(), Encoding.UTF8);
        }

        private static async Task GuardarConfiguracionRecomendadaAsync(MetricasProcesamiento metricas, string timestamp)
        {
            var configPath = Path.Combine(CarpetaMetricas, $"config_recomendada_{timestamp}.txt");
            var config = new StringBuilder();

            var mejorPrueba = metricas.ResultadosPruebas.OrderByDescending(p => p.Speedup).FirstOrDefault();

            config.AppendLine("=== CONFIGURACIÓN RECOMENDADA ===");
            config.AppendLine($"Fecha del análisis: {metricas.FechaAnalisis:yyyy-MM-dd HH:mm:ss}");
            config.AppendLine($"Procesadores disponibles: {metricas.AnalisisProcesadores.ProcessorsDisponibles}");
            config.AppendLine($"Tipo de procesador: {metricas.AnalisisProcesadores.TipoProcesador}");
            config.AppendLine();

            if (mejorPrueba != null)
            {
                config.AppendLine("=== MEJOR CONFIGURACIÓN ENCONTRADA ===");
                config.AppendLine($"Cores óptimos: {mejorPrueba.NumeroProcessors}");
                config.AppendLine($"Speedup obtenido: {mejorPrueba.Speedup:F2}x");
                config.AppendLine($"Eficiencia: {mejorPrueba.Eficiencia * 100:F1}%");
                config.AppendLine($"Tiempo de procesamiento: {mejorPrueba.TiempoMs}ms");
                config.AppendLine();
            }

            config.AppendLine("=== MÉTRICAS DE RENDIMIENTO ===");
            config.AppendLine($"Archivos procesados: {metricas.ArchivosProcesados}/{metricas.ArchivosTotal}");
            config.AppendLine($"Palabras únicas encontradas: {metricas.PalabrasUnicas:N0}");
            config.AppendLine($"Palabras totales: {metricas.PalabrasTotal:N0}");
            config.AppendLine($"Throughput paralelo: {metricas.PalabrasParaleloPorSeg:N0} palabras/seg");
            config.AppendLine($"Eficiencia de memoria: {metricas.MetricasEspecificas.EficienciaMemoria:F1}%");
            config.AppendLine($"Evaluación general: {metricas.EvaluacionSpeedup}");

            await File.WriteAllTextAsync(configPath, config.ToString(), Encoding.UTF8);
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
        // Colecciones concurrentes para garantizar seguridad en el acceso desde múltiples hilos.
        private readonly ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>> _palabrasConOrigen;
        private readonly ConcurrentBag<ContextoPalabra> _contextoPalabras;
        private readonly List<EstadoArchivo> _estadoArchivos;
        private readonly ConcurrentDictionary<string, int> _frecuenciaPalabras;
        // Objeto para bloquear el acceso a _estadoArchivos y evitar conflictos de hilos.
        private readonly object _lockEstados = new object();

        // Conjunto de palabras comunes (stop words) que se ignoran en el análisis.
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

        // Constructor que inicializa las colecciones concurrentes.
        public ProcesadorArchivos()
        {
            _palabrasConOrigen = new ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>>();
            _contextoPalabras = new ConcurrentBag<ContextoPalabra>();
            _estadoArchivos = new List<EstadoArchivo>();
            _frecuenciaPalabras = new ConcurrentDictionary<string, int>();
        }

        // Método principal para iniciar el procesamiento de archivos subidos desde una interfaz web.
        public async Task<MetricasProcesamiento> ProcesarArchivosSubidosAsync(IFormFile[] archivosSubidos, string carpetaTemporal = "uploads")
        {
            try
            {
                // Crea el directorio temporal si no existe.
                Directory.CreateDirectory(carpetaTemporal);
                var rutasGuardadas = new List<string>();

                // Guarda los archivos subidos en el disco local.
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

        // Ejecuta un análisis de rendimiento completo, comparando secuencial y paralelo.
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

            // Llama al método para obtener información del procesador con configuración extendida.
            metricas.AnalisisProcesadores = AnalizarProcesadoresExtendido();

            // Ejecuta el procesamiento secuencial como línea base (baseline).
            Console.WriteLine("Ejecutando baseline secuencial...");
            var secuencial = await ProcesarSecuencialAsync(archivos);

            // Limpia las colecciones para la siguiente prueba.
            LimpiarDatos();

            // Prueba el procesamiento con configuraciones extendidas de cores.
            var configuraciones = GenerarConfiguracionesCoresExtendidas();
            Console.WriteLine($"Probando con {configuraciones.Count} configuraciones diferentes de cores...");

            foreach (var cores in configuraciones)
            {
                Console.WriteLine($"Probando con {cores} cores...");
                var inicioMemoria = GC.GetTotalMemory(false);
                var paralelo = await ProcesarParaleloAsync(archivos, cores);
                var finMemoria = GC.GetTotalMemory(false);

                // Crea un objeto de resultado para esta prueba.
                var prueba = new ResultadoPrueba
                {
                    NumeroProcessors = cores,
                    NombrePrueba = $"Paralelo-{cores}cores",
                    TiempoMs = paralelo.TiempoMs,
                    FechaPrueba = DateTime.Now,
                    MemoriaUtilizada = finMemoria - inicioMemoria,
                    // Calcula el speedup.
                    Speedup = secuencial.TiempoMs > 0 ? (double)secuencial.TiempoMs / paralelo.TiempoMs : 0,
                    // Calcula la eficiencia.
                    Eficiencia = cores > 0 ? ((double)secuencial.TiempoMs / paralelo.TiempoMs) / cores : 0,
                    ArchivosTotal = archivos.Length,
                    ParametrosPrueba = $"MaxDegreeOfParallelism={cores}, MemoriaUsada={finMemoria - inicioMemoria:N0}bytes"
                };

                metricas.ResultadosPruebas.Add(prueba);

                Console.WriteLine($"  Speedup: {prueba.Speedup:F2}x | Eficiencia: {prueba.Eficiencia * 100:F1}% | Memoria: {(finMemoria - inicioMemoria) / 1024.0 / 1024.0:F1}MB");

                LimpiarDatos();
            }

            // Selecciona el mejor resultado de las pruebas paralelas.
            var mejorPrueba = metricas.ResultadosPruebas.OrderByDescending(p => p.Speedup).First();
            Console.WriteLine($"\nMejor configuración: {mejorPrueba.NumeroProcessors} cores con speedup de {mejorPrueba.Speedup:F2}x");

            // Vuelve a ejecutar el mejor caso para obtener las métricas finales.
            var resultadoFinal = await ProcesarParaleloAsync(archivos, mejorPrueba.NumeroProcessors);

            // Completa y muestra el informe final.
            metricas = CompletarMetricas(metricas, secuencial, resultadoFinal);
            metricas.ConfiguracionOptima = $"{mejorPrueba.NumeroProcessors} cores (Speedup: {mejorPrueba.Speedup:F2}x)";

            // Guarda las métricas en la carpeta externa
            await GuardadorMetricas.GuardarMetricasAsync(metricas);

            MostrarAnalisisCompletoRendimiento(metricas);
            return metricas;
        }

        // Procesa archivos de manera secuencial (uno a uno).
        private async Task<ResultadoProcesamiento> ProcesarSecuencialAsync(string[] archivos)
        {
            // Inicia el cronómetro.
            var sw = Stopwatch.StartNew();
            var inicioMemoria = GC.GetTotalMemory(false);
            var palabras = new Dictionary<string, List<OrigenPalabra>>();
            int procesados = 0, totalPalabras = 0;

            foreach (var archivo in archivos)
            {
                try
                {
                    ActualizarEstado(archivo, "Procesando...");
                    // Extrae las palabras del archivo.
                    var resultado = await ExtraerPalabrasAsync(archivo);

                    // Acumula las palabras encontradas en un diccionario.
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

            // Devuelve el objeto con los resultados del procesamiento secuencial.
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

        // Procesa archivos en paralelo usando la librería Parallel.
        private async Task<ResultadoProcesamiento> ProcesarParaleloAsync(string[] archivos, int maxCores)
        {
            var sw = Stopwatch.StartNew();
            var inicioMemoria = GC.GetTotalMemory(false);
            _palabrasConOrigen.Clear();
            int procesados = 0, totalPalabras = 0;

            await Task.Run(() =>
            {
                // Parallel.ForEach distribuye el trabajo entre los hilos del pool.
                Parallel.ForEach(archivos, new ParallelOptions
                {
                    MaxDegreeOfParallelism = maxCores
                }, archivo =>
                {
                    try
                    {
                        ActualizarEstado(archivo, "Procesando...");
                        // ExtraerPalabrasAsync().Result bloquea el hilo hasta que la tarea asíncrona termine.
                        var resultado = ExtraerPalabrasAsync(archivo).Result;

                        // Usa ConcurrentDictionary para agregar palabras de forma segura entre hilos.
                        foreach (var palabra in resultado.Palabras.Where(p => !PalabrasVacias.Contains(p.Key) && p.Key.Length > 1))
                        {
                            // AddOrUpdate es una operación atómica para añadir o actualizar la entrada.
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

                        // Agrega los contextos para el modelo de lenguaje de forma segura.
                        foreach (var contexto in resultado.Contextos)
                            _contextoPalabras.Add(contexto);

                        // Interlocked.Add es un método seguro para incrementar una variable entre hilos.
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

            // Devuelve el objeto con los resultados del procesamiento paralelo.
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

        // Método que extrae el texto de diferentes tipos de archivos según su extensión.
        private async Task<(Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos)> ExtraerPalabrasAsync(string archivo)
        {
            var extension = Path.GetExtension(archivo).ToLower();
            string contenido = "";

            try
            {
                // Utiliza una expresión switch para seleccionar el método de extracción adecuado.
                contenido = extension switch
                {
                    ".txt" => await File.ReadAllTextAsync(archivo, Encoding.UTF8),
                    ".docx" => await ExtraerDeDocxAsync(archivo),
                    ".pdf" => await ExtraerDePdfAsync(archivo),
                    _ => throw new NotSupportedException($"Formato {extension} no soportado")
                };

                // Llama al método para dividir el texto en palabras y crear contextos.
                return TokenizarConContexto(contenido, Path.GetFileName(archivo));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en {Path.GetFileName(archivo)}: {ex.Message}");
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());
            }
        }

        // Extrae el texto de un archivo .docx usando la librería Open XML.
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

        // Extrae el texto de un archivo .pdf usando la librería iText.
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

        // Divide el texto en palabras y cuenta su frecuencia.
        private (Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos) TokenizarConContexto(string texto, string archivo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());

            // Divide el texto en palabras usando varios delimitadores.
            var palabras = texto.ToLower()
                .Split(new[] { ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?', '\"', '(', ')', '[', ']', '{', '}' },
                       StringSplitOptions.RemoveEmptyEntries)
                .Where(p => !PalabrasVacias.Contains(p) && p.Length > 1)
                .ToArray();

            // Cuenta la frecuencia de cada palabra en un diccionario.
            var conteos = new Dictionary<string, int>();
            foreach (var palabra in palabras)
                conteos[palabra] = conteos.GetValueOrDefault(palabra, 0) + 1;

            // Crea los objetos ContextoPalabra para cada par de palabras consecutivas.
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

        // Genera una lista extendida de configuraciones de cores a probar.
        private List<int> GenerarConfiguracionesCoresExtendidas()
        {
            var maxCores = Environment.ProcessorCount;
            var configs = new List<int> { 1, 2 };

            // Añade configuraciones basadas en el número total de cores.
            if (maxCores >= 4) configs.Add(maxCores / 2);
            if (maxCores >= 6) configs.Add((int)(maxCores * 0.75));
            if (maxCores >= 8) configs.Add((int)(maxCores * 0.85));

            configs.Add(maxCores);

            // Añade configuraciones para sistemas con muchos cores
            if (maxCores >= 8)
            {
                configs.Add(maxCores + 2);  // Sobresuscripción ligera
                configs.Add(maxCores * 2);  // Sobresuscripción agresiva
            }

            // Para sistemas de alto rendimiento
            if (maxCores >= 16)
            {
                configs.Add((int)(maxCores * 1.25));
                configs.Add((int)(maxCores * 1.5));
            }

            return configs.Distinct().OrderBy(x => x).ToList();
        }

        // Analiza las características del procesador del sistema con información extendida.
        private AnalisisProcesadores AnalizarProcesadoresExtendido()
        {
            var cores = Environment.ProcessorCount;
            return new AnalisisProcesadores
            {
                ProcessorsDisponibles = cores,
                ProcessorsRecomendados = Math.Max(1, (int)(cores * 0.75)),
                ProcessorsOptimos = cores,
                ProcessorsMaximoPrueba = cores * 2, // Para pruebas de sobresuscripción
                // Asigna un tipo de procesador en base al número de cores.
                TipoProcesador = cores >= 32 ? "Servidor/Workstation High-End" :
                                cores >= 16 ? "Servidor/Workstation" :
                                cores >= 8 ? "Desktop High-End" :
                                cores >= 4 ? "Desktop Standard" : "Low-End/Mobile",
                RecomendacionHardware = cores >= 16 ? "Excelente para paralelismo intensivo" :
                                       cores >= 8 ? "Excelente para paralelismo" :
                                       cores >= 4 ? "Adecuado para paralelismo" : "Limitado",
                JustificacionRecomendacion = $"Para I/O + CPU intensivo, usar {Math.Max(1, (int)(cores * 0.75))} de {cores} cores evita contención. " +
                                            $"Configuración máxima de prueba: {cores * 2} cores para evaluar sobresuscripción."
            };
        }

        // Completa el objeto MetricasProcesamiento con los resultados finales.
        private MetricasProcesamiento CompletarMetricas(MetricasProcesamiento metricas, ResultadoProcesamiento secuencial, ResultadoProcesamiento paralelo)
        {
            metricas.ArchivosProcesados = paralelo.ArchivosProcesados;
            metricas.PalabrasUnicas = paralelo.PalabrasUnicas;
            metricas.PalabrasTotal = paralelo.PalabrasTotal;
            metricas.TiempoSecuencialMs = secuencial.TiempoMs;
            metricas.TiempoParaleloMs = paralelo.TiempoMs;
            metricas.EstadoArchivos = new List<EstadoArchivo>(_estadoArchivos);
            metricas.MemoriaTotal = GC.GetTotalMemory(false);

            // Calcula y asigna las métricas específicas del proyecto.
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

        // Muestra un informe simplificado de rendimiento en la consola.
        private void MostrarAnalisisCompletoRendimiento(MetricasProcesamiento metricas)
        {
            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine("ANÁLISIS COMPLETO DE RENDIMIENTO DEL SISTEMA");
            Console.WriteLine(new string('=', 80));

            // Imprime la información del procesador.
            var proc = metricas.AnalisisProcesadores;
            Console.WriteLine($"PROCESADOR: {proc.TipoProcesador}");
            Console.WriteLine($"   Cores disponibles: {proc.ProcessorsDisponibles}");
            Console.WriteLine($"   Cores recomendados: {proc.ProcessorsRecomendados}");
            Console.WriteLine($"   Cores óptimos: {proc.ProcessorsOptimos}");
            Console.WriteLine($"   Cores máx. prueba: {proc.ProcessorsMaximoPrueba}");
            Console.WriteLine($"   Recomendación: {proc.RecomendacionHardware}");
            Console.WriteLine($"   Justificación: {proc.JustificacionRecomendacion}");

            Console.WriteLine("\n" + new string('-', 60));

            // Imprime las métricas principales.
            Console.WriteLine("MÉTRICAS PRINCIPALES:");
            Console.WriteLine($"   Tiempo Secuencial: {metricas.TiempoSecuencialSeg:F2}s");
            Console.WriteLine($"   Tiempo Paralelo: {metricas.TiempoParaleloSeg:F2}s");
            Console.WriteLine($"   Speedup: {metricas.Speedup:F2}x ({metricas.EvaluacionSpeedup})");
            Console.WriteLine($"   Eficiencia: {metricas.Eficiencia * 100:F1}%");
            Console.WriteLine($"   Configuración óptima: {metricas.ConfiguracionOptima}");

            Console.WriteLine("\n" + new string('-', 60));

            // Imprime las métricas específicas del proyecto.
            var esp = metricas.MetricasEspecificas;
            Console.WriteLine("MÉTRICAS ESPECÍFICAS DEL PROYECTO:");
            Console.WriteLine($"   Archivos/seg: {esp.ArchivosPromedioSegundo:F2}");
            Console.WriteLine($"   Palabras/seg: {esp.ThroughputPalabras:N0}");
            Console.WriteLine($"   Eficiencia memoria: {esp.EficienciaMemoria:F1}%");
            Console.WriteLine($"   Respuesta promedio: {esp.TiempoRespuestaPromedio:F0}ms/archivo");
            Console.WriteLine($"   Latencia inicial: {esp.LatenciaInicial:F0}ms");
            Console.WriteLine($"   Paralelismo máximo: {esp.PicosMaximosParalelismo} cores");
            Console.WriteLine($"   Rendimiento general: {esp.RendimientoGeneral}");

            Console.WriteLine("\n" + new string('-', 60));
            Console.WriteLine("RESUMEN DE TODAS LAS PRUEBAS EJECUTADAS:");
            foreach (var prueba in metricas.ResultadosPruebas.OrderBy(p => p.NumeroProcessors))
            {
                var memoriaMB = prueba.MemoriaUtilizada / 1024.0 / 1024.0;
                var estado = prueba.Speedup >= 2.0 ? "EXCELENTE" : prueba.Speedup >= 1.5 ? "BUENO" : "REGULAR";
                Console.WriteLine($"   {estado} - {prueba.NombrePrueba}: {prueba.Speedup:F2}x speedup en {prueba.TiempoMs}ms (Memoria: {memoriaMB:F1}MB)");
            }

            Console.WriteLine("\n" + new string('=', 80));
            Console.WriteLine($"ANÁLISIS COMPLETADO - {metricas.ArchivosProcesados} archivos procesados");
            Console.WriteLine($"Palabras encontradas: {metricas.PalabrasTotal:N0} total, {metricas.PalabrasUnicas:N0} únicas");
            Console.WriteLine($"Métricas guardadas en carpeta: metrics/");
            Console.WriteLine(new string('=', 80) + "\n");
        }

        // Métodos para acceder a los datos de forma segura desde otras partes de la aplicación.
        public List<ContextoPalabra> ObtenerContextosPalabras() => _contextoPalabras.ToList();

        public Dictionary<string, int> ObtenerFrecuenciaPalabras() => _frecuenciaPalabras.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public Dictionary<string, List<OrigenPalabra>> ObtenerTodasLasPalabras() =>
            _palabrasConOrigen.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());

        // Usa un bloqueo para asegurar el acceso seguro a _estadoArchivos.
        public List<EstadoArchivo> ObtenerEstadoArchivos()
        {
            lock (_lockEstados)
            {
                return new List<EstadoArchivo>(_estadoArchivos);
            }
        }

        // Métodos auxiliares para la lógica interna.
        private void ActualizarEstado(string ruta, string estado, int palabras = 0)
        {
            var nombre = Path.GetFileName(ruta);
            var tamaño = File.Exists(ruta) ? new FileInfo(ruta).Length : 0;

            // Bloquea el acceso para evitar conflictos de hilos.
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

        // Método para obtener los datos de métricas para la interfaz de usuario.
        public MetricasProcesamiento ObtenerDatosParaInterfaz()
        {
            return new MetricasProcesamiento
            {
                EstadoArchivos = ObtenerEstadoArchivos(),
                PalabrasTotal = _frecuenciaPalabras.Sum(x => x.Value),
                PalabrasUnicas = _frecuenciaPalabras.Count
            };
        }

        // Valida si la extensión del archivo es soportada.
        private bool EsArchivoValido(string nombre) =>
            new[] { ".txt", ".pdf", ".docx" }.Contains(Path.GetExtension(nombre).ToLower());

        // Limpia caracteres no válidos de los nombres de archivo para guardarlos.
        private string LimpiarNombreArchivo(string nombre)
        {
            foreach (var c in new[] { "<", ">", ":", "\"", "|", "?", "*" })
                nombre = nombre.Replace(c, "_");
            return nombre;
        }

        // Limpia todas las colecciones de datos para una nueva ejecución.
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

        // Método para comparar rendimiento entre diferentes ejecuciones
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