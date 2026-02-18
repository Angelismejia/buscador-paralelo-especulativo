# buscador-paralelo-especulativo

Curso: Programación Paralela
Equipo: CANDY ANGELIS (Líder del grupo), ANGEL, JASON, ISMER, INDI
Entrega: 20 de Agosto, 2025

# Descripcion 

El Sistema de Predicción de Texto Especulativo es una aplicación web en C# que procesa archivos TXT, DOCX y PDF usando paralelismo para generar sugerencias de autocompletado basadas en patrones lingüísticos (bigramas y trigramas).

Desarrollado como trabajo en equipo, combina procesamiento paralelo, predicción de texto y una interfaz web intuitiva, garantizando eficiencia y escalabilidad. Aplicable en:

Escritura corporativa y colaboración en documentos.

Análisis académico de grandes bibliotecas de texto.

Gestión documental y búsqueda inteligente.

# Tecnologías 

C# (.NET 6), ASP.NET Core

iText7 (PDF), OpenXML (DOCX)

Task Parallel Library & ConcurrentCollections

HTML5, CSS3, JavaScript

Git, GitHub, Microsoft Teams

# Estructura
buscador-paralelo-especulativo/
├── docs/       # Documentación
├── metrics/    # Métricas de rendimiento
├── src/
│   └── BuscadorParaleloEspeculativo.UI/
│       ├── wwwroot/css, js
│       ├── Pages/Index.cshtml
│       ├── Models/ProcesadorArchivos.cs, ModeloPrediccion.cs
│       └── Controllers/
├── tests/
├── .gitignore
└── README.md

# Ejecución

Clonar el repositorio:

git clone https://github.com/Angelismejia/buscador-paralelo-especulativo.git
cd buscador-paralelo-especulativo/src/BuscadorParaleloEspeculativo.UI

Restaurar dependencias y ejecutar:

dotnet restore
dotnet run

Abrir navegador en https://localhost:7XXX y probar con archivos TXT, DOCX o PDF.
