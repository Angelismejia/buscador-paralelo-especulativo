using Microsoft.AspNetCore.Mvc;
using BuscadorParaleloEspeculativo.UI.Models;
using System.ComponentModel.DataAnnotations;

namespace BuscadorParaleloEspeculativo.UI.Controllers
{
    
    /// Controlador que maneja las operaciones de predicción de texto
    /// Implementa los endpoints para predecir la siguiente palabra basado en el contexto
   
    [Route("api/[controller]")]
    [ApiController]
    public class PrediccionController : ControllerBase
    {
        private readonly ILogger<PrediccionController> _logger;
        private readonly ModeloPrediccion _modeloPrediccion;
        private readonly ProcesadorArchivos _procesadorArchivos;

        
        /// Constructor que inicializa las dependencias necesarias
        
        public PrediccionController(ILogger<PrediccionController> logger,
            ModeloPrediccion modeloPrediccion,
            ProcesadorArchivos procesadorArchivos)
        {
            _logger = logger;
            _modeloPrediccion = modeloPrediccion;
            _procesadorArchivos = procesadorArchivos;
        }

       
        /// Endpoint principal para predecir la siguiente palabra basado en el contexto proporcionado
        /// Utiliza modelos de bigramas y trigramas para generar sugerencias inteligentes
        
        [HttpPost("predecir")]
        public IActionResult PredecirSiguientePalabra([FromBody] PrediccionRequest request)
        {
            try
            {
                // Validación básica del contexto de entrada
                if (string.IsNullOrWhiteSpace(request?.Contexto))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "El contexto no puede estar vacío",
                        predicciones = new List<object>()
                    });
                }

                // Verificar si el modelo ha sido entrenado con datos
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

                // Preparar los parámetros de búsqueda
                var contexto = request.Contexto.Trim();
                var topK = Math.Min(Math.Max(request.TopK ?? 8, 1), 20); // Limitar entre 1 y 20 resultados

                _logger.LogDebug($"Generando predicciones para contexto: '{contexto}' con límite: {topK}");

                // Llamar al modelo de predicción desarrollado por el equipo
                var prediccionesRaw = _modeloPrediccion.PredecirSiguientePalabra(contexto, topK);

