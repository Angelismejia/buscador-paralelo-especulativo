using Microsoft.AspNetCore.Mvc;
using BuscadorParaleloEspeculativo.UI.Models;
using System.ComponentModel.DataAnnotations;

namespace BuscadorParaleloEspeculativo.UI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrediccionController : ControllerBase
    {
        private readonly ILogger<PrediccionController> _logger;
        private readonly ModeloPrediccion _modeloPrediccion;
        private readonly ProcesadorArchivos _procesadorArchivos;

        public PrediccionController(ILogger<PrediccionController> logger,
            ModeloPrediccion modeloPrediccion,
            ProcesadorArchivos procesadorArchivos)
        {
            _logger = logger;
            _modeloPrediccion = modeloPrediccion;
            _procesadorArchivos = procesadorArchivos;
        }

        [HttpPost("predecir")]
        public IActionResult PredecirSiguientePalabra([FromBody] PrediccionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Contexto))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El contexto no puede estar vacío",
                        predicciones = new List<object>()
                    });
                }

                // Verificar si el modelo está entrenado
                if (_modeloPrediccion.TotalBigramas == 0 && _modeloPrediccion.TotalTrigramas == 0)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "Modelo no entrenado. Procese archivos primero.",
                        predicciones = new List<object>(),
                        modeloEntrenado = false
                    });
                }

                var contexto = request.Contexto.Trim();
                var topK = Math.Min(Math.Max(request.TopK ?? 8, 1), 20); // Entre 1 y 20

                _logger.LogDebug($"[Predicción] Generando predicciones para: '{contexto}' (topK: {topK})");

                // Obtener predicciones del modelo de ANGEL
                var prediccionesRaw = _modeloPrediccion.PredecirSiguientePalabra(contexto, topK);

                if (!prediccionesRaw.Any())
                {
                    _logger.LogInformation($"[Predicción] No se encontraron predicciones para: '{contexto}'");

                    return Ok(new
                    {
                        success = true,
                        message = "No se encontraron predicciones para este contexto",
                        predicciones = new List<object>(),
                        contexto = contexto,
                        modeloEntrenado = true,
                        bigramas = _modeloPrediccion.TotalBigramas,
                        trigramas = _modeloPrediccion.TotalTrigramas
                    });
                }

                // Determinar método usado (trigrama vs bigrama)
                var palabrasContexto = contexto.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var metodoUsado = palabrasContexto.Length >= 2 ? "trigrama" : "bigrama";

                // Formatear predicciones para la interfaz
                var prediccionesFormateadas = prediccionesRaw.Select((pred, index) => new {
                    palabra = pred.Palabra,
                    relevancia = pred.Archivos.Count,
                    archivos = pred.Archivos,
                    metodo = metodoUsado,
                    posicion = index + 1,
                    confianza = CalcularConfianza(pred.Archivos.Count, prediccionesRaw.Count)
                }).ToList();

                _logger.LogInformation($"[Predicción] Devolviendo {prediccionesFormateadas.Count} predicciones usando {metodoUsado}");

                return Ok(new
                {
                    success = true,
                    predicciones = prediccionesFormateadas,
                    contexto = contexto,
                    metodoUsado = metodoUsado,
                    totalPredicciones = prediccionesFormateadas.Count,
                    modeloEntrenado = true,
                    estadisticasModelo = new
                    {
                        bigramas = _modeloPrediccion.TotalBigramas,
                        trigramas = _modeloPrediccion.TotalTrigramas,
                        contextos = _modeloPrediccion.TotalContextos
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Predicción] Error generando predicciones para: '{request?.Contexto}'");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor",
                    predicciones = new List<object>(),
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("estadisticas")]
        public IActionResult ObtenerEstadisticas()
        {
            try
            {
                var estadisticas = _modeloPrediccion.ObtenerEstadisticas();

                return Ok(new
                {
                    success = true,
                    modeloEntrenado = _modeloPrediccion.TotalBigramas > 0 || _modeloPrediccion.TotalTrigramas > 0,
                    bigramas = _modeloPrediccion.TotalBigramas,
                    trigramas = _modeloPrediccion.TotalTrigramas,
                    contextos = _modeloPrediccion.TotalContextos,
                    estadisticasDetalladas = estadisticas,
                    capacidadPrediccion = new
                    {
                        puedePredeir = _modeloPrediccion.TotalBigramas > 0,
                        precisionEsperada = CalcularPrecisionEsperada(),
                        cobertura = CalcularCobertura()
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Predicción] Error obteniendo estadísticas del modelo");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpPost("buscar-prefijo")]
        public IActionResult BuscarPorPrefijo([FromBody] PrefijoBusquedaRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Prefijo))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El prefijo no puede estar vacío"
                    });
                }

                var limite = Math.Min(Math.Max(request.Limite ?? 10, 1), 50);

                _logger.LogDebug($"[Búsqueda] Buscando palabras con prefijo: '{request.Prefijo}' (límite: {limite})");

                var resultados = _modeloPrediccion.BuscarPorPrefijo(request.Prefijo, limite);

                var resultadosFormateados = resultados.Select(r => new {
                    palabra = r.Palabra,
                    archivos = r.Archivos,
                    relevancia = r.Relevancia,
                    fuentes = r.Archivos.Take(3).ToList(), // Primeras 3 fuentes
                    totalFuentes = r.Archivos.Count
                }).ToList();

                return Ok(new
                {
                    success = true,
                    prefijo = request.Prefijo,
                    resultados = resultadosFormateados,
                    totalEncontrados = resultadosFormateados.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Búsqueda] Error buscando prefijo: '{request?.Prefijo}'");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("diagnostico")]
        public IActionResult ObtenerDiagnostico()
        {
            try
            {
                var estadisticas = _modeloPrediccion.ObtenerEstadisticas();
                var contextos = _procesadorArchivos.ObtenerContextosPalabras().Take(20).ToList();
                var palabrasComunes = _procesadorArchivos.ObtenerFrecuenciaPalabras()
                    .OrderByDescending(p => p.Value)
                    .Take(20)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    diagnostico = new
                    {
                        modeloEntrenado = _modeloPrediccion.TotalBigramas > 0 || _modeloPrediccion.TotalTrigramas > 0,
                        estadisticasModelo = estadisticas,
                        ejemplosContextos = contextos.Select(c => new {
                            anterior = c.PalabraAnterior,
                            actual = c.PalabraActual,
                            archivo = c.ArchivoOrigen,
                            posicion = c.Posicion
                        }),
                        palabrasMasFrecuentes = palabrasComunes.Select(p => new {
                            palabra = p.Key,
                            frecuencia = p.Value
                        }),
                        resumen = new
                        {
                            totalContextosProcesados = _procesadorArchivos.ObtenerContextosPalabras().Count,
                            totalPalabrasUnicas = _procesadorArchivos.ObtenerFrecuenciaPalabras().Count,
                            archivosProcesados = _procesadorArchivos.ObtenerEstadoArchivos()
                                .Count(a => a.Estado == "Procesado")
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Diagnóstico] Error obteniendo diagnóstico del sistema");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        public IActionResult ValidarContexto([FromBody] ValidacionContextoRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.Contexto))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El contexto no puede estar vacío"
                    });
                }

                var contexto = request.Contexto.Trim();
                var palabras = contexto.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var validacion = new
                {
                    contextValido = palabras.Length > 0,
                    numeroPalabras = palabras.Length,
                    puedeUsarTrigramas = palabras.Length >= 2,
                    puedeUsarBigramas = palabras.Length >= 1,
                    metodosDisponibles = new List<string>(),
                    ultimaPalabra = palabras.LastOrDefault(),
                    penultimaPalabra = palabras.Length >= 2 ? palabras[^2] : null
                };

                if (validacion.puedeUsarTrigramas)
                    ((List<string>)validacion.metodosDisponibles).Add("trigrama");

                if (validacion.puedeUsarBigramas)
                    ((List<string>)validacion.metodosDisponibles).Add("bigrama");

                return Ok(new
                {
                    success = true,
                    contexto = contexto,
                    validacion = validacion
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Validación] Error validando contexto: '{request?.Contexto}'");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // Métodos auxiliares
        private double CalcularConfianza(int relevancia, int totalPredicciones)
        {
            if (totalPredicciones == 0) return 0.0;
            return Math.Round((double)relevancia / totalPredicciones * 100, 1);
        }

        private string CalcularPrecisionEsperada()
        {
            var totalBigramas = _modeloPrediccion.TotalBigramas;
            var totalTrigramas = _modeloPrediccion.TotalTrigramas;

            if (totalTrigramas > 1000) return "Alta";
            if (totalBigramas > 500) return "Media";
            if (totalBigramas > 100) return "Baja";
            return "Mínima";
        }

        private string CalcularCobertura()
        {
            var totalContextos = _modeloPrediccion.TotalContextos;

            if (totalContextos > 5000) return "Amplia";
            if (totalContextos > 1000) return "Media";
            if (totalContextos > 100) return "Limitada";
            return "Mínima";
        }
    }

    // Modelos para las requests
    public class PrediccionRequest
    {
        [Required]
        public string? Contexto { get; set; }

        [Range(1, 20)]
        public int? TopK { get; set; } = 8;
    }

    public class PrefijoBusquedaRequest
    {
        [Required]
        public string? Prefijo { get; set; }

        [Range(1, 50)]
        public int? Limite { get; set; } = 10;
    }

    public class ValidacionContextoRequest
    {
        [Required]
        public string? Contexto { get; set; }
    }
}