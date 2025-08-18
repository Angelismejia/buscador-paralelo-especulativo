using Microsoft.AspNetCore.Mvc;
using BuscadorParaleloEspeculativo.UI.Models;
using System.ComponentModel.DataAnnotations;

namespace BuscadorParaleloEspeculativo.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivosController : ControllerBase
    {
        private readonly ILogger<ArchivosController> _logger;
        private readonly ProcesadorArchivos _procesadorArchivos;
        private readonly ModeloPrediccion _modeloPrediccion;

        public ArchivosController(ILogger<ArchivosController> logger,
            ProcesadorArchivos procesadorArchivos,
            ModeloPrediccion modeloPrediccion)
        {
            _logger = logger;
            _procesadorArchivos = procesadorArchivos;
            _modeloPrediccion = modeloPrediccion;
        }

        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarArchivos([FromForm] List<IFormFile> files)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se han subido archivos para procesar"
                    });
                }

                _logger.LogInformation($"[API] Procesando {files.Count} archivos");

                // Limpiar datos previos
                _procesadorArchivos.LimpiarDatos();
                _modeloPrediccion.LimpiarModelo();

                // Crear carpeta temporal única
                var carpetaTemporal = Path.Combine(Path.GetTempPath(), "BuscadorParalelo", Guid.NewGuid().ToString());

                // Procesar archivos con CANDY
                var metricas = await _procesadorArchivos.ProcesarArchivosSubidosAsync(
                    files.ToArray(),
                    carpetaTemporal
                );

                // Entrenar modelo de predicción con ANGEL
                var contextos = _procesadorArchivos.ObtenerContextosPalabras();
                if (contextos.Any())
                {
                    _logger.LogInformation($"[API] Entrenando modelo con {contextos.Count} contextos");
                    _modeloPrediccion.EntrenarModelo(contextos);

                    var estadisticas = _modeloPrediccion.ObtenerEstadisticas();
                    _logger.LogInformation($"[API] Modelo entrenado: {_modeloPrediccion.TotalBigramas} bigramas, {_modeloPrediccion.TotalTrigramas} trigramas");
                }

                // Preparar respuesta con métricas reales
                var respuesta = new
                {
                    success = true,
                    message = "Archivos procesados exitosamente",

                    // Métricas de rendimiento paralelo
                    tiempoSecuencialSeg = Math.Round(metricas.TiempoSecuencialSeg, 2),
                    tiempoParaleloSeg = Math.Round(metricas.TiempoParaleloSeg, 2),
                    speedup = Math.Round(metricas.Speedup, 2),
                    eficiencia = Math.Round(metricas.Eficiencia, 3),
                    palabrasSecuencialPorSeg = (int)metricas.PalabrasSecuencialPorSeg,
                    palabrasParaleloPorSeg = (int)metricas.PalabrasParaleloPorSeg,
                    evaluacionSpeedup = metricas.EvaluacionSpeedup,

                    // Datos de procesamiento
                    archivosTotal = metricas.ArchivosTotal,
                    archivosProcesados = metricas.ArchivosProcesados,
                    palabrasUnicas = metricas.PalabrasUnicas,
                    palabrasTotal = metricas.PalabrasTotal,

                    // Estado de archivos individuales
                    archivos = metricas.EstadoArchivos.Select(a => new {
                        nombre = a.NombreArchivo,
                        estado = a.Estado,
                        palabras = a.PalabrasProcesadas,
                        tamaño = a.TamañoLegible
                    }).ToList(),

                    // Información del modelo entrenado
                    modeloEntrenado = _modeloPrediccion.TotalBigramas > 0,
                    bigramas = _modeloPrediccion.TotalBigramas,
                    trigramas = _modeloPrediccion.TotalTrigramas,
                    contextosEntrenamiento = _modeloPrediccion.TotalContextos
                };

                _logger.LogInformation($"[API] Procesamiento completado: Speedup {metricas.Speedup:F2}x, Modelo: {_modeloPrediccion.TotalBigramas} bigramas");

                // Limpiar archivos temporales
                try
                {
                    if (Directory.Exists(carpetaTemporal))
                        Directory.Delete(carpetaTemporal, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"[API] No se pudo limpiar carpeta temporal: {ex.Message}");
                }

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[API] Error durante el procesamiento de archivos");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("estado")]
        public IActionResult ObtenerEstado()
        {
            try
            {
                var datosInterfaz = _procesadorArchivos.ObtenerDatosParaInterfaz();
                var estadisticasModelo = _modeloPrediccion.ObtenerEstadisticas();

                return Ok(new
                {
                    success = true,
                    procesamiento = datosInterfaz,
                    modelo = new
                    {
                        entrenado = _modeloPrediccion.TotalBigramas > 0,
                        bigramas = _modeloPrediccion.TotalBigramas,
                        trigramas = _modeloPrediccion.TotalTrigramas,
                        contextos = _modeloPrediccion.TotalContextos,
                        estadisticas = estadisticasModelo
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[API] Error obteniendo estado del sistema");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("limpiar")]
        public IActionResult LimpiarDatos()
        {
            try
            {
                _procesadorArchivos.LimpiarDatos();
                _modeloPrediccion.LimpiarModelo();

                _logger.LogInformation("[API] Datos limpiados exitosamente");

                return Ok(new
                {
                    success = true,
                    message = "Datos limpiados exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[API] Error limpiando datos");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
