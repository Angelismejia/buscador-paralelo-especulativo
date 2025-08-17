using System;
using System.Collections.Generic;
using System.Linq;
using BuscadorParaleloEspeculativo.UI.Models;

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

        // Contadore