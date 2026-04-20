using Microsoft.AspNetCore.Mvc;
using BuscadorParaleloEspeculativo.UI.Models;
using System.ComponentModel.DataAnnotations;

namespace BuscadorParaleloEspeculativo.UI.Controllers
{
    // Controlador API REST para manejo de archivos y procesamiento paralelo
    // Define las rutas base como "api/archivos"
    [Route("api/[controller]")]
    [ApiController]
    public class ArchivosController : ControllerBase
    {
        // Servicios inyectados para el funcionamiento del controlador
        private readonly ILogger<ArchivosController> _logger;           // Sistema de logging para diagnóstico
        private readonly ProcesadorArchivos _procesadorArchivos;        // Componente CANDY - procesamiento paralelo
        private readonly ModeloPrediccion _modeloPrediccion;            // Componente ANGEL - modelo de predicción

        // Constructor con inyección de dependencias
        public ArchivosController(ILogger<ArchivosController> logger,
            ProcesadorArchivos procesadorArchivos,
            ModeloPrediccion modeloPrediccion)
        {
            _logger = logger;
            _procesadorArchivos = procesadorArchivos;
            _modeloPrediccion = modeloPrediccion;
        }

        // Endpoint principal para procesar archivos subidos
        // Recibe archivos mediante formulario multipart y los procesa de forma paralela
        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarArchivos([FromForm] List<IFormFile> files)
        {
            try
            {
                // Validación inicial de entrada
                if (files == null || !files.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "No se han subido archivos para procesar"
                    });
                }

                _logger.LogInformation($"Iniciando procesamiento de {files.Count} archivos");

                // Preparación del sistema - limpiar datos de ejecuciones anteriores
                _procesadorArchivos.LimpiarDatos();
                _modeloPrediccion.LimpiarModelo();

                // Crear directorio temporal único para almacenar archivos durante el procesamiento
                var carpetaTemporal = Path.Combine(Path.GetTempPath(), "BuscadorParalelo", Guid.NewGuid().ToString());

                // FASE 1: Procesamiento paralelo de archivos usando componente CANDY
                // Analiza los archivos de forma simultánea para extraer texto y generar métricas
                var metricas = await _procesadorArchivos.ProcesarArchivosSubidosAsync(
                    files.ToArray(),
                    carpetaTemporal
                );

                // FASE 2: Entrenamiento del modelo de predicción usando componente ANGEL
                // Obtiene contextos de palabras del procesamiento anterior
                var contextos = _procesadorArchivos.ObtenerContextosPalabras();
                if (contextos.Any())
                {
                    _logger.LogInformation($"Iniciando entrenamiento del modelo con {contextos.Count} contextos de palabras");

                    // Entrena el modelo con los contextos extraídos
                    _modeloPrediccion.EntrenarModelo(contextos);

                    var estadisticas = _modeloPrediccion.ObtenerEstadisticas();
                    _logger.LogInformation($"Modelo entrenado exitosamente: {_modeloPrediccion.TotalBigramas} bigramas, {_modeloPrediccion.TotalTrigramas} trigramas");
                }

