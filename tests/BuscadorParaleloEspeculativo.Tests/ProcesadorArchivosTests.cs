using NUnit.Framework;
using BuscadorParaleloEspeculativo.UI; // 👈 cambia esto por el namespace real de tu proyecto principal
using System.IO;

namespace PruebasProyecto
{
    [TestFixture]
    public class ProcesadorArchivosTests
    {
        private string _rutaPrueba;

        [SetUp]
        public void Setup()
        {
            // Crear carpeta temporal para las pruebas
            _rutaPrueba = Path.Combine(Path.GetTempPath(), "CarpetaPruebas");
            if (!Directory.Exists(_rutaPrueba))
                Directory.CreateDirectory(_rutaPrueba);
        }

        [TearDown]
        public void Cleanup()
        {
            // Borrar carpeta temporal después de cada prueba
            if (Directory.Exists(_rutaPrueba))
                Directory.Delete(_rutaPrueba, true);
        }

        [Test]
        public void ProcesarArchivos_DeberiaCrearArchivoProcesado()
        {
            // Arrange
            string archivo = Path.Combine(_rutaPrueba, "archivo.txt");
            File.WriteAllText(archivo, "Contenido de prueba");

            var procesador = new ProcesadorArchivos();

            // Act
            procesador.Procesar(archivo);

            // Assert
            string archivoProcesado = Path.Combine(_rutaPrueba, "archivo.procesado.txt");
            Assert.That(File.Exists(archivoProcesado), Is.True, "El archivo procesado no fue creado");
        }

        [Test]
        public void ProcesarArchivos_ArchivoNoExiste_DeberiaLanzarExcepcion()
        {
            // Arrange
            string archivoInexistente = Path.Combine(_rutaPrueba, "noexiste.txt");
            var procesador = new ProcesadorArchivos();

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() =>
            {
                procesador.Procesar(archivoInexistente);
            });
        }
    }
}
