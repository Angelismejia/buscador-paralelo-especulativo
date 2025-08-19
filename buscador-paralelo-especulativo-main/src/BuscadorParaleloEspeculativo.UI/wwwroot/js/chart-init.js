const chartConfig = {
    type: 'line',
    data: {
        labels: [],
        datasets: [{
            label: 'Rendimiento',
            data: [],
            borderColor: '#007bff',
            backgroundColor: 'rgba(0, 123, 255, 0.1)',
            tension: 0.1
        }]
    },
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'top'
            },
            title: {
                display: true,
                text: 'Análisis de Rendimiento'
            }
        },
        scales: {
            y: {
                beginAtZero: true
            }
        }
    }
};

function initializeChart() {
    const canvas = document.getElementById('performanceChart');
    if (canvas) {
        const ctx = canvas.getContext('2d');
        if (ctx) {
            // Inicializar Chart.js aquí
            new Chart(ctx, chartConfig);
        }
    }
}

// Llamar cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', initializeChart);
