<<<<<<< Updated upstream:src/BuscadorParaleloEspeculativo.UI/Models/ModeloPrediccion.cs
﻿using System;
=======
using BuscadorParaleloEspeculativo.UI.Models;
using System;
>>>>>>> Stashed changes:buscador-paralelo-especulativo-main/src/BuscadorParaleloEspeculativo.UI/Models/ModeloPrediccion.cs
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


public class ModeloPrediccion
{
    // Diccionario para almacenar bigramas: palabra1 -> { palabra2: [archivos] }
    private Dictionary<string, Dictionary<string, HashSet<string>>> bigramas;

    // Diccionario para almacenar trigramas: "palabra1 palabra2" -> { palabra3: [archivos] }
    private Dictionary<string, Dictionary<string, HashSet<string>>> trigramas;

    // Propiedades para obtener estadísticas del modelo
    public int TotalBigramas => bigramas.Count;
    public int TotalTrigramas => trigramas.Count;
    public int TotalContextos { get; private set; }

    public ModeloPrediccion()
    {
        // Inicializar las estructuras de datos para almacenar n-gramas
        bigramas = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        trigramas = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        TotalContextos = 0;
    }

    /// <summary>
    /// Entrena el modelo con contextos de palabras del procesador
    /// </summary>
    public void EntrenarModelo(List<ContextoPalabra> contextos)
    {
        Console.WriteLine($"[ModeloPrediccion] Iniciando entrenamiento con {contextos.Count} contextos...");

        // Limpiar los datos del modelo anterior antes de entrenar
        bigramas.Clear();
        trigramas.Clear();
        TotalContextos = contextos.Count;

        // Contadores para mostrar progreso del entrenamiento
        int bigramasAgregados = 0;
        int trigramasAgregados = 0;

        // Agrupar contextos por archivo para procesamiento más eficiente
        var contextosPorArchivo = contextos.GroupBy(c => c.ArchivoOrigen).ToList();

        foreach (var grupo in contextosPorArchivo)
        {
            // Ordenar contextos por posición dentro del archivo
            var contextosArchivo = grupo.OrderBy(c => c.Posicion).ToList();
            Console.WriteLine($"[ModeloPrediccion] Procesando {contextosArchivo.Count} contextos de {grupo.Key}");

            for (int i = 0; i < contextosArchivo.Count; i++)
            {
                var contexto = contextosArchivo[i];

                // CREAR BIGRAMAS: palabraAnterior -> palabraActual
                if (!string.IsNullOrEmpty(contexto.PalabraAnterior) &&
                    !string.IsNullOrEmpty(contexto.PalabraActual))
                {
                    // Limpiar las palabras antes de procesarlas
                    string palabraAnterior = LimpiarPalabra(contexto.PalabraAnterior);
                    string palabraActual = LimpiarPalabra(contexto.PalabraActual);

                    // Solo procesar si ambas palabras son válidas
                    if (EsPalabraValida(palabraAnterior) && EsPalabraValida(palabraActual))
                    {
                        // Crear entrada en el diccionario si no existe
                        if (!bigramas.ContainsKey(palabraAnterior))
                            bigramas[palabraAnterior] = new Dictionary<string, HashSet<string>>();

                        // Crear entrada para la palabra siguiente si no existe
                        if (!bigramas[palabraAnterior].ContainsKey(palabraActual))
                            bigramas[palabraAnterior][palabraActual] = new HashSet<string>();

                        // Agregar el archivo origen a la lista de fuentes
                        bigramas[palabraAnterior][palabraActual].Add(contexto.ArchivoOrigen);
                        bigramasAgregados++;
                    }
                }

                // CREAR TRIGRAMAS: buscar contexto siguiente para formar trigrama completo
                if (i < contextosArchivo.Count - 1)
                {
                    var siguienteContexto = contextosArchivo[i + 1];

                    // Verificar que los contextos sean consecutivos (palabra actual = palabra anterior del siguiente)
                    if (contexto.PalabraActual == siguienteContexto.PalabraAnterior)
                    {
                        // Extraer las tres palabras del trigrama
                        string palabra1 = LimpiarPalabra(contexto.PalabraAnterior ?? "");
                        string palabra2 = LimpiarPalabra(contexto.PalabraActual);
                        string palabra3 = LimpiarPalabra(siguienteContexto.PalabraActual);

                        // Verificar que las tres palabras sean válidas
                        if (EsPalabraValida(palabra1) && EsPalabraValida(palabra2) && EsPalabraValida(palabra3))
                        {
                            // Crear clave del trigrama combinando las dos primeras palabras
                            string claveTrigrama = $"{palabra1} {palabra2}";

                            // Crear entrada en el diccionario si no existe
                            if (!trigramas.ContainsKey(claveTrigrama))
                                trigramas[claveTrigrama] = new Dictionary<string, HashSet<string>>();

                            // Crear entrada para la tercera palabra si no existe
                            if (!trigramas[claveTrigrama].ContainsKey(palabra3))
                                trigramas[claveTrigrama][palabra3] = new HashSet<string>();

                            // Agregar el archivo origen a la lista de fuentes
                            trigramas[claveTrigrama][palabra3].Add(contexto.ArchivoOrigen);
                            trigramasAgregados++;
                        }
                    }
                }
            }
        }

        // Mostrar resumen del entrenamiento completado
        Console.WriteLine($"[ModeloPrediccion] Entrenamiento completado:");
        Console.WriteLine($"  - {bigramas.Count} bigramas únicos ({bigramasAgregados} total)");
        Console.WriteLine($"  - {trigramas.Count} trigramas únicos ({trigramasAgregados} total)");
        Console.WriteLine($"  - {TotalContextos} contextos procesados");

        // Mostrar algunos ejemplos de lo que el modelo aprendió
        MostrarEjemplosAprendidos();
    }

