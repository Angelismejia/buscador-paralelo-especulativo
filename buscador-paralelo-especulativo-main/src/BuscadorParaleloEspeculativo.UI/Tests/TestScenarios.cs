

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using BuscadorParaleloEspeculativo.UI.Models;

namespace BuscadorParaleloEspeculativo.UI.Tests
{
    
    public class TestScenarios
    {
        private readonly ProcesadorArchivos _procesador;
        private readonly ModeloPrediccion _modelo;

        public TestScenarios()
        {
            _procesador = new ProcesadorArchivos();
            _modelo = new ModeloPrediccion();
        }

      
        public async Task<TestResults> EjecutarTodasLasPruebasAsync()
        {
            var resultados = new TestResults();
            
            Console.WriteLine("🧪 INICIANDO BATERÍA DE PRUEBAS DEL SISTEMA");
            Console.WriteLine("=".PadRight(60, '='));

            try
            {
                // Crear archivos de prueba con diferentes tamaños y contenidos
                var archivosTest = await CrearArchivosDeTestAsync();

                // ESCENARIO 1: Archivos pequeños (rapidez)
                Console.WriteLine("\n🔬 ESCENARIO 1: Archivos pequeños (1-5KB)");
                var resultado1 = await PruebaCarga(archivosTest.ArchivosPequeños, "Archivos Pequeños");
                resultados.EscenariosPrueba.Add(resultado1);

                // ESCENARIO 2: Archivos medianos (balance)
                Console.WriteLine("\n🔬 ESCENARIO 2: Archivos medianos (10-50KB)");
                var resultado2 = await PruebaCarga(archivosTest.ArchivosMedianos, "Archivos Medianos");
                resultados.EscenariosPrueba.Add(resultado2);

                // ESCENARIO 3: Archivos grandes (throughput)
                Console.WriteLine("\n🔬 ESCENARIO 3: Archivos grandes (100KB+)");
                var resultado3 = await PruebaCarga(archivosTest.ArchivosGrandes, "Archivos Grandes");
                resultados.EscenariosPrueba.Add(resultado3);

                // ESCENARIO 4: Mezcla de archivos (realista)
                Console.WriteLine("\n🔬 ESCENARIO 4: Mezcla realista de tamaños");
                var todosMezclados = new List<string>();
                todosMezclados.AddRange(archivosTest.ArchivosPequeños);
                todosMezclados.AddRange(archivosTest.ArchivosMedianos);
                todosMezclados.AddRange(archivosTest.ArchivosGrandes);
                var resultado4 = await PruebaCarga(todosMezclados, "Mezcla Realista");
                resultados.EscenariosPrueba.Add(resultado4);

                // ESCENARIO 5: Prueba de consistencia (3 ejecuciones del mismo conjunto)
                Console.WriteLine("\n🔬 ESCENARIO 5: Prueba de consistencia (3 ejecuciones)");
                var resultadosConsistencia = await PruebaConsistencia(archivosTest.ArchivosMedianos);
                resultados.PruebaConsistencia = resultadosConsistencia;

                // ESCENARIO 6: Prueba de escalabilidad (diferentes números de cores)
                Console.WriteLine("\n🔬 ESCENARIO 6: Prueba de escalabilidad");
                var resultadosEscalabilidad = await PruebaEscalabilidad(archivosTest.ArchivosMedianos);
                resultados.PruebaEscalabilidad = resultadosEscalabilidad;

                // VALIDAR MODELO DE PREDICCIÓN
                Console.WriteLine("\n🔬 VALIDANDO MODELO DE PREDICCIÓN...");
                var validacionModelo = await ValidarModeloPrediccion(todosMezclados);
                resultados.ValidacionModelo = validacionModelo;

                // GENERAR REPORTE FINAL
                GenerarReporteFinal(resultados);

                LimpiarArchivosDeTest(archivosTest);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en pruebas: {ex.Message}");
                resultados.ErrorGeneral = ex.Message;
            }

            return resultados;
        }

