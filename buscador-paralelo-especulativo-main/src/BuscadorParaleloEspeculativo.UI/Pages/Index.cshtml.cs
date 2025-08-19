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

                // 🔥 RESULTADO MEJORADO - Agregar las nuevas métricas
                var resultado = new
                {
                    success = true,
                    
                    // Métricas básicas (sin cambios)
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
                    
                    // 🆕 NUEVAS MÉTRICAS PARA PROYECTO FINAL
                    analisisProcesadores = new {
                        disponibles = metricas.AnalisisProcesadores.ProcessorsDisponibles,
                        recomendados = metricas.AnalisisProcesadores.ProcessorsRecomendados,
                        optimos = metricas.AnalisisProcesadores.ProcessorsOptimos,
                        tipo = metricas.AnalisisProcesadores.TipoProcesador,
                        recomendacionHardware = metricas.AnalisisProcesadores.RecomendacionHardware,
                        justificacion = metricas.AnalisisProcesadores.JustificacionRecomendacion
                    },
                    
                    metricasEspecificas = new {
                        archivosSegundo = Math.Round(metricas.MetricasEspecificas.ArchivosPromedioSegundo, 2),
                        throughputPalabras = (long)metricas.MetricasEspecificas.ThroughputPalabras,
                        eficienciaMemoria = Math.Round(metricas.MetricasEspecificas.EficienciaMemoria, 1),
                        tiempoRespuestaPromedio = Math.Round(metricas.MetricasEspecificas.TiempoRespuestaPromedio, 0),
                        latenciaInicial = Math.Round(metricas.MetricasEspecificas.LatenciaInicial, 0),
                        rendimientoGeneral = metricas.MetricasEspecificas.RendimientoGeneral
                    },
                    
                    curvaRendimiento = metricas.CurvaRendimiento.ToDictionary(
                        x => x.Key.ToString(), 
                        x => Math.Round(x.Value, 2)
                    ),
                    
                    resultadosPruebas = metricas.ResultadosPruebas.Select(p => new {
                        cores = p.NumeroProcessors,
                        nombre = p.NombrePrueba,
                        tiempo = p.TiempoMs,
                        speedup = Math.Round(p.Speedup, 2),
                        eficiencia = Math.Round(p.Eficiencia * 100, 1),
                        parametros = p.ParametrosPrueba
                    }),
                    
                    // Fuentes (sin cambios)
                    fuentes = metricas.EstadoArchivos.Select(a => new {
                        nombre = a.NombreArchivo,
                        tamaño = a.TamañoLegible,
                        palabras = a.PalabrasProcesadas,
                        estado = a.Estado
                    }).ToList()
                };

                _logger.LogInformation($"Análisis completo completado. Speedup: {metricas.Speedup:F2}x ({metricas.EvaluacionSpeedup}), Procesadores: {metricas.AnalisisProcesadores.ProcessorsRecomendados}/{metricas.AnalisisProcesadores.ProcessorsDisponibles}");

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

        // 🆕 NUEVO ENDPOINT: Obtener análisis de sistema
        public IActionResult OnPostAnalisisSistema()
        {
            try
            {
                var analisis = new {
                    procesadores = new {
                        disponibles = Environment.ProcessorCount,
                        recomendados = Math.Max(1, (int)(Environment.ProcessorCount * 0.75)),
                        arquitectura = Environment.Is64BitProcess ? "64-bit" : "32-bit",
                        machineName = Environment.MachineName
                    },
                    memoria = new {
                        disponible = GC.GetTotalMemory(false),
                        estimadaTotal = "N/A" // Se puede agregar con WMI si se requiere
                    },
                    sistema = new {
                        os = Environment.OSVersion.ToString(),
                        framework = Environment.Version.ToString(),
                        tiempoInicio = DateTime.Now
                    }
                };
                
                return new JsonResult(new { success = true, analisis });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo análisis del sistema");
                return new JsonResult(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Ejecuta la batería completa de pruebas del sistema
        /// </summary>
        public async Task<IActionResult> OnPostEjecutarPruebasAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando batería de pruebas del sistema");

                var testRunner = new BuscadorParaleloEspeculativo.UI.Tests.TestScenarios();
                var resultados = await testRunner.EjecutarTodasLasPruebasAsync();

                var respuesta = new
                {
                    success = true,
                    fechaEjecucion = DateTime.Now,
                    resumen = new
                    {
                        totalEscenarios = resultados.EscenariosPrueba.Count,
                        escenariosExitosos = resultados.EscenariosPrueba.Count(e => e.Exitoso),
                        todosPasaron = resultados.TodosLosTestsPasaron,
                        speedupPromedio = resultados.EscenariosPrueba.Where(e => e.Exitoso && e.Metricas != null)
                            .DefaultIfEmpty()
                            .Average(e => e?.Metricas?.Speedup ?? 0)
                    },
                    
                    escenarios = resultados.EscenariosPrueba.Select(e => new
                    {
                        nombre = e.NombreEscenario,
                        exitoso = e.Exitoso,
                        archivos = e.NumeroArchivos,
                        tiempoMs = e.TiempoTotal,
                        speedup = e.Metricas?.Speedup ?? 0,
                        eficiencia = (e.Metricas?.Eficiencia ?? 0) * 100,
                        throughputArchivos = Math.Round(e.ThroughputArchivos, 2),
                        tamañoPromedio = FormatearBytes((long)e.TamañoPromedio),
                        error = e.ErrorMensaje
                    }),
                    
                    consistencia = resultados.PruebaConsistencia != null ? new
                    {
                        ejecuciones = resultados.PruebaConsistencia.EjecucionesExitosas,
                        consistente = resultados.PruebaConsistencia.EsConsistente,
                        speedupPromedio = Math.Round(resultados.PruebaConsistencia.SpeedupPromedio, 2),
                        speedupVariacion = Math.Round(resultados.PruebaConsistencia.SpeedupDesviacion, 2),
                        tiempoPromedio = Math.Round(resultados.PruebaConsistencia.TiempoPromedio, 0),
                        tiempoVariacion = Math.Round(resultados.PruebaConsistencia.TiempoDesviacion, 0)
                    } : null,
                    
                    escalabilidad = resultados.PruebaEscalabilidad != null ? new
                    {
                        configuracionOptima = resultados.PruebaEscalabilidad.ConfiguracionOptima,
                        mejorSpeedup = Math.Round(resultados.PruebaEscalabilidad.MejorSpeedup, 2),
                        mejorEficiencia = Math.Round(resultados.PruebaEscalabilidad.MejorEficiencia * 100, 1),
                        resultadosPorCores = resultados.PruebaEscalabilidad.ResultadosPorCores.ToDictionary(
                            kvp => kvp.Key.ToString(),
                            kvp => new
                            {
                                cores = kvp.Value.Cores,
                                speedup = Math.Round(kvp.Value.Speedup, 2),
                                eficiencia = Math.Round(kvp.Value.Eficiencia * 100, 1),
                                tiempoMs = kvp.Value.TiempoMs
                            }
                        )
                    } : null,
                    
                    modeloPredictivo = resultados.ValidacionModelo != null ? new
                    {
                        entrenado = resultados.ValidacionModelo.ModeloEntrenado,
                        bigramas = resultados.ValidacionModelo.BigramasEntrenados,
                        trigramas = resultados.ValidacionModelo.TrigramasEntrenados,
                        exitoPrediccion = Math.Round(resultados.ValidacionModelo.PorcentajeExitoPredicion, 1),
                        pruebas = resultados.ValidacionModelo.PrediccionesPrueba.Select(p => new
                        {
                            contexto = p.Contexto,
                            predicciones = p.NumeroPredicciones,
                            exitoso = p.TienePredicciones
                        }),
                        error = resultados.ValidacionModelo.ErrorEntrenamiento
                    } : null,
                    
                    recomendaciones = GenerarRecomendacionesBasedOnTests(resultados)
                };

                _logger.LogInformation($"Batería de pruebas completada. Estado general: {(resultados.TodosLosTestsPasaron ? "ÉXITO" : "CON FALLOS")}");

                return new JsonResult(respuesta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando batería de pruebas");
                return new JsonResult(new
                {
                    success = false,
                    message = $"Error ejecutando pruebas: {ex.Message}",
                    fechaEjecucion = DateTime.Now
                });
            }
        }

        /// <summary>
        /// Genera recomendaciones basadas en los resultados de las pruebas
        /// </summary>
        private object GenerarRecomendacionesBasedOnTests(BuscadorParaleloEspeculativo.UI.Tests.TestResults resultados)
        {
            var recomendaciones = new List<string>();
            var configuracionOptima = new Dictionary<string, object>();

            // Análisis de escalabilidad
            if (resultados.PruebaEscalabilidad?.ResultadosPorCores?.Any() == true)
            {
                var mejorConfig = resultados.PruebaEscalabilidad.ResultadosPorCores.Values
                    .OrderByDescending(r => r.Eficiencia)
                    .First();
                    
                configuracionOptima["coresRecomendados"] = mejorConfig.Cores;
                configuracionOptima["speedupEsperado"] = Math.Round(mejorConfig.Speedup, 2);
                configuracionOptima["eficienciaEsperada"] = Math.Round(mejorConfig.Eficiencia * 100, 1);

                if (mejorConfig.Eficiencia > 0.8)
                {
                    recomendaciones.Add($"✅ Excelente escalabilidad con {mejorConfig.Cores} cores ({mejorConfig.Eficiencia * 100:F1}% eficiencia)");
                }
                else if (mejorConfig.Eficiencia > 0.6)
                {
                    recomendaciones.Add($"⚡ Buena escalabilidad con {mejorConfig.Cores} cores. Considere optimizar E/O para mejor eficiencia.");
                }
                else
                {
                    recomendaciones.Add($"⚠️ Escalabilidad limitada. El cuello de botella probablemente sea E/O de disco o memoria.");
                }

                // Análisis de degradación por exceso de cores
                var maxCores = resultados.PruebaEscalabilidad.ResultadosPorCores.Keys.Max();
                var resultadoMax = resultados.PruebaEscalabilidad.ResultadosPorCores[maxCores];
                
                if (resultadoMax.Eficiencia < mejorConfig.Eficiencia * 0.8)
                {
                    recomendaciones.Add($"📉 Usar más de {mejorConfig.Cores} cores reduce la eficiencia por overhead de sincronización.");
                }
            }

            // Análisis de consistencia
            if (resultados.PruebaConsistencia != null)
            {
                if (resultados.PruebaConsistencia.EsConsistente)
                {
                    recomendaciones.Add("🎯 Sistema muestra rendimiento consistente entre ejecuciones.");
                }
                else
                {
                    var variacion = (resultados.PruebaConsistencia.SpeedupDesviacion / resultados.PruebaConsistencia.SpeedupPromedio) * 100;
                    recomendaciones.Add($"⚠️ Variabilidad del {variacion:F1}% en rendimiento. Posibles causas: carga del sistema o GC de .NET.");
                }
            }

            // Análisis de tamaño óptimo de archivos
            var escenariosExitosos = resultados.EscenariosPrueba.Where(e => e.Exitoso && e.Metricas != null).ToList();
            if (escenariosExitosos.Any())
            {
                var mejorEscenario = escenariosExitosos.OrderByDescending(e => e.Metricas.Speedup).First();
                
                if (mejorEscenario.NombreEscenario.Contains("Mediano"))
                {
                    recomendaciones.Add("📁 Archivos de tamaño mediano (10-50KB) ofrecen el mejor balance rendimiento/overhead.");
                }
                else if (mejorEscenario.NombreEscenario.Contains("Grande"))
                {
                    recomendaciones.Add("📚 Archivos grandes muestran mejor speedup, pero considere el uso de memoria.");
                }
                else if (mejorEscenario.NombreEscenario.Contains("Pequeño"))
                {
                    recomendaciones.Add("⚡ Archivos pequeños son eficientes, pero pueden sufrir de overhead paralelo.");
                }

                configuracionOptima["tipoArchivoOptimo"] = mejorEscenario.NombreEscenario;
                configuracionOptima["speedupOptimo"] = Math.Round(mejorEscenario.Metricas.Speedup, 2);
            }

            // Análisis del modelo predictivo
            if (resultados.ValidacionModelo?.ModeloEntrenado == true)
            {
                if (resultados.ValidacionModelo.PorcentajeExitoPredicion > 80)
                {
                    recomendaciones.Add($"🧠 Modelo predictivo funciona excelentemente ({resultados.ValidacionModelo.PorcentajeExitoPredicion:F1}% éxito).");
                }
                else if (resultados.ValidacionModelo.PorcentajeExitoPredicion > 60)
                {
                    recomendaciones.Add($"🤔 Modelo predictivo funciona bien, pero podría mejorar con más datos de entrenamiento.");
                }
                else
                {
                    recomendaciones.Add("❌ Modelo predictivo necesita optimización o más datos de entrenamiento.");
                }

                configuracionOptima["nGramasTotal"] = resultados.ValidacionModelo.BigramasEntrenados + resultados.ValidacionModelo.TrigramasEntrenados;
            }

            // Recomendaciones de hardware
            var coresDisponibles = Environment.ProcessorCount;
            if (resultados.PruebaEscalabilidad?.ConfiguracionOptima > 0)
            {
                var coresOptimos = resultados.PruebaEscalabilidad.ConfiguracionOptima;
                
                if (coresOptimos >= coresDisponibles)
                {
                    recomendaciones.Add($"🖥️ Sistema utiliza todos los cores disponibles eficientemente. Hardware actual es adecuado.");
                }
                else
                {
                    var porcentajeUso = (double)coresOptimos / coresDisponibles * 100;
                    recomendaciones.Add($"💡 Sistema utiliza {porcentajeUso:F0}% de cores disponibles. Cores adicionales no mejoran rendimiento.");
                }

                if (coresDisponibles >= 8 && coresOptimos >= 6)
                {
                    configuracionOptima["tipoHardware"] = "Workstation/Server (ideal para este workload)";
                }
                else if (coresDisponibles >= 4 && coresOptimos >= 3)
                {
                    configuracionOptima["tipoHardware"] = "Desktop moderno (adecuado)";
                }
                else
                {
                    configuracionOptima["tipoHardware"] = "Hardware limitado (considere upgrade)";
                }
            }

            return new
            {
                recomendaciones = recomendaciones,
                configuracionOptima = configuracionOptima,
                fechaAnalisis = DateTime.Now,
                validez = "Recomendaciones basadas en pruebas ejecutadas en este hardware específico"
            };
        }

        /// <summary>
        /// Formatea bytes a unidades legibles
        /// </summary>
        private string FormatearBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
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