    private void MostrarEjemplosAprendidos()
    {
        Console.WriteLine("[ModeloPrediccion] Ejemplos de lo que aprendió:");

        // Mostrar algunos bigramas como ejemplo
        var ejemplosBigramas = bigramas.Take(5);
        foreach (var bigrama in ejemplosBigramas)
        {
            // Tomar solo las primeras 3 palabras siguientes para no saturar la salida
            var siguientes = string.Join(", ", bigrama.Value.Keys.Take(3));
            Console.WriteLine($"  '{bigrama.Key}' -> [{siguientes}]");
        }

        // Mostrar algunos trigramas como ejemplo
        var ejemplosTrigramas = trigramas.Take(3);
        foreach (var trigrama in ejemplosTrigramas)
        {
            // Tomar solo las primeras 2 palabras siguientes para no saturar la salida
            var siguientes = string.Join(", ", trigrama.Value.Keys.Take(2));
            Console.WriteLine($"  '{trigrama.Key}' -> [{siguientes}]");
        }
    }

    /// <summary>
    /// Predice la siguiente palabra basada en el contexto proporcionado
    /// </summary>
    public List<(string Palabra, List<string> Archivos)> PredecirSiguientePalabra(string contexto, int topK = 5)
    {
        // Verificar que el contexto no esté vacío
        if (string.IsNullOrWhiteSpace(contexto))
        {
            Console.WriteLine("[ModeloPrediccion] Contexto vacío");
            return new List<(string, List<string>)>();
        }

        // Procesar el contexto: dividir en palabras, limpiar y validar
        contexto = contexto.Trim();
        var palabrasContexto = contexto.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(LimpiarPalabra)
            .Where(EsPalabraValida)
            .ToArray();

        Console.WriteLine($"[ModeloPrediccion] Prediciendo para contexto: '{contexto}'");
        Console.WriteLine($"[ModeloPrediccion] Palabras procesadas: [{string.Join(", ", palabrasContexto)}]");

        // Variables para almacenar los candidatos encontrados
        Dictionary<string, HashSet<string>> candidatos = null;
        string metodoUsado = "";

        // PRIORIDAD 1: Buscar TRIGRAMAS (más específico y preciso)
        if (palabrasContexto.Length >= 2)
        {
            // Tomar las últimas dos palabras para formar la clave del trigrama
            string claveTrigrama = $"{palabrasContexto[^2]} {palabrasContexto[^1]}";
            Console.WriteLine($"[ModeloPrediccion] Buscando trigrama: '{claveTrigrama}'");

            if (trigramas.ContainsKey(claveTrigrama))
            {
                candidatos = trigramas[claveTrigrama];
                metodoUsado = "trigrama";
                Console.WriteLine($"[ModeloPrediccion] ✓ Encontrado trigrama con {candidatos.Count} candidatos");
            }
            else
            {
                Console.WriteLine("[ModeloPrediccion] ✗ No se encontró el trigrama");
            }
        }

        // PRIORIDAD 2: Buscar BIGRAMAS (menos específico pero más amplio)
        if (candidatos == null && palabrasContexto.Length >= 1)
        {
            // Tomar la última palabra del contexto
            string ultimaPalabra = palabrasContexto[^1];
            Console.WriteLine($"[ModeloPrediccion] Buscando bigrama: '{ultimaPalabra}' -> ?");

            if (bigramas.ContainsKey(ultimaPalabra))
            {
                candidatos = bigramas[ultimaPalabra];
                metodoUsado = "bigrama";
                Console.WriteLine($"[ModeloPrediccion] ✓ Encontrado bigrama con {candidatos.Count} candidatos");
            }
            else
            {
                Console.WriteLine("[ModeloPrediccion] ✗ No se encontró el bigrama");

                // FALLBACK: Buscar palabras que contengan la última palabra como prefijo
                var candidatosPorPrefijo = BuscarCandidatosPorPrefijo(ultimaPalabra);
                if (candidatosPorPrefijo.Any())
                {
                    Console.WriteLine($"[ModeloPrediccion] ✓ Encontrados {candidatosPorPrefijo.Count} candidatos por prefijo");
                    return candidatosPorPrefijo.Take(topK).ToList();
                }
            }
        }

        // Si no se encontraron candidatos, retornar lista vacía
        if (candidatos == null || candidatos.Count == 0)
        {
            Console.WriteLine("[ModeloPrediccion] No se encontraron candidatos");
            return new List<(string, List<string>)>();
        }

        // Ordenar candidatos por relevancia: más archivos = más relevante
        var resultado = candidatos
            .OrderByDescending(p => p.Value.Count)           // Prioridad por número de fuentes
            .ThenBy(p => p.Key)                              // Orden alfabético como desempate
            .Take(topK)                                      // Tomar solo los mejores K candidatos
            .Select(p => (p.Key, p.Value.ToList()))
            .ToList();

        // Mostrar el resultado de la predicción
        Console.WriteLine($"[ModeloPrediccion] Devolviendo {resultado.Count} predicciones usando {metodoUsado}:");
        foreach (var (palabra, archivos) in resultado)
        {
            // Mostrar algunos archivos fuente (máximo 3) para no saturar la salida
            Console.WriteLine($"  - '{palabra}' ({archivos.Count} fuentes: {string.Join(", ", archivos.Take(3))}{(archivos.Count > 3 ? "..." : "")})");
        }

        return resultado;
    }