        private async Task<ArchivosTest> CrearArchivosDeTestAsync()
        {
            var carpetaTest = Path.Combine(Path.GetTempPath(), "TestParalelo", Guid.NewGuid().ToString());
            Directory.CreateDirectory(carpetaTest);

            var archivos = new ArchivosTest { CarpetaBase = carpetaTest };

            // Generar contenido base
            var contenidoBase = GenerarContenidoTexto();

            // ARCHIVOS PEQUEÑOS (1-5KB)
            for (int i = 1; i <= 5; i++)
            {
                var rutaArchivo = Path.Combine(carpetaTest, $"pequeño_{i}.txt");
                var contenido = contenidoBase.Substring(0, Math.Min(contenidoBase.Length, 1000 + i * 800));
                await File.WriteAllTextAsync(rutaArchivo, contenido);
                archivos.ArchivosPequeños.Add(rutaArchivo);
            }

            // ARCHIVOS MEDIANOS (10-50KB)
            for (int i = 1; i <= 4; i++)
            {
                var rutaArchivo = Path.Combine(carpetaTest, $"mediano_{i}.txt");
                var contenido = RepetirContenido(contenidoBase, 10 + i * 5);
                await File.WriteAllTextAsync(rutaArchivo, contenido);
                archivos.ArchivosMedianos.Add(rutaArchivo);
            }

            // ARCHIVOS GRANDES (100KB+)
            for (int i = 1; i <= 3; i++)
            {
                var rutaArchivo = Path.Combine(carpetaTest, $"grande_{i}.txt");
                var contenido = RepetirContenido(contenidoBase, 80 + i * 20);
                await File.WriteAllTextAsync(rutaArchivo, contenido);
                archivos.ArchivosGrandes.Add(rutaArchivo);
            }

            return archivos;
        }

        private string GenerarContenidoTexto()
        {
            return @"
El procesamiento paralelo de documentos representa una evolución significativa en el análisis de texto.
Esta tecnología permite procesar múltiples archivos simultáneamente, aprovechando todos los cores del procesador.
La eficiencia del sistema depende de varios factores: el tamaño de los archivos, el tipo de procesador y la carga del sistema.

En el contexto de la predicción de texto, el modelo utiliza bigramas y trigramas para generar sugerencias.
Los bigramas son secuencias de dos palabras consecutivas que ayudan a predecir la siguiente palabra.
Los trigramas extienden este concepto a tres palabras, proporcionando mayor precisión en las predicciones.

La arquitectura del sistema implementa patrones concurrentes que maximizan el throughput.
Cada archivo se procesa en paralelo, extrayendo palabras y construyendo el vocabulario del modelo.
El balanceador de carga distribuye el trabajo entre los cores disponibles de manera óptima.

La medición del rendimiento utiliza métricas como speedup, eficiencia y throughput de palabras.
El speedup indica cuántas veces más rápido es el procesamiento paralelo comparado con el secuencial.
La eficiencia mide qué tan bien se utilizan los recursos del procesador.

Los resultados experimentales demuestran que el paralelismo es más efectivo con archivos de tamaño mediano.
Los archivos muy pequeños sufren de overhead de sincronización.
Los archivos muy grandes pueden saturar la memoria y causar contención por recursos de E/S.

La implementación utiliza estructuras thread-safe como ConcurrentDictionary y ConcurrentBag.
Estas garantizan la consistencia de los datos durante el procesamiento concurrente.
El algoritmo de tokenización divide el texto en palabras y elimina caracteres especiales.

El modelo predictivo entrena con los contextos extraídos de todos los archivos procesados.
Utiliza análisis de frecuencias para determinar las palabras más probables.
La interfaz de usuario muestra las predicciones en tiempo real mientras el usuario escribe.
";
        }

