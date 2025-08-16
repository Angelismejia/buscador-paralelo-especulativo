using System;
using System.Collections.Generic;
using System.Linq;
using BuscadorParaleloEspeculativo.UI.Models; // para tener acceso a los modelos

public class ModeloPrediccion
{
    // Bigramas y trigramas con lista de archivos
    private Dictionary<string, Dictionary<string, HashSet<string>>> bigramas;
    private Dictionary<string, Dictionary<string, HashSet<string>>> trigramas;

    public ModeloPrediccion()
    {
        bigramas = new Dictionary<string, Dictionary<string, HashSet<string>>>();
        trigramas = new Dictionary<string, Dictionary<string, HashSet<string>>>();
    }

    /// aqui entreno el modelo segun el contexto de Procesador Archivos
    public void EntrenarModelo(List<ContextoPalabra> contextos)
    {
        for (int i = 0; i < contextos.Count; i++)
        {
            var contexto = contextos[i];

            // BIGRAMAS
            if (!bigramas.ContainsKey(contexto.PalabraAnterior))
                bigramas[contexto.PalabraAnterior] = new Dictionary<string, HashSet<string>>();

            if (!bigramas[contexto.PalabraAnterior].ContainsKey(contexto.PalabraActual))
                bigramas[contexto.PalabraAnterior][contexto.PalabraActual] = new HashSet<string>();

            bigramas[contexto.PalabraAnterior][contexto.PalabraActual].Add(contexto.ArchivoOrigen);

            // TRIGRAMAS si hay al menos 2 contextos 
            if (i > 0)
            {
                var contextoPrevio = contextos[i - 1];
                string claveTrigrama = contextoPrevio.PalabraAnterior + " " + contextoPrevio.PalabraActual;

                if (!trigramas.ContainsKey(claveTrigrama))
                    trigramas[claveTrigrama] = new Dictionary<string, HashSet<string>>();

                if (!trigramas[claveTrigrama].ContainsKey(contexto.PalabraActual))
                    trigramas[claveTrigrama][contexto.PalabraActual] = new HashSet<string>();

                trigramas[claveTrigrama][contexto.PalabraActual].Add(contexto.ArchivoOrigen);
            }
        }
    }

    /// aqui se predice la siguiente palabra, devolviendo también los archivos de origen.
    public List<(string Palabra, List<string> Archivos)> PredecirSiguientePalabra(string contexto, int topK = 5)
    {
        contexto = contexto.Trim().ToLower();
        var palabrasContexto = contexto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, HashSet<string>> candidatos = null;

        // Primero busca trigramas
        if (palabrasContexto.Length >= 2)
        {
            string claveTrigrama = palabrasContexto[^2] + " " + palabrasContexto[^1];
            if (trigramas.ContainsKey(claveTrigrama))
                candidatos = trigramas[claveTrigrama];
        }

        // Si no hay trigramas, buscar bigramas
        if (candidatos == null && palabrasContexto.Length >= 1)
        {
            string ultimaPalabra = palabrasContexto.Last();
            if (bigramas.ContainsKey(ultimaPalabra))
                candidatos = bigramas[ultimaPalabra];
        }

        if (candidatos == null)
            return new List<(string, List<string>)>();

        // Ordenar por frecuenciamayor cantidad de archivos → más relevancia
        return candidatos
            .OrderByDescending(p => p.Value.Count)
            .Take(topK)
            .Select(p => (p.Key, p.Value.ToList()))
            .ToList();
    }
}
