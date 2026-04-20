// Configura la apariencia y tipo de gráfico (línea).
const chartConfig = {
    type: 'line',
    data: {
        // Los datos y etiquetas irán aquí.
        labels: [],
        datasets: [{
            label: 'Rendimiento',
            data: [],
            // Un poco de estilo para que se vea bien.
            borderColor: '#007bff',
            backgroundColor: 'rgba(0, 123, 255, 0.1)',
            tension: 0.1
        }]
    },
    options: {
        // Opciones visuales y de comportamiento.
        responsive: true,
        plugins: {
            legend: {
                position: 'top' // Leyenda arriba.
            },
            title: {
                display: true,
                text: 'Análisis de Rendimiento' // Título del gráfico.
            }
        },
        scales: {
            y: {
                beginAtZero: true // El eje Y siempre empieza en 0.
            }
        }
    }
};

// Función para crear y dibujar el gráfico.
function initializeChart() {
    // Encuentra el lienzo donde se va a dibujar.
    const canvas = document.getElementById('performanceChart');
    if (canvas) {
        const ctx = canvas.getContext('2d');
        if (ctx) {
            // ¡Dibuja el gráfico usando Chart.js!
            new Chart(ctx, chartConfig);
        }
    }
}

// Asegura que el gráfico se cree solo cuando la página esté lista.
document.addEventListener('DOMContentLoaded', initializeChart);