        private string RepetirContenido(string contenido, int veces)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < veces; i++)
            {
                sb.AppendLine($"=== Sección {i + 1} ===");
                sb.AppendLine(contenido);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private async Task<ResultadoEscenario> PruebaCarga(List<string> archivos, string nombreEscenario)
        {
            var escenario = new ResultadoEscenario
            {
                NombreEscenario = nombreEscenario,
                NumeroArchivos = archivos.Count,
                FechaEjecucion = DateTime.Now
            };

            try
            {
                Console.WriteLine($"   📁 Procesando {archivos.Count} archivos...");

                var stopwatch = Stopwatch.StartNew();
                var metricas = await _procesador.AnalisisCompletoRendimientoAsync(archivos.ToArray());
                stopwatch.Stop();

                escenario.TiempoTotal = stopwatch.ElapsedMilliseconds;
                escenario.Metricas = metricas;
                escenario.Exitoso = true;

                // Cálculos específicos del escenario
                escenario.TamañoPromedio = archivos.Average(a => new FileInfo(a).Length);
                escenario.ThroughputArchivos = archivos.Count / (stopwatch.ElapsedMilliseconds / 1000.0);
                
                Console.WriteLine($"   ✅ Completado en {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"   📊 Speedup: {metricas.Speedup:F2}x, Throughput: {escenario.ThroughputArchivos:F2} archivos/seg");

                _procesador.LimpiarDatos();
            }
            catch (Exception ex)
            {
                escenario.Exitoso = false;
                escenario.ErrorMensaje = ex.Message;
                Console.WriteLine($"   ❌ Error: {ex.Message}");
            }

            return escenario;
        }

        private async Task<PruebaConsistencia> PruebaConsistencia(List<string> archivos)
        {
            var prueba = new PruebaConsistencia();
            var resultados = new List<MetricasProcesamiento>();

            for (int i = 1; i <= 3; i++)
            {
                Console.WriteLine($"   🔄 Ejecución {i}/3...");
                try
                {
                    var metricas = await _procesador.AnalisisCompletoRendimientoAsync(archivos.ToArray());
                    resultados.Add(metricas);
                    _procesador.LimpiarDatos();
                    
                    // Pausa pequeña entre ejecuciones
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error en ejecución {i}: {ex.Message}");
                }
            }

            if (resultados.Count >= 2)
            {
                prueba.EjecucionesExitosas = resultados.Count;
                prueba.SpeedupPromedio = resultados.Average(r => r.Speedup);
                prueba.SpeedupDesviacion = CalcularDesviacionEstandar(resultados.Select(r => r.Speedup).ToList());
                prueba.TiempoPromedio = resultados.Average(r => r.TiempoParaleloMs);
                prueba.TiempoDesviacion = CalcularDesviacionEstandar(resultados.Select(r => (double)r.TiempoParaleloMs).ToList());
                
                // Considerar consistente si la desviación del speedup es < 10%
                prueba.EsConsistente = prueba.SpeedupDesviacion / prueba.SpeedupPromedio < 0.1;
                
                Console.WriteLine($"   📊 Speedup promedio: {prueba.SpeedupPromedio:F2}x ± {prueba.SpeedupDesviacion:F2}");
                Console.WriteLine($"   ⏱️ Tiempo promedio: {prueba.TiempoPromedio:F0}ms ± {prueba.TiempoDesviacion:F0}");
                Console.WriteLine($"   ✅ Consistencia: {(prueba.EsConsistente ? "BUENA" : "VARIABLE")}");
            }

            return prueba;
        }

        private async Task<PruebaEscalabilidad> PruebaEscalabilidad(List<string> archivos)
        {
            var prueba = new PruebaEscalabilidad();
            var maxCores = Environment.ProcessorCount;
            var configuraciones = new List<int> { 1, 2 };
            
            if (maxCores >= 4) configuraciones.Add(4);
            if (maxCores >= 6) configuraciones.Add(maxCores / 2);
            if (maxCores >= 8) configuraciones.Add((int)(maxCores * 0.75));
            configuraciones.Add(maxCores);

            foreach (var cores in configuraciones.Distinct().OrderBy(x => x))
            {
                Console.WriteLine($"   ⚙️ Probando con {cores} cores...");
                try
                {
                    var metricas = await _procesador.AnalisisCompletoRendimientoAsync(archivos.ToArray());
                    var mejorPrueba = metricas.ResultadosPruebas.FirstOrDefault(p => p.NumeroProcessors == cores);
                    
                    if (mejorPrueba != null)
                    {
                        prueba.ResultadosPorCores[cores] = new ResultadoEscalabilidad
                        {
                            Cores = cores,
                            Speedup = mejorPrueba.Speedup,
                            Eficiencia = mejorPrueba.Eficiencia,
                            TiempoMs = mejorPrueba.TiempoMs
                        };
                        
                        Console.WriteLine($"  📈 Speedup: {mejorPrueba.Speedup:F2}x, Eficiencia: {mejorPrueba.Eficiencia * 100:F1}%");
                    }
                    
                    _procesador.LimpiarDatos();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   ❌ Error con {cores} cores: {ex.Message}");
                }
            }

            // Encontrar configuración óptima
            if (prueba.ResultadosPorCores.Any())
            {
                var mejorEficiencia = prueba.ResultadosPorCores.Values.OrderByDescending(r => r.Eficiencia).First();
                var mejorSpeedup = prueba.ResultadosPorCores.Values.OrderByDescending(r => r.Speedup).First();
                
                prueba.ConfiguracionOptima = mejorEficiencia.Cores;
                prueba.MejorSpeedup = mejorSpeedup.Speedup;
                prueba.MejorEficiencia = mejorEficiencia.Eficiencia;
                
                Console.WriteLine($"   🎯 Configuración óptima: {prueba.ConfiguracionOptima} cores");
                Console.WriteLine($"   🚀 Mejor speedup: {prueba.MejorSpeedup:F2}x con {mejorSpeedup.Cores} cores");
                Console.WriteLine($"   📊 Mejor eficiencia: {prueba.MejorEficiencia * 100:F1}% con {mejorEficiencia.Cores} cores");
            }

            return prueba;
        }

        private async Task<ValidacionModelo> ValidarModeloPrediccion(List<string> archivos)
        {
            var validacion = new ValidacionModelo();

            try
            {
                // Procesar archivos y entrenar modelo
                var metricas = await _procesador.AnalisisCompletoRendimientoAsync(archivos.ToArray());
                var contextos = _procesador.ObtenerContextosPalabras();
                
                if (contextos.Any())
                {
                    _modelo.EntrenarModelo(contextos);
                    validacion.ModeloEntrenado = true;
                    validacion.BigramasEntrenados = _modelo.TotalBigramas;
                    validacion.TrigramasEntrenados = _modelo.TotalTrigramas;
                    
                    // Probar predicciones con frases de prueba
                    var frasesPrueba = new[] { "el procesamiento", "sistema de", "archivos grandes" };
                    
                    foreach (var frase in frasesPrueba)
                    {
                        var predicciones = _modelo.PredecirSiguientePalabra(frase, 5);
                        validacion.PrediccionesPrueba.Add(new PruebaPredicion
                        {
                            Contexto = frase,
                            NumeroPredicciones = predicciones.Count,
                            TienePredicciones = predicciones.Any()
                        });
                    }
                    
                    validacion.PorcentajeExitoPredicion = validacion.PrediccionesPrueba.Count(p => p.TienePredicciones) 
                        / (double)validacion.PrediccionesPrueba.Count * 100;
                    
                    Console.WriteLine($"  Modelo entrenado: {validacion.BigramasEntrenados} bigramas, {validacion.TrigramasEntrenados} trigramas");
                    Console.WriteLine($"  Éxito predicción: {validacion.PorcentajeExitoPredicion:F1}%");
                }
            }
            catch (Exception ex)
            {
                validacion.ErrorEntrenamiento = ex.Message;
                Console.WriteLine($"  Error validando modelo: {ex.Message}");
            }

            return validacion;
        }

        private double CalcularDesviacionEstandar(List<double> valores)
        {
            if (valores.Count < 2) return 0;
            
            double promedio = valores.Average();
            double sumaCuadrados = valores.Sum(v => Math.Pow(v - promedio, 2));
            return Math.Sqrt(sumaCuadrados / (valores.Count - 1));
        }

        private void GenerarReporteFinal(TestResults resultados)
        {
            Console.WriteLine("\n" + "🏆".PadRight(60, '='));
            Console.WriteLine("                    REPORTE FINAL DE PRUEBAS");
            Console.WriteLine("🏆".PadRight(60, "🏆"[0]));

            Console.WriteLine($"\n📊 RESUMEN GENERAL:");
            Console.WriteLine($"   Total escenarios: {resultados.EscenariosPrueba.Count}");
            Console.WriteLine($"   Escenarios exitosos: {resultados.EscenariosPrueba.Count(e => e.Exitoso)}");
            
            if (resultados.EscenariosPrueba.Any(e => e.Exitoso))
            {
                var mejorEscenario = resultados.EscenariosPrueba.Where(e => e.Exitoso).OrderByDescending(e => e.Metricas.Speedup).First();
                Console.WriteLine($"   Mejor rendimiento: {mejorEscenario.NombreEscenario} ({mejorEscenario.Metricas.Speedup:F2}x speedup)");
                
                var speedupPromedio = resultados.EscenariosPrueba.Where(e => e.Exitoso).Average(e => e.Metricas.Speedup);
                Console.WriteLine($"   Speedup promedio: {speedupPromedio:F2}x");
            }

            if (resultados.PruebaConsistencia != null)
            {
                Console.WriteLine($"\n🔄 CONSISTENCIA:");
                Console.WriteLine($"   Estado: {(resultados.PruebaConsistencia.EsConsistente ? "✅ BUENA" : "⚠️ VARIABLE")}");
                Console.WriteLine($"   Variabilidad speedup: ±{resultados.PruebaConsistencia.SpeedupDesviacion:F2}x");
            }

            if (resultados.PruebaEscalabilidad != null && resultados.PruebaEscalabilidad.ResultadosPorCores.Any())
            {
                Console.WriteLine($"\n📈 ESCALABILIDAD:");
                Console.WriteLine($"   Configuración óptima: {resultados.PruebaEscalabilidad.ConfiguracionOptima} cores");
                Console.WriteLine($"   Mejor speedup: {resultados.PruebaEscalabilidad.MejorSpeedup:F2}x");
                Console.WriteLine($"   Mejor eficiencia: {resultados.PruebaEscalabilidad.MejorEficiencia * 100:F1}%");
            }

            if (resultados.ValidacionModelo != null)
            {
                Console.WriteLine($"\n🧠 MODELO PREDICTIVO:");
                Console.WriteLine($"   Estado: {(resultados.ValidacionModelo.ModeloEntrenado ? "✅ ENTRENADO" : "❌ ERROR")}");
                if (resultados.ValidacionModelo.ModeloEntrenado)
                {
                    Console.WriteLine($"   N-gramas: {resultados.ValidacionModelo.BigramasEntrenados + resultados.ValidacionModelo.TrigramasEntrenados:N0}");
                    Console.WriteLine($"   Éxito predicción: {resultados.ValidacionModelo.PorcentajeExitoPredicion:F1}%");
                }
            }

            Console.WriteLine($"\n✅ ESTADO GENERAL: {(resultados.TodosLosTestsPasaron ? "TODOS LOS TESTS PASARON" : "HAY FALLOS EN ALGUNOS TESTS")}");
            Console.WriteLine("=".PadRight(60, '='));
        }

        private void LimpiarArchivosDeTest(ArchivosTest archivos)
        {
            try
            {
                if (Directory.Exists(archivos.CarpetaBase))
                {
                    Directory.Delete(archivos.CarpetaBase, true);
                    Console.WriteLine("🗑️ Archivos de test limpiados");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudieron limpiar archivos de test: {ex.Message}");
            }
        }
    }

    // CLASES DE DATOS PARA LOS RESULTADOS DE PRUEBAS

    public class TestResults
    {
        public List<ResultadoEscenario> EscenariosPrueba { get; set; } = new List<ResultadoEscenario>();
        public PruebaConsistencia? PruebaConsistencia { get; set; }
        public PruebaEscalabilidad? PruebaEscalabilidad { get; set; }
        public ValidacionModelo? ValidacionModelo { get; set; }
        public string ErrorGeneral { get; set; } = string.Empty;
        
        public bool TodosLosTestsPasaron => 
            EscenariosPrueba.All(e => e.Exitoso) && 
            (PruebaConsistencia?.EsConsistente ?? true) && 
            (ValidacionModelo?.ModeloEntrenado ?? false) &&
            string.IsNullOrEmpty(ErrorGeneral);
    }

    public class ResultadoEscenario
    {
        public string NombreEscenario { get; set; } = string.Empty;
        public int NumeroArchivos { get; set; }
        public long TiempoTotal { get; set; }
        public MetricasProcesamiento? Metricas { get; set; }
        public bool Exitoso { get; set; }
        public string ErrorMensaje { get; set; } = string.Empty;
        public DateTime FechaEjecucion { get; set; }
        public double TamañoPromedio { get; set; }
        public double ThroughputArchivos { get; set; }
    }

    public class PruebaConsistencia
    {
        public int EjecucionesExitosas { get; set; }
        public double SpeedupPromedio { get; set; }
        public double SpeedupDesviacion { get; set; }
        public double TiempoPromedio { get; set; }
        public double TiempoDesviacion { get; set; }
        public bool EsConsistente { get; set; }
    }

    public class PruebaEscalabilidad
    {
        public Dictionary<int, ResultadoEscalabilidad> ResultadosPorCores { get; set; } = new Dictionary<int, ResultadoEscalabilidad>();
        public int ConfiguracionOptima { get; set; }
        public double MejorSpeedup { get; set; }
        public double MejorEficiencia { get; set; }
    }

    public class ResultadoEscalabilidad
    {
        public int Cores { get; set; }
        public double Speedup { get; set; }
        public double Eficiencia { get; set; }
        public long TiempoMs { get; set; }
    }

    public class ValidacionModelo
    {
        public bool ModeloEntrenado { get; set; }
        public int BigramasEntrenados { get; set; }
        public int TrigramasEntrenados { get; set; }
        public List<PruebaPredicion> PrediccionesPrueba { get; set; } = new List<PruebaPredicion>();
        public double PorcentajeExitoPredicion { get; set; }
        public string ErrorEntrenamiento { get; set; } = string.Empty;
    }

    public class PruebaPredicion
    {
        public string Contexto { get; set; } = string.Empty;
        public int NumeroPredicciones { get; set; }
        public bool TienePredicciones { get; set; }
    }

    public class ArchivosTest
    {
        public string CarpetaBase { get; set; } = string.Empty;
        public List<string> ArchivosPequeños { get; set; } = new List<string>();
        public List<string> ArchivosMedianos { get; set; } = new List<string>();
        public List<string> ArchivosGrandes { get; set; } = new List<string>();
    }
}