                // FASE 3: Preparar respuesta completa con todas las métricas
                var respuesta = new
                {
                    success = true,
                    message = "Archivos procesados exitosamente",

                    // Métricas de rendimiento del procesamiento paralelo
                    tiempoSecuencialSeg = Math.Round(metricas.TiempoSecuencialSeg, 2),      // Tiempo que tomaría procesamiento secuencial
                    tiempoParaleloSeg = Math.Round(metricas.TiempoParaleloSeg, 2),          // Tiempo real del procesamiento paralelo
                    speedup = Math.Round(metricas.Speedup, 2),                              // Factor de aceleración obtenido
                    eficiencia = Math.Round(metricas.Eficiencia, 3),                       // Eficiencia del paralelismo (0-1)
                    palabrasSecuencialPorSeg = (int)metricas.PalabrasSecuencialPorSeg,     // Throughput secuencial
                    palabrasParaleloPorSeg = (int)metricas.PalabrasParaleloPorSeg,         // Throughput paralelo
                    evaluacionSpeedup = metricas.EvaluacionSpeedup,                        // Evaluación cualitativa del speedup

                    // Datos estadísticos del procesamiento
                    archivosTotal = metricas.ArchivosTotal,           // Cantidad total de archivos procesados
                    archivosProcesados = metricas.ArchivosProcesados, // Archivos procesados exitosamente
                    palabrasUnicas = metricas.PalabrasUnicas,         // Vocabulario único extraído
                    palabrasTotal = metricas.PalabrasTotal,           // Total de palabras procesadas

                    // Estado detallado de cada archivo individual
                    archivos = metricas.EstadoArchivos.Select(a => new {
                        nombre = a.NombreArchivo,         // Nombre original del archivo
                        estado = a.Estado,                // Estado de procesamiento (Procesado/Error)
                        palabras = a.PalabrasProcesadas,  // Cantidad de palabras extraídas
                        tamaño = a.TamañoLegible          // Tamaño en formato legible
                    }).ToList(),

                    // Información del modelo de predicción entrenado
                    modeloEntrenado = _modeloPrediccion.TotalBigramas > 0,    // Indica si el modelo está listo
                    bigramas = _modeloPrediccion.TotalBigramas,               // Cantidad de bigramas en el modelo
                    trigramas = _modeloPrediccion.TotalTrigramas,             // Cantidad de trigramas en el modelo
                    contextosEntrenamiento = _modeloPrediccion.TotalContextos // Contextos usados para entrenamiento
                };

                _logger.LogInformation($"Procesamiento completado - Speedup: {metricas.Speedup:F2}x, Modelo: {_modeloPrediccion.TotalBigramas} bigramas");

                // Limpieza de archivos temporales para liberar espacio
                try
                {
                    if (Directory.Exists(carpetaTemporal))
                        Directory.Delete(carpetaTemporal, true);
                }
                catch (Exception ex)
                {
                    // Error en limpieza no es crítico, solo se registra
                    _logger.LogWarning($"No se pudo limpiar la carpeta temporal: {ex.Message}");
                }

                return Ok(respuesta);
            }
            catch (Exception ex)
            {
                // Manejo centralizado de errores con logging detallado
                _logger.LogError(ex, "Error crítico durante el procesamiento de archivos");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error interno del servidor: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }

        // Endpoint para consultar el estado actual del sistema
        // Proporciona información sobre archivos procesados y estado del modelo
        [HttpGet("estado")]
        public IActionResult ObtenerEstado()
        {
            try
            {
                // Obtiene datos actuales del procesador de archivos
                var datosInterfaz = _procesadorArchivos.ObtenerDatosParaInterfaz();

                // Obtiene estadísticas actuales del modelo de predicción
                var estadisticasModelo = _modeloPrediccion.ObtenerEstadisticas();

                return Ok(new
                {
                    success = true,
                    procesamiento = datosInterfaz,  // Estado completo del procesamiento
                    modelo = new
                    {
                        entrenado = _modeloPrediccion.TotalBigramas > 0,  // Estado del modelo
                        bigramas = _modeloPrediccion.TotalBigramas,       // Conteo de bigramas
                        trigramas = _modeloPrediccion.TotalTrigramas,     // Conteo de trigramas
                        contextos = _modeloPrediccion.TotalContextos,     // Total de contextos
                        estadisticas = estadisticasModelo                 // Estadísticas detalladas
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el estado del sistema");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // Endpoint para limpiar todos los datos del sistema
        // Resetea tanto el procesador como el modelo a estado inicial
        [HttpPost("limpiar")]
        public IActionResult LimpiarDatos()
        {
            try
            {
                // Limpia datos del procesador de archivos
                _procesadorArchivos.LimpiarDatos();

                // Resetea el modelo de predicción
                _modeloPrediccion.LimpiarModelo();

                _logger.LogInformation("Sistema limpiado exitosamente - todos los datos han sido borrados");

                return Ok(new
                {
                    success = true,
                    message = "Datos limpiados exitosamente"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la limpieza del sistema");
                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}