using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;

namespace BuscadorParaleloEspeculativo.UI.Models
{
    // Datos de dónde viene cada palabra procesada
    public class OrigenPalabra
    {
        public string ArchivoOrigen { get; set; }
        public int Frecuencia { get; set; }
        public DateTime FechaProcesamiento { get; set; }
        public string UbicacionEnTexto { get; set; }
    }

    // Resultados de procesamiento (secuencial o paralelo)
    public class ResultadoProcesamiento
    {
        public string Metodo { get; set; }
        public long TiempoMs { get; set; }
        public double TiempoSeg => TiempoMs / 1000.0;
        public int ArchivosProcesados { get; set; }
        public int PalabrasUnicas { get; set; }
        public int PalabrasTotal { get; set; }
        public double PalabrasPorSegundo => TiempoSeg > 0 ? PalabrasTotal / TiempoSeg : 0;
        public Dictionary<string, List<OrigenPalabra>> PalabrasConOrigen { get; set; }
        public DateTime FechaEjecucion { get; set; }
    }

    // Estado individual de procesamiento de cada archivo
    public class EstadoArchivo
    {
        public string NombreArchivo { get; set; }
        public string RutaCompleta { get; set; }
        public string Estado { get; set; }
        public int PalabrasProcesadas { get; set; }
        public long TamañoBytes { get; set; }
        public string TamañoLegible => FormatearTamaño(TamañoBytes);

