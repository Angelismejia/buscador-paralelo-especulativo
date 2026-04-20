using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BuscadorParaleloEspeculativo.UI.Models;
using System.Text.Json;

namespace BuscadorParaleloEspeculativo.UI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ProcesadorArchivos _procesadorArchivos;
        private readonly ModeloPrediccion _modeloPrediccion;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
            // Inicializa el procesador de archivos que maneja el análisis paralelo de rendimiento
            _procesadorArchivos = new ProcesadorArchivos();
            // Inicializa el modelo de predicción de texto que usa los contextos extraídos
            _modeloPrediccion = new ModeloPrediccion();
        }

        // Propiedad que se enlaza automáticamente con los archivos subidos desde el formulario web
        [BindProperty]
        public List<IFormFile> ArchivosSubidos { get; set; } = new List<IFormFile>();

        public void OnGet()
        {
            _logger.LogInformation("Sistema de Predicción de Texto Especulativo iniciado");
        }

        // Método principal que procesa los archivos subidos y ejecuta el análisis de rendimiento paralelo
        public async Task<IActionResult> OnPostProcesarArchivosAsync()
        {
            try
            {
                // Validación temprana: verifica que se hayan subido archivos antes de continuar
                if (ArchivosSubidos == null || !ArchivosSubidos.Any())
                {
                    return new JsonResult(new
                    {
                        success = false,
                        message = "No se han subido archivos para procesar"
                    });
                }

                _logger.LogInformation($"Iniciando procesamiento de {ArchivosSubidos.Count} archivos");

                // Crea una carpeta temporal única para guardar los archivos subidos durante el procesamiento
                var carpetaTemporal = Path.Combine(Path.GetTempPath(), "BuscadorParalelo", Guid.NewGuid().ToString());

                // Ejecuta el análisis completo de rendimiento: secuencial vs paralelo con múltiples configuraciones de cores
                var metricas = await _procesadorArchivos.ProcesarArchivosSubidosAsync(
                    ArchivosSubidos.ToArray(),
                    carpetaTemporal
                );

                // Obtiene los contextos de palabras extraídos durante el procesamiento (palabra anterior -> palabra actual)
                var contextos = _procesadorArchivos.ObtenerContextosPalabras();
                if (contextos.Any())
                {
                    // Entrena el modelo de predicción con los contextos encontrados para habilitar autocompletado
                    _modeloPrediccion.EntrenarModelo(contextos);
                }

                // Prepara la respuesta JSON con todas las métricas de rendimiento calculadas
                var resultado = new
                {
                    success = true,
                    // Tiempos de ejecución en segundos para comparación
                    tiempoSecuencial = metricas.TiempoSecuencialSeg,
                    tiempoParalelo = metricas.TiempoParaleloSeg,
                    // Speedup: cuántas veces más rápido es el procesamiento paralelo vs secuencial
                    speedup = Math.Round(metricas.Speedup, 2),
                    // Eficiencia: qué tan bien se aprovechan los cores disponibles (ideal = 100%)
                    eficiencia = Math.Round(metricas.Eficiencia * 100, 1),
                    // Throughput: palabras procesadas por segundo en cada método
                    palabrasSegSecuencial = (int)metricas.PalabrasSecuencialPorSeg,
                    palabrasSegParalelo = (int)metricas.PalabrasParaleloPorSeg,
                    // Estadísticas de contenido procesado
                    palabrasUnicas = metricas.PalabrasUnicas,
                    palabrasTotales = metricas.PalabrasTotal,
                    archivosCount = metricas.ArchivosProcesados,
                    // Evaluación cualitativa del rendimiento (Excelente, Muy Bueno, Bueno, etc.)
                    evaluacionSpeedup = metricas.EvaluacionSpeedup,
                    // Detalle por archivo: nombre, tamaño, palabras procesadas y estado final
                    fuentes = metricas.EstadoArchivos.Select(a => new {
                        nombre = a.NombreArchivo,
                        tamaño = a.TamañoLegible,  // Formato legible (KB, MB)
                        palabras = a.PalabrasProcesadas,
                        estado = a.Estado  // "Procesado", "Error", etc.
                    }).ToList()
                };

                _logger.LogInformation($"Procesamiento completado. Speedup: {metricas.Speedup:F2}x ({metricas.EvaluacionSpeedup}), Eficiencia: {metricas.Eficiencia * 100:F1}%");

                return new JsonResult(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el procesamiento de archivos");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error durante el procesamiento: {ex.Message}"
                });
            }
        }

        // Genera predicciones de texto basadas en el contexto proporcionado usando el modelo entrenado
        public IActionResult OnPostPredecirTexto([FromBody] PrediccionRequest request)
        {
            try
            {
                // Validación: requiere un contexto no vacío para generar predicciones
                if (string.IsNullOrWhiteSpace(request?.Contexto))
                {
                    return new JsonResult(new
                    {
                        success = false,
                        predicciones = new List<string>()
                    });
                }

                _logger.LogDebug($"Generando predicciones para contexto: '{request.Contexto}'");

                // Genera las top-K predicciones más probables basadas en el contexto dado
                var predicciones = _modeloPrediccion.PredecirSiguientePalabra(
                    request.Contexto,
                    request.TopK ?? 8  // Por defecto muestra las 8 predicciones más probables
                );

                return new JsonResult(new
                {
                    success = true,
                    predicciones = predicciones,
                    contexto = request.Contexto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar predicciones para contexto: '{request?.Contexto}'");
                return new JsonResult(new
                {
                    success = false,
                    predicciones = new List<string>(),
                    error = ex.Message
                });
            }
        }

        // Valida archivos antes del procesamiento para evitar errores y optimizar la experiencia del usuario
        public IActionResult OnPostValidarArchivos([FromBody] ValidacionRequest request)
        {
            try
            {
                // Define las reglas de validación para archivos
                var extensionesValidas = new[] { ".txt", ".docx", ".pdf" };
                var tamañoMaximo = 50 * 1024 * 1024; // Límite de 50MB por archivo para evitar problemas de memoria
                var errores = new List<string>();
                var archivosValidos = new List<object>();

                // Itera cada archivo para aplicar las reglas de validación
                foreach (var archivo in request.Archivos ?? new List<ArchivoInfo>())
                {
                    var extension = Path.GetExtension(archivo.Nombre).ToLowerInvariant();
                    var esValido = true;
                    var mensajesArchivo = new List<string>();

                    // Validación 1: extensión de archivo soportada
                    if (!extensionesValidas.Contains(extension))
                    {
                        mensajesArchivo.Add($"Extensión {extension} no soportada");
                        esValido = false;
                    }

                    // Validación 2: tamaño no excede el límite para evitar problemas de memoria
                    if (archivo.Tamaño > tamañoMaximo)
                    {
                        mensajesArchivo.Add($"Archivo demasiado grande ({archivo.Tamaño / (1024 * 1024):F1}MB)");
                        esValido = false;
                    }

                    // Validación 3: archivo no está vacío
                    if (archivo.Tamaño <= 0)
                    {
                        mensajesArchivo.Add("Archivo vacío");
                        esValido = false;
                    }

                    // Agrega el resultado de validación para este archivo
                    archivosValidos.Add(new
                    {
                        nombre = archivo.Nombre,
                        tamaño = archivo.Tamaño,
                        valido = esValido,
                        mensajes = mensajesArchivo
                    });

                    // Acumula errores para el reporte general si el archivo no es válido
                    if (!esValido)
                    {
                        errores.AddRange(mensajesArchivo.Select(m => $"{archivo.Nombre}: {m}"));
                    }
                }

                // Respuesta con resultados de validación y resumen estadístico
                return new JsonResult(new
                {
                    success = !errores.Any(),  // Éxito solo si no hay errores
                    archivos = archivosValidos,
                    errores = errores,
                    // Resumen estadístico para mostrar en la interfaz
                    resumen = new
                    {
                        total = archivosValidos.Count,
                        validos = archivosValidos.Count(a => (bool)((dynamic)a).valido),
                        invalidos = archivosValidos.Count(a => !(bool)((dynamic)a).valido)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la validación de archivos");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error durante la validación: {ex.Message}"
                });
            }
        }
    }

    // Modelo para las requests de predicción de texto desde el cliente
    public class PrediccionRequest
    {
        public string? Contexto { get; set; }  // Texto base para generar predicciones
        public int? TopK { get; set; }         // Número de predicciones a retornar (opcional)
    }

    // Modelo para las requests de validación de archivos desde el cliente
    public class ValidacionRequest
    {
        public List<ArchivoInfo>? Archivos { get; set; }  // Lista de archivos a validar
    }

    // Información básica de un archivo para validación previa al upload
    public class ArchivoInfo
    {
        public string Nombre { get; set; } = string.Empty;  // Nombre del archivo con extensión
        public long Tamaño { get; set; }                    // Tamaño en bytes
    }
}