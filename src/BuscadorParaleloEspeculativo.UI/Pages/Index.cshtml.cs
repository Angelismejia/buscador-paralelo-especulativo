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
            _procesadorArchivos = new ProcesadorArchivos();
            _modeloPrediccion = new ModeloPrediccion();
        }

        [BindProperty]
        public List<IFormFile> ArchivosSubidos { get; set; } = new List<IFormFile>();

        public void OnGet()
        {
            _logger.LogInformation("Sistema de Predicción de Texto Especulativo iniciado");
        }

        
        /// Procesa los archivos subidos utilizando el procesamiento paralelo de CANDY 
        
        public async Task<IActionResult> OnPostProcesarArchivosAsync()
        {
            try
            {
                //  verificación null más explícita
                if (ArchivosSubidos == null || !ArchivosSubidos.Any())
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = "No se han subido archivos para procesar" 
                    });
                }

                _logger.LogInformation($"Iniciando procesamiento de {ArchivosSubidos.Count} archivos");

                // Usar el método nuevo del ProcesadorArchivos
                var metricas = await _procesadorArchivos.ProcesarArchivosSubidosAsync(
                    ArchivosSubidos.ToArray(), 
                    Path.Combine(Path.GetTempPath(), "BuscadorParalelo", Guid.NewGuid().ToString())
                );

                // Entrenar el modelo con los contextos de ANGEL
                var contextos = _procesadorArchivos.ObtenerContextosPalabras();
                if (contextos.Any())
                {
                    _modeloPrediccion.EntrenarModelo(contextos);
                }

                // Calcular métricas mejoradas
                var resultado = new
                {
                    success = true,
                    tiempoSecuencial = metricas.TiempoSecuencialSeg,
                    tiempoParalelo = metricas.TiempoParaleloSeg,
                    speedup = Math.Round(metricas.Speedup, 2),
                    eficiencia = Math.Round(metricas.Eficiencia * 100, 1),
                    palabrasSegSecuencial = (int)metricas.PalabrasSecuencialPorSeg, 
                    palabrasSegParalelo = (int)metricas.PalabrasParaleloPorSeg,
                    palabrasUnicas = metricas.PalabrasUnicas,
                    palabrasTotales = metricas.PalabrasTotal,
                    archivosCount = metricas.ArchivosProcesados,
                    evaluacionSpeedup = metricas.EvaluacionSpeedup,
                    // Corrección CS8917: especificar el tipo explícitamente
                    fuentes = metricas.EstadoArchivos.Select(a => new {
                        nombre = a.NombreArchivo,
                        tamaño = a.TamañoLegible,
                        palabras = a.PalabrasProcesadas,
                        estado = a.Estado
                    }).ToList()
                };

                _logger.LogInformation($"Procesamiento completado. Speedup: {metricas.Speedup:F2}x ({metricas.EvaluacionSpeedup}), Eficiencia: {metricas.Eficiencia * 100:F1}%");

                return new JsonResult(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el procesamiento de archivos");
                return new JsonResult(new { 
                    success = false, 
                    message = $"Error durante el procesamiento: {ex.Message}" 
                });
            }
        }

        /// <summary>
        /// Obtiene predicciones de texto utilizando el modelo de ANGEL
        /// </summary>
        public IActionResult OnPostPredecirTexto([FromBody] PrediccionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Contexto))
                {
                    return new JsonResult(new { 
                        success = false, 
                        predicciones = new List<string>() 
                    });
                }

                _logger.LogDebug($"Generando predicciones para contexto: '{request.Contexto}'");

                var predicciones = _modeloPrediccion.PredecirSiguientePalabra(
                    request.Contexto, 
                    request.TopK ?? 8
                );

                return new JsonResult(new { 
                    success = true, 
                    predicciones = predicciones,
                    contexto = request.Contexto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al generar predicciones para contexto: '{request?.Contexto}'");
                return new JsonResult(new { 
                    success = false, 
                    predicciones = new List<string>(),
                    error = ex.Message
                });
            }
        }


        /// <summary>
        /// Valida el formato y tamaño de archivos antes de procesarlos
        /// </summary>
        public IActionResult OnPostValidarArchivos([FromBody] ValidacionRequest request)
        {
            try
            {
                var extensionesValidas = new[] { ".txt", ".docx", ".pdf" };
                var tamañoMaximo = 50 * 1024 * 1024; // 50MB
                var errores = new List<string>();
                var archivosValidos = new List<object>();

                foreach (var archivo in request.Archivos ?? new List<ArchivoInfo>())
                {
                    var extension = Path.GetExtension(archivo.Nombre).ToLowerInvariant();
                    var esValido = true;
                    var mensajesArchivo = new List<string>();

                    if (!extensionesValidas.Contains(extension))
                    {
                        mensajesArchivo.Add($"Extensión {extension} no soportada");
                        esValido = false;
                    }

                    if (archivo.Tamaño > tamañoMaximo)
                    {
                        mensajesArchivo.Add($"Archivo demasiado grande ({archivo.Tamaño / (1024 * 1024):F1}MB)");
                        esValido = false;
                    }

                    if (archivo.Tamaño <= 0)
                    {
                        mensajesArchivo.Add("Archivo vacío");
                        esValido = false;
                    }

                    archivosValidos.Add(new
                    {
                        nombre = archivo.Nombre,
                        tamaño = archivo.Tamaño,
                        valido = esValido,
                        mensajes = mensajesArchivo
                    });

                    if (!esValido)
                    {
                        errores.AddRange(mensajesArchivo.Select(m => $"{archivo.Nombre}: {m}"));
                    }
                }

                return new JsonResult(new
                {
                    success = !errores.Any(),
                    archivos = archivosValidos,
                    errores = errores,
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

    // Modelos para las requests JSON
    public class PrediccionRequest
    {
        public string? Contexto { get; set; }
        public int? TopK { get; set; }
    }

    public class ValidacionRequest
    {
        public List<ArchivoInfo>? Archivos { get; set; }
    }

    public class ArchivoInfo
    {
        public string Nombre { get; set; } = string.Empty;
        public long Tamaño { get; set; }
    }
}