        // Convierte bytes a formato legible (KB, MB)
        private string FormatearTamaño(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F1} MB";
        }
    }

    // PARA ANGEL: Contexto de palabras para modelo predictivo
    public class ContextoPalabra
    {
        public string PalabraActual { get; set; }
        public string PalabraAnterior { get; set; }
        public string ArchivoOrigen { get; set; }
        public int Posicion { get; set; }
    }

    // CLASE PRINCIPAL - CANDY: Procesamiento paralelo de archivos de texto
    public class ProcesadorArchivos
    {
        // Estructuras thread-safe para procesamiento paralelo
        private readonly ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>> _palabrasConOrigen;
        private readonly ConcurrentBag<ContextoPalabra> _contextoPalabras;
        private readonly List<EstadoArchivo> _estadoArchivos;
        private readonly object _lockEstados = new object();
        private readonly ConcurrentDictionary<string, int> _frecuenciaPalabras;

        // Lista de palabras que no aportan valor semántico (stop words en español)
        private static readonly HashSet<string> PalabrasVacias = new HashSet<string>
        {
            "que", "para", "con", "por", "como", "más", "pero", "sus", "les", "una", "del",
            "los", "las", "son", "fue", "han", "muy", "hay", "sin", "ser", "está", "todo",
            "también", "donde", "cuando", "sobre", "desde", "hasta", "entre", "mientras",
            "este", "esta", "estos", "estas", "otro", "otros", "otra", "otras", "mismo", "misma",
            "será", "pueden", "solo", "cada", "tiene", "hacer", "después", "forma", "bien",
            "aquí", "tanto", "estado", "durante", "siempre", "ejemplo", "tiempo", "casos"
        };

        // Constructor: inicializa todas las estructuras de datos
        public ProcesadorArchivos()
        {
            _palabrasConOrigen = new ConcurrentDictionary<string, ConcurrentBag<OrigenPalabra>>();
            _contextoPalabras = new ConcurrentBag<ContextoPalabra>();
            _estadoArchivos = new List<EstadoArchivo>();
            _frecuenciaPalabras = new ConcurrentDictionary<string, int>();
        }

        // Extrae texto de diferentes tipos de archivo (TXT, DOCX, PDF)
        private async Task<(Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos)> ExtraerPalabrasDelArchivoAsync(string rutaArchivo)
        {
            var extension = Path.GetExtension(rutaArchivo).ToLower();
            string contenido = "";

            try
            {
                // Procesar según el tipo de archivo
                switch (extension)
                {
                    case ".txt":
                        contenido = await File.ReadAllTextAsync(rutaArchivo, Encoding.UTF8);
                        break;
                    case ".docx":
                        contenido = await ExtraerTextoDeDocxAsync(rutaArchivo);
                        break;
                    case ".pdf":
                        contenido = await ExtraerTextoDePdfAsync(rutaArchivo);
                        break;
                    default:
                        throw new NotSupportedException($"Formato no soportado: {extension}");
                }

                // Tokenizar y crear contextos
                var resultado = TokenizarYContarPalabrasConContexto(contenido, Path.GetFileName(rutaArchivo));
                return (resultado.Palabras, resultado.Contextos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error procesando {Path.GetFileName(rutaArchivo)}: {ex.Message}");
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());
            }
        }

        // Extrae texto de archivos Word (.docx)
        private async Task<string> ExtraerTextoDeDocxAsync(string rutaArchivo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var doc = WordprocessingDocument.Open(rutaArchivo, false);
                    var body = doc.MainDocumentPart?.Document?.Body;
                    return body?.InnerText ?? string.Empty;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en DOCX: {ex.Message}");
                    return string.Empty;
                }
            });
        }

        // Extrae texto de archivos PDF
        private async Task<string> ExtraerTextoDePdfAsync(string rutaArchivo)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var pdfReader = new PdfReader(rutaArchivo);
                    using var pdfDocument = new PdfDocument(pdfReader);
                    var contenido = new StringBuilder();

                    // Procesar cada página del PDF
                    for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                    {
                        var pagina = pdfDocument.GetPage(i);
                        var textoPagina = PdfTextExtractor.GetTextFromPage(pagina);
                        contenido.AppendLine(textoPagina);
                    }

                    return contenido.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error en PDF: {ex.Message}");
                    return string.Empty;
                }
            });
        }

        // Convierte texto en palabras limpias y crea contextos para predicción
        private (Dictionary<string, int> Palabras, List<ContextoPalabra> Contextos) TokenizarYContarPalabrasConContexto(string texto, string nombreArchivo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return (new Dictionary<string, int>(), new List<ContextoPalabra>());

            // Limpiar texto: solo letras españolas y espacios
            var textoLimpio = Regex.Replace(texto.ToLower(), @"[^a-záéíóúüñ\s]", " ");

            // Dividir en palabras válidas (mínimo 3 caracteres, no stop words)
            var palabras = textoLimpio
                .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p.Length >= 3 && !EsPalabraVacia(p))
                .ToArray();

            // Contar frecuencias de cada palabra
            var conteos = new Dictionary<string, int>();
            foreach (var palabra in palabras)
            {
                conteos[palabra] = conteos.ContainsKey(palabra) ? conteos[palabra] + 1 : 1;
            }

            // Crear contextos palabra-anterior → palabra-actual para ANGEL
            var contextos = new List<ContextoPalabra>();
            for (int i = 1; i < palabras.Length; i++)
            {
                contextos.Add(new ContextoPalabra
                {
                    PalabraAnterior = palabras[i - 1],
                    PalabraActual = palabras[i],
                    ArchivoOrigen = nombreArchivo,
                    Posicion = i
                });
            }

            return (conteos, contextos);
        }

        // Verifica si una palabra está en la lista de stop words
        private bool EsPalabraVacia(string palabra)
        {
            return PalabrasVacias.Contains(palabra);
        }

        // Verifica si el archivo tiene extensión válida
        private bool EsArchivoValido(string nombreArchivo)
        {
            var extensionesValidas = new[] { ".txt", ".pdf", ".docx" };
            return extensionesValidas.Contains(Path.GetExtension(nombreArchivo).ToLower());
        }

        // Limpia nombres de archivo de caracteres peligrosos
        private string LimpiarNombreArchivo(string nombreArchivo)
        {
            var caracteresPeligrosos = new[] { "<", ">", ":", "\"", "|", "?", "*" };
            var nombreLimpio = nombreArchivo;

            foreach (var caracter in caracteresPeligrosos)
            {
                nombreLimpio = nombreLimpio.Replace(caracter, "_");
            }

            return nombreLimpio;
        }

        // Limpia todas las estructuras de datos para nueva ejecución
        public void LimpiarDatos()
        {
            _palabrasConOrigen.Clear();
            _contextoPalabras.Clear();
            _frecuenciaPalabras.Clear();
            lock (_lockEstados)
            {
                _estadoArchivos.Clear();
            }
        }
    }
}