Documento Principal (5ptos)

<img width="1199" height="580" alt="Captura de pantalla 2025-08-19 110711" src="https://github.com/user-attachments/assets/10dc2be1-ee89-46f8-aa5a-9ec6d1eb049c" />


Portada
•	Nombre del curso
•	Título del proyecto
•	Integrantes del equipo
•	Nombre del líder
•	Fecha de entrega

Índice
1. Introducción
•	Presentación general del proyecto
•	Justificación del tema elegido
•	Objetivos (general y específicos)

2. Descripción del Problema
•	Contexto del problema seleccionado
•	Aplicación del problema en un escenario real
•	Importancia del paralelismo en la solución

3. Cumplimiento de los Requisitos del Proyecto
Para cada criterio, se debe justificar cómo el proyecto lo aborda:
1.	Ejecución simultánea de múltiples tareas
2.	Necesidad de compartir datos entre tareas
3.	Exploración de diferentes estrategias de paralelización
4.	Escalabilidad con más recursos
5.	Métricas de evaluación del rendimiento
6.	Aplicación a un problema del mundo real

   
4. Diseño de la Solución
   
•	Arquitectura general del sistema
•	Diagrama de componentes/tareas paralelas
•	Estrategia de paralelización utilizada
•	Herramientas y tecnologías empleadas (C#, TPL, etc.)

5. Implementación Técnica
•	Descripción de la estructura del proyecto
•	Explicación del código clave
•	Uso de mecanismos de sincronización
•	Justificación técnica de las decisiones tomadas

6. Evaluación de Desempeño
•	Comparativa entre ejecución secuencial y paralela
•	Métricas: tiempo de ejecución, eficiencia, escalabilidad
•	Gráficas o tablas con resultados
•	Análisis de cuellos de botella o limitaciones

7. Trabajo en Equipo
•	Descripción del reparto de tareas
•	Herramientas utilizadas para coordinación (Git,

8. Conclusiones
•	Principales aprendizajes técnicos
•	Retos enfrentados y superados
•	Posibles mejoras o líneas futuras

9. Referencias
•	Fuentes bibliográficas, técnicas o académicas consultadas

10. Anexos
•	Manual de ejecución del sistema
•	Capturas adicionales, pruebas complementarias
•	Enlace al repositorio de Git (público)

-----------------------------------------------------

Interfaz Principal
La aplicación presenta un diseño moderno con cuatro módulos principales distribuidos en una interfaz intuitiva.

 Procesamiento de Archivos (Módulo Superior Izquierdo)
 Función: Cargar y procesar archivos de texto para análisis
  
Características:
Zona de Arrastre: Área central con ícono de carpeta donde puedes arrastrar archivos
            
Texto Guía: "Arrastra archivos aquí o haz clic para seleccionar"


Formatos Soportados: .txt, .docx, .pdf (indicados por los botones en la parte inferior)
Instrucciones de uso:
1.	Arrastra archivos directamente a la zona punteada
2.	O haz clic en el área para abrir el selector de archivos
3.	Selecciona el formato apropiado (.txt, .docx, .pdf)
4.	Presiona "PROCESAR ARCHIVOS" para iniciar el análisis
5.	Usa "LIMPIAR" para resetear la selección



Paso 1: Preparar Datos
1.	Accede al módulo Procesamiento de Archivos
2.	Carga tus documentos de texto
3.	Selecciona el formato correcto
4.	Procesa los archivos




 
Paso 2: Monitorear Rendimiento
1.	Observa las Métricas de Rendimiento
2.	Verifica que el procesamiento paralelo esté activo (indicado en verde)
3.	Compara tiempos secuenciales vs. paralelos

	



 Paso 3: Realizar Predicciones
1.	En Predicción de Texto, introduce tu texto base
2.	Espera a que el sistema procese la información
3.	Revisa las predicciones generadas


<img width="1199" height="580" alt="image" src="https://github.com/user-attachments/assets/74c9044e-1687-4864-839d-02aeccc24337" />