                // Si no hay predicciones, informar al usuario
                if (!prediccionesRaw.Any())
                {
                    _logger.LogInformation($"No se encontraron predicciones para el contexto: '{contexto}'");

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

                // Determinar qué método de predicción se está usando
                var palabrasContexto = contexto.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var metodoUsado = palabrasContexto.Length >= 2 ? "trigrama" : "bigrama";

                // Formatear las predicciones para enviar al frontend
                var prediccionesFormateadas = prediccionesRaw.Select((pred, index) => new {
                    palabra = pred.Palabra,
                    relevancia = pred.Archivos.Count, // Cuántos archivos contienen esta palabra
                    archivos = pred.Archivos,
                    metodo = metodoUsado,
                    posicion = index + 1,
                    confianza = CalcularConfianza(pred.Archivos.Count, prediccionesRaw.Count)
                }).ToList();

                _logger.LogInformation($"Devolviendo {prediccionesFormateadas.Count} predicciones usando método {metodoUsado}");

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
                _logger.LogError(ex, $"Error al generar predicciones para contexto: '{request?.Contexto}'");

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

        
        /// Obtiene las estadísticas actuales del modelo de predicción
        /// Incluye información sobre bigramas, trigramas y capacidad del modelo
       
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
                _logger.LogError(ex, "Error al obtener estadísticas del modelo");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

       
        /// Busca palabras que comiencen con un prefijo específico
        /// Útil para autocompletar mientras el usuario escribe
        
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

                var limite = Math.Min(Math.Max(request.Limite ?? 10, 1), 50); // Entre 1 y 50 resultados

                _logger.LogDebug($"Buscando palabras con prefijo: '{request.Prefijo}' con límite: {limite}");

                var resultados = _modeloPrediccion.BuscarPorPrefijo(request.Prefijo, limite);

                // Formatear resultados para el frontend
                var resultadosFormateados = resultados.Select(r => new {
                    palabra = r.Palabra,
                    archivos = r.Archivos,
                    relevancia = r.Relevancia,
                    fuentes = r.Archivos.Take(3).ToList(), // Mostrar solo las primeras 3 fuentes
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
                _logger.LogError(ex, $"Error al buscar prefijo: '{request?.Prefijo}'");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

      
        /// Endpoint para diagnóstico del sistema
        /// Proporciona información detallada sobre el estado del modelo y los datos procesados
       
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
                _logger.LogError(ex, "Error al obtener diagnóstico del sistema");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

      
        /// Valida si un contexto es válido para realizar predicciones
        /// Determina qué métodos de predicción están disponibles según el contexto
       
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

                // Crear objeto de validación con información útil
                var validacion = new
                {
                    contextValido = palabras.Length > 0,
                    numeroPalabras = palabras.Length,
                    puedeUsarTrigramas = palabras.Length >= 2, // Necesita al menos 2 palabras para trigramas
                    puedeUsarBigramas = palabras.Length >= 1,  // Necesita al menos 1 palabra para bigramas
                    metodosDisponibles = new List<string>(),
                    ultimaPalabra = palabras.LastOrDefault(),
                    penultimaPalabra = palabras.Length >= 2 ? palabras[^2] : null
                };

                // Determinar qué métodos están disponibles
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
                _logger.LogError(ex, $"Error al validar contexto: '{request?.Contexto}'");

                return StatusCode(500, new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        #region Métodos Auxiliares

       
        /// Calcula el porcentaje de confianza de una predicción
        /// Basado en la relevancia relativa dentro del conjunto de predicciones
        
        private double CalcularConfianza(int relevancia, int totalPredicciones)
        {
            if (totalPredicciones == 0) return 0.0;
            return Math.Round((double)relevancia / totalPredicciones * 100, 1);
        }

        
        /// Evalúa la precisión esperada del modelo basado en la cantidad de datos
        /// Más bigramas y trigramas generalmente significa mejor precisión
        
        private string CalcularPrecisionEsperada()
        {
            var totalBigramas = _modeloPrediccion.TotalBigramas;
            var totalTrigramas = _modeloPrediccion.TotalTrigramas;

            if (totalTrigramas > 1000) return "Alta";
            if (totalBigramas > 500) return "Media";
            if (totalBigramas > 100) return "Baja";
            return "Mínima";
        }

       
        /// Evalúa la cobertura del modelo basado en la cantidad de contextos disponibles
        /// Más contextos significa mejor cobertura de diferentes situaciones
        
        private string CalcularCobertura()
        {
            var totalContextos = _modeloPrediccion.TotalContextos;

            if (totalContextos > 5000) return "Amplia";
            if (totalContextos > 1000) return "Media";
            if (totalContextos > 100) return "Limitada";
            return "Mínima";
        }

        
    }

    // Modelos de Request

    
    /// Modelo para las peticiones de predicción de texto
    
    public class PrediccionRequest
    {
        
        /// Contexto de texto para generar la predicción
       
        [Required]
        public string? Contexto { get; set; }

       
        /// Número máximo de predicciones a devolver (entre 1 y 20)
        
        [Range(1, 20)]
        public int? TopK { get; set; } = 8;
    }

   
    /// Modelo para las peticiones de búsqueda por prefijo
   
    public class PrefijoBusquedaRequest
    {
        
        /// Prefijo a buscar en las palabras procesadas
       
        [Required]
        public string? Prefijo { get; set; }

        
        /// Límite de resultados a devolver (entre 1 y 50)
       
        [Range(1, 50)]
        public int? Limite { get; set; } = 10;
    }

 
    /// Modelo para las peticiones de validación de contexto
   
    public class ValidacionContextoRequest
    {
        
        /// Contexto a validar para predicciones
        
        [Required]
        public string? Contexto { get; set; }
    }

    #endregion
}