    private List<(string Palabra, List<string> Archivos)> BuscarCandidatosPorPrefijo(string prefijo)
    {
        var candidatos = new Dictionary<string, HashSet<string>>();

        // Buscar en todos los bigramas palabras que empiecen con el prefijo
        foreach (var bigrama in bigramas)
        {
            foreach (var siguiente in bigrama.Value)
            {
                // Verificar si la palabra siguiente empieza con el prefijo (ignorando mayúsculas)
                if (siguiente.Key.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
                {
                    // Agregar la palabra a los candidatos si no existe
                    if (!candidatos.ContainsKey(siguiente.Key))
                        candidatos[siguiente.Key] = new HashSet<string>();

                    // Agregar todos los archivos donde aparece esta palabra
                    foreach (var archivo in siguiente.Value)
                        candidatos[siguiente.Key].Add(archivo);
                }
            }
        }

        // Ordenar por relevancia (número de archivos) y convertir a la estructura esperada
        return candidatos
            .OrderByDescending(c => c.Value.Count)
            .Select(c => (c.Key, c.Value.ToList()))
            .ToList();
    }

    /// <summary>
    /// Limpia y normaliza una palabra para el procesamiento
    /// </summary>
    private string LimpiarPalabra(string palabra)
    {
        if (string.IsNullOrWhiteSpace(palabra))
            return "";

        // Primero paso todo a minúsculas
        palabra = palabra.ToLower().Trim();

        // Quito acentos
        palabra = QuitarAcentos(palabra);

        // Quito signos de puntuación
        palabra = palabra
            .Replace(".", "")
            .Replace(",", "")
            .Replace(";", "")
            .Replace(":", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("\"", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Replace("«", "")
            .Replace("»", "")
            .Replace("'", "")
            .Replace("—", "")
            .Replace("–", "")
            .Replace("\t", "")
            .Replace("\n", "")
            .Replace("\r", "");

        return palabra;
    }

    private string QuitarAcentos(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;
            
        var normalized = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (var c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

/// <summary>
/// Verifica si una palabra es válida para incluir en el modelo
/// </summary>
private bool EsPalabraValida(string palabra)
    {
        // Filtros básicos de validación
        if (string.IsNullOrWhiteSpace(palabra)) return false;
        if (palabra.Length < 2) return false; // Descartar palabras muy cortas
        if (palabra.Length > 50) return false; // Descartar palabras muy largas (probablemente errores)

        // Descartar si es solo números (fechas, IDs, etc.)
        if (palabra.All(char.IsDigit)) return false;

        // Debe tener al menos una letra para ser considerada palabra válida
        if (!palabra.Any(char.IsLetter)) return false;

        return true;
    }

    /// <summary>
    /// Obtiene estadísticas detalladas del modelo entrenado
    /// </summary>
    public object ObtenerEstadisticas()
    {
        // Calcular estadísticas del vocabulario
        var palabrasUnicasBigramas = bigramas.Keys.Count + bigramas.Values.SelectMany(d => d.Keys).Distinct().Count();
        var palabrasUnicasTrigramas = trigramas.Values.SelectMany(d => d.Keys).Distinct().Count();

        return new
        {
            TotalBigramas = bigramas.Count,
            TotalTrigramas = trigramas.Count,
            TotalContextos = TotalContextos,
            PalabrasUnicasBigramas = palabrasUnicasBigramas,
            PalabrasUnicasTrigramas = palabrasUnicasTrigramas,
            BigramaConMasPalabras = bigramas.OrderByDescending(b => b.Value.Count).FirstOrDefault(),
            TrigramaConMasPalabras = trigramas.OrderByDescending(t => t.Value.Count).FirstOrDefault(),
            EjemplosBigramas = bigramas.Take(5).ToDictionary(b => b.Key, b => b.Value.Keys.Take(3).ToList()),
            EjemplosTrigramas = trigramas.Take(3).ToDictionary(t => t.Key, t => t.Value.Keys.Take(3).ToList())
        };
    }

    /// <summary>
    /// Busca palabras que comiencen con el prefijo dado
    /// </summary>
    public List<(string Palabra, List<string> Archivos, int Relevancia)> BuscarPorPrefijo(string prefijo, int limite = 10)
    {
        if (string.IsNullOrWhiteSpace(prefijo))
            return new List<(string, List<string>, int)>();

        // Limpiar el prefijo usando la misma lógica que las palabras del modelo
        var prefijoLimpio = LimpiarPalabra(prefijo);
        var resultados = new Dictionary<string, HashSet<string>>();

        // Buscar coincidencias en bigramas
        foreach (var bigrama in bigramas)
        {
            foreach (var siguiente in bigrama.Value)
            {
                if (siguiente.Key.StartsWith(prefijoLimpio))
                {
                    // Agregar palabra si no existe en resultados
                    if (!resultados.ContainsKey(siguiente.Key))
                        resultados[siguiente.Key] = new HashSet<string>();

                    // Agregar todos los archivos donde aparece
                    foreach (var archivo in siguiente.Value)
                        resultados[siguiente.Key].Add(archivo);
                }
            }
        }

        // Buscar coincidencias en trigramas
        foreach (var trigrama in trigramas)
        {
            foreach (var siguiente in trigrama.Value)
            {
                if (siguiente.Key.StartsWith(prefijoLimpio))
                {
                    // Agregar palabra si no existe en resultados
                    if (!resultados.ContainsKey(siguiente.Key))
                        resultados[siguiente.Key] = new HashSet<string>();

                    // Agregar todos los archivos donde aparece
                    foreach (var archivo in siguiente.Value)
                        resultados[siguiente.Key].Add(archivo);
                }
            }
        }

        // Ordenar por relevancia (número de archivos) y limitar resultados
        return resultados
            .OrderByDescending(r => r.Value.Count)
            .Take(limite)
            .Select(r => (r.Key, r.Value.ToList(), r.Value.Count))
            .ToList();
    }

    /// <summary>
    /// Reinicia completamente el modelo eliminando todos los datos
    /// </summary>
    public void LimpiarModelo()
    {
        bigramas.Clear();
        trigramas.Clear();
        TotalContextos = 0;
        Console.WriteLine("[ModeloPrediccion] Modelo limpiado");
    }
}