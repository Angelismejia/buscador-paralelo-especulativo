// ==========================================================================
// SISTEMA DE PREDICCIÓN DE TEXTO CON BACKEND INTEGRADO
// Equipo de desarrollo: CANDY • ANGEL • JASON • ISMER • INDI
// ==========================================================================

class TextPredictionSystem {
    constructor() {
        // Inicialización de variables de estado del sistema
        this.files = [];                    // Lista de archivos cargados
        this.processedData = null;          // Datos procesados del backend
        this.isProcessing = false;          // Estado del procesamiento
        this.currentPredictions = [];       // Predicciones actuales
        this.isModelReady = false;          // Estado del modelo de predicción
        this.selectedSuggestionIndex = -1;  // Índice de sugerencia seleccionada
        this.debounceTimer = null;          // Timer para evitar llamadas excesivas al API

        // Configuración inicial del sistema
        this.initializeElements();
        this.bindEvents();
        this.startAnimations();
        this.checkModelStatus();
    }

    // Obtiene referencias a todos los elementos del DOM necesarios
    initializeElements() {
        // Elementos de carga de archivos
        this.uploadZone = document.getElementById('uploadZone');
        this.fileInput = document.getElementById('fileInput');
        this.fileList = document.getElementById('fileList');
        this.processBtn = document.getElementById('processBtn');
        this.clearBtn = document.getElementById('clearBtn');

        // Elementos de la barra de progreso
        this.progressSection = document.getElementById('progressSection');
        this.progressFill = document.getElementById('progressFill');
        this.progressText = document.getElementById('progressText');

        // Elementos de métricas de rendimiento
        this.sequentialTime = document.getElementById('sequentialTime');
        this.parallelTime = document.getElementById('parallelTime');
        this.speedup = document.getElementById('speedup');
        this.efficiency = document.getElementById('efficiency');
        this.wordsPerSec = document.getElementById('wordsPerSec');
        this.filesProcessed = document.getElementById('filesProcessed');
        this.uniqueWords = document.getElementById('uniqueWords');
        this.totalWords = document.getElementById('totalWords');

        // Elementos del sistema de predicción de texto
        this.textInput = document.getElementById('textInput');
        this.suggestionsContainer = document.getElementById('suggestionsContainer');
        this.suggestionsGrid = document.getElementById('suggestionsGrid');
        this.sourcesList = document.getElementById('sourcesList');

        console.log('[Sistema] Elementos inicializados correctamente');
    }

    // Vincula todos los eventos necesarios para la funcionalidad
    bindEvents() {
        // Eventos para carga de archivos
        if (this.uploadZone) {
            this.uploadZone.addEventListener('click', () => this.fileInput?.click());
        }

        if (this.fileInput) {
            this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        }

        // Eventos para arrastrar y soltar archivos
        if (this.uploadZone) {
            this.uploadZone.addEventListener('dragover', (e) => this.handleDragOver(e));
            this.uploadZone.addEventListener('dragleave', (e) => this.handleDragLeave(e));
            this.uploadZone.addEventListener('drop', (e) => this.handleFileDrop(e));
        }

        // Eventos de botones principales
        if (this.processBtn) {
            this.processBtn.addEventListener('click', () => this.processFiles());
        }

        if (this.clearBtn) {
            this.clearBtn.addEventListener('click', () => this.clearAll());
        }

        // Eventos del campo de texto para predicción
        if (this.textInput) {
            this.textInput.addEventListener('input', (e) => this.handleTextInput(e));
            this.textInput.addEventListener('keydown', (e) => this.handleKeyDown(e));
            this.textInput.addEventListener('focus', () => this.showSuggestions());
            // Delay antes de ocultar sugerencias para permitir clics
            this.textInput.addEventListener('blur', () => {
                setTimeout(() => this.hideSuggestions(), 200);
            });
        }

        this.bindTitleAnimations();
    }

    // Añade efectos visuales al título principal
    bindTitleAnimations() {
        const titleParts = document.querySelectorAll('.title-part');
        titleParts.forEach((part, index) => {
            part.addEventListener('mouseenter', () => {
                // Efecto de escala y rotación suave al pasar el mouse
                part.style.transform = `scale(1.1) rotate(${Math.random() * 10 - 5}deg)`;
                part.style.transition = 'all 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55)';
            });

            part.addEventListener('mouseleave', () => {
                part.style.transform = 'scale(1) rotate(0deg)';
            });
        });
    }

    // Inicia las animaciones y efectos visuales
    startAnimations() {
        this.createFloatingParticles();
        this.initializePredictionArea();
    }

    // Crea partículas flotantes para el fondo
    createFloatingParticles() {
        const container = document.querySelector('.floating-particles');
        if (!container) return;

        // Genera 20 partículas con propiedades aleatorias
        for (let i = 0; i < 20; i++) {
            const particle = document.createElement('div');
            particle.className = 'particle';
            particle.style.cssText = `
                position: absolute;
                width: ${Math.random() * 4 + 2}px;
                height: ${Math.random() * 4 + 2}px;
                background: rgba(0, 255, 136, ${Math.random() * 0.5 + 0.2});
                border-radius: 50%;
                left: ${Math.random() * 100}%;
                top: ${Math.random() * 100}%;
                animation: float ${Math.random() * 10 + 10}s infinite linear;
            `;
            container.appendChild(particle);
        }
    }

    // Configura el área de predicción inicial
    initializePredictionArea() {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="info-message">
                    <span class="info-icon">💡</span>
                    <div class="info-content">
                        <strong>Listo para predecir</strong>
                        <p>Procese archivos y comience a escribir para obtener sugerencias inteligentes</p>
                    </div>
                </div>
            `;
        }
    }

    // ==========================================================================
    // MÉTODOS PARA MANEJO DE ARCHIVOS
    // ==========================================================================

    // Maneja la selección de archivos desde el input
    handleFileSelect(event) {
        const files = Array.from(event.target.files);
        this.addFiles(files);
    }

    // Maneja el evento cuando se arrastra un archivo sobre la zona
    handleDragOver(event) {
        event.preventDefault();
        if (this.uploadZone) {
            this.uploadZone.classList.add('dragover');
        }
    }

    // Maneja cuando el archivo sale de la zona de arrastre
    handleDragLeave(event) {
        event.preventDefault();
        if (this.uploadZone) {
            this.uploadZone.classList.remove('dragover');
        }
    }

    // Maneja cuando se suelta un archivo en la zona
    handleFileDrop(event) {
        event.preventDefault();
        if (this.uploadZone) {
            this.uploadZone.classList.remove('dragover');
        }
        const files = Array.from(event.dataTransfer.files);
        this.addFiles(files);
    }

    // Añade archivos válidos a la lista, filtrando por extensión
    addFiles(newFiles) {
        const validExtensions = ['.txt', '.docx', '.pdf'];
        const validFiles = newFiles.filter(file => {
            const extension = '.' + file.name.split('.').pop().toLowerCase();
            return validExtensions.includes(extension);
        });

        // Notifica si algunos archivos fueron rechazados
        if (validFiles.length !== newFiles.length) {
            this.showNotification('Algunos archivos fueron ignorados. Solo se aceptan .txt, .docx y .pdf', 'warning');
        }

        // Evita archivos duplicados comparando nombres
        const existingNames = this.files.map(f => f.name);
        const uniqueFiles = validFiles.filter(file => !existingNames.includes(file.name));

        if (uniqueFiles.length !== validFiles.length) {
            this.showNotification('Algunos archivos ya estaban agregados', 'info');
        }

        // Actualiza la lista de archivos y la interfaz
        this.files = [...this.files, ...uniqueFiles];
        this.updateFileList();
        this.updateProcessButton();
        this.animateNewFiles();
    }

    // Actualiza la visualización de la lista de archivos
    updateFileList() {
        if (!this.fileList) return;

        if (this.files.length === 0) {
            this.fileList.innerHTML = '';
            return;
        }

        // Genera HTML para cada archivo con información y botón de eliminar
        this.fileList.innerHTML = this.files.map((file, index) => `
            <div class="file-item fade-in" data-index="${index}" style="animation-delay: ${index * 0.1}s">
                <div class="file-icon">${this.getFileIcon(file.name)}</div>
                <div class="file-info">
                    <div class="file-name" title="${this.escapeHtml(file.name)}">${this.escapeHtml(file.name)}</div>
                    <div class="file-size">${this.formatFileSize(file.size)}</div>
                </div>
                <button class="file-remove" onclick="textPredictionSystem.removeFile(${index})" title="Eliminar archivo">
                    <span class="remove-icon">×</span>
                </button>
            </div>
        `).join('');
    }

    // Elimina un archivo específico de la lista
    removeFile(index) {
        if (index >= 0 && index < this.files.length) {
            const removedFile = this.files[index];
            this.files.splice(index, 1);
            this.updateFileList();
            this.updateProcessButton();
            this.showNotification(`Archivo "${removedFile.name}" eliminado`, 'info');
        }
    }

    // Devuelve el icono apropiado según la extensión del archivo
    getFileIcon(filename) {
        const extension = filename.split('.').pop().toLowerCase();
        const icons = {
            'txt': '📄',
            'docx': '📝',
            'pdf': '📕',
            'doc': '📝'
        };
        return icons[extension] || '📄';
    }

    // Convierte bytes a formato legible (KB, MB, etc.)
    formatFileSize(bytes) {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    // Actualiza el estado del botón de procesamiento
    updateProcessButton() {
        if (this.processBtn && this.filesProcessed) {
            this.processBtn.disabled = this.files.length === 0 || this.isProcessing;
            this.filesProcessed.textContent = `0/${this.files.length}`;
        }
    }

    // Anima la aparición de nuevos archivos en la lista
    animateNewFiles() {
        setTimeout(() => {
            const newFileItems = this.fileList?.querySelectorAll('.file-item:not(.animated)') || [];
            newFileItems.forEach((item, index) => {
                item.classList.add('animated');
                item.style.animationDelay = `${index * 0.1}s`;
                item.classList.add('slide-left');
            });
        }, 50);
    }

    // Limpia completamente el sistema
    clearAll() {
        this.files = [];
        this.processedData = null;
        this.currentPredictions = [];
        this.isModelReady = false;

        this.updateFileList();
        this.updateProcessButton();
        this.updateSourcesList();
        this.clearMetrics();
        this.initializePredictionArea();

        // Limpia también los datos del backend
        this.clearBackendData();

        this.showNotification('Sistema limpiado correctamente', 'success');
    }

    // Envía comando al backend para limpiar datos
    async clearBackendData() {
        try {
            const response = await fetch('/api/archivos/limpiar', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                console.log('[Sistema] Datos del backend limpiados');
            }
        } catch (error) {
            console.warn('[Sistema] Error limpiando datos del backend:', error);
        }
    }

    // Resetea todas las métricas a valores por defecto
    clearMetrics() {
        const metricsElements = [
            this.sequentialTime, this.parallelTime, this.speedup,
            this.efficiency, this.wordsPerSec, this.uniqueWords, this.totalWords
        ];

        metricsElements.forEach(element => {
            if (element) element.textContent = '--';
        });
    }

    // ==========================================================================
    // PROCESAMIENTO DE ARCHIVOS CON INTEGRACIÓN AL BACKEND
    // ==========================================================================

    // Proceso principal que envía archivos al servidor para análisis
    async processFiles() {
        if (this.isProcessing || this.files.length === 0) {
            console.log('[Sistema] Procesamiento cancelado: ya en proceso o sin archivos');
            return;
        }

        console.log('[Sistema] Iniciando procesamiento de archivos...');

        this.isProcessing = true;
        this.processBtn.disabled = true;
        this.showProgress();

        try {
            // Paso 1: Preparar los archivos en formato FormData para envío
            const formData = new FormData();
            this.files.forEach(file => {
                formData.append('files', file);
                console.log(`[Sistema] Agregando archivo: ${file.name} (${this.formatFileSize(file.size)})`);
            });

            this.updateProgressStep('Subiendo archivos al servidor...', 15);

            // Paso 2: Envío real al endpoint del backend
            console.log('[Sistema] Enviando archivos al backend...');
            const response = await fetch('/api/archivos/procesar', {
                method: 'POST',
                body: formData
            });

            this.updateProgressStep('Procesando archivos...', 40);

            // Validación de la respuesta del servidor
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Error HTTP ${response.status}: ${errorText}`);
            }

            const result = await response.json();
            console.log('[Sistema] Respuesta del backend:', result);

            if (!result.success) {
                throw new Error(result.message || 'Error desconocido del servidor');
            }

            this.updateProgressStep('Entrenando modelo de predicción...', 70);

            // Paso 3: Verificar que el modelo esté correctamente entrenado
            await this.delay(500); // Tiempo para que el modelo termine de entrenar
            const modelStatus = await this.checkModelStatus();

            this.updateProgressStep('Procesamiento completado', 100);

            // Paso 4: Mostrar métricas reales obtenidas del backend
            this.showRealMetrics(result);
            this.updateSourcesList(result.archivos || []);

            this.isModelReady = result.modeloEntrenado || false;

            // Mensaje de éxito con detalles del entrenamiento
            let message = 'Archivos procesados exitosamente. ';
            if (this.isModelReady) {
                message += `Modelo entrenado con ${result.bigramas || 0} bigramas y ${result.trigramas || 0} trigramas`;
            } else {
                message += 'Advertencia: Modelo no se entrenó correctamente';
            }

            this.showNotification(message, this.isModelReady ? 'success' : 'warning');

        } catch (error) {
            console.error('[Sistema] Error procesando archivos:', error);
            this.showNotification(`Error: ${error.message}`, 'error');
            this.updateProgressStep('Error en el procesamiento', 0);
        } finally {
            this.isProcessing = false;
            this.hideProgress();
            this.updateProcessButton();
        }
    }

    // Verifica el estado actual del modelo en el backend
    async checkModelStatus() {
        try {
            console.log('[Sistema] Verificando estado del modelo...');
            const response = await fetch('/api/prediccion/estadisticas');

            if (response.ok) {
                const data = await response.json();
                this.isModelReady = data.modeloEntrenado || false;
                console.log('[Sistema] Estado del modelo:', {
                    entrenado: this.isModelReady,
                    bigramas: data.bigramas,
                    trigramas: data.trigramas,
                    contextos: data.contextos
                });
                return data;
            } else {
                console.warn('[Sistema] No se pudo verificar estado del modelo - respuesta no OK');
            }
        } catch (error) {
            console.warn('[Sistema] No se pudo verificar el estado del modelo:', error);
            this.isModelReady = false;
        }
        return null;
    }

    // Actualiza el progreso visual del procesamiento
    updateProgressStep(message, percentage) {
        if (this.progressText) {
            this.progressText.textContent = message;
        }

        if (this.progressFill) {
            this.progressFill.style.width = `${percentage}%`;
        }

        if (this.filesProcessed) {
            const processedCount = Math.floor(percentage / 20);
            this.filesProcessed.textContent = `${Math.min(processedCount, this.files.length)}/${this.files.length}`;
        }
    }

    // Muestra métricas reales obtenidas del procesamiento
    showRealMetrics(data) {
        console.log('[Sistema] Mostrando métricas reales:', data);

        // Actualiza cada métrica con animación suave
        if (this.sequentialTime) {
            this.animateMetricUpdate(this.sequentialTime, `${data.tiempoSecuencialSeg?.toFixed(2) || '0.00'}s`);
        }
        if (this.parallelTime) {
            this.animateMetricUpdate(this.parallelTime, `${data.tiempoParaleloSeg?.toFixed(2) || '0.00'}s`);
        }
        if (this.speedup) {
            this.animateMetricUpdate(this.speedup, `${data.speedup?.toFixed(2) || '1.0'}x ${this.getSpeedupLabel(data.speedup)}`);
        }
        if (this.efficiency) {
            this.animateMetricUpdate(this.efficiency, `${(data.eficiencia * 100)?.toFixed(1) || '0.0'}%`);
        }
        if (this.wordsPerSec) {
            const seqWPS = data.palabrasSecuencialPorSeg?.toLocaleString() || '0';
            const parWPS = data.palabrasParaleloPorSeg?.toLocaleString() || '0';
            this.animateMetricUpdate(this.wordsPerSec, `${seqWPS} → ${parWPS}`);
        }
        if (this.uniqueWords) {
            this.animateMetricUpdate(this.uniqueWords, (data.palabrasUnicas || 0).toLocaleString());
        }
        if (this.totalWords) {
            this.animateMetricUpdate(this.totalWords, (data.palabrasTotal || 0).toLocaleString());
        }

        // Almacena los datos procesados para uso posterior
        this.processedData = {
            totalWords: data.palabrasTotal || 0,
            uniqueWords: data.palabrasUnicas || 0,
            files: this.files.map(file => file.name),
            realData: true,
            archivos: data.archivos || [],
            modelData: {
                bigramas: data.bigramas || 0,
                trigramas: data.trigramas || 0,
                contextosEntrenamiento: data.contextosEntrenamiento || 0
            }
        };
    }

    // Evalúa el rendimiento del speedup obtenido
    getSpeedupLabel(speedup) {
        if (!speedup) return '';
        if (speedup >= 3.5) return 'Excelente';
        if (speedup >= 2.5) return 'Muy Bueno';
        if (speedup >= 1.5) return 'Bueno';
        if (speedup >= 1.1) return 'Aceptable';
        return 'Pobre';
    }

    // Muestra la barra de progreso
    showProgress() {
        if (this.progressSection) {
            this.progressSection.classList.add('visible');
        }
        if (this.progressFill) {
            this.progressFill.style.width = '0%';
        }
    }

    // Oculta la barra de progreso después del procesamiento
    hideProgress() {
        setTimeout(() => {
            if (this.progressSection) {
                this.progressSection.classList.remove('visible');
            }
        }, 2000);
    }

    // Anima la actualización de métricas con efecto visual
    animateMetricUpdate(element, newValue) {
        if (!element) return;

        // Efecto de contracción
        element.style.transform = 'scale(0.8)';
        element.style.opacity = '0.5';

        setTimeout(() => {
            element.textContent = newValue;
            // Efecto de expansión
            element.style.transform = 'scale(1.1)';
            element.style.opacity = '1';

            setTimeout(() => {
                element.style.transform = 'scale(1)';
            }, 200);
        }, 100);
    }

    // Actualiza la lista de archivos fuente procesados
    updateSourcesList(archivos = null) {
        if (!this.sourcesList) return;

        if (this.files.length === 0 && (!archivos || archivos.length === 0)) {
            this.sourcesList.innerHTML = `
                <div class="empty-state">
                    <div class="empty-icon">📖</div>
                    <p>Los archivos procesados aparecerán aquí</p>
                </div>
            `;
            return;
        }

        // Usa datos reales del backend si están disponibles
        const datosArchivos = archivos && archivos.length > 0
            ? archivos
            : this.files.map(file => ({
                nombre: file.name,
                estado: 'Pendiente',
                palabras: 0,
                tamaño: this.formatFileSize(file.size)
            }));

        this.sourcesList.innerHTML = datosArchivos.map((archivo, index) => `
            <div class="source-item fade-in" style="animation-delay: ${index * 0.1}s">
                <div class="source-header">
                    <span class="source-icon">${this.getFileIcon(archivo.nombre)}</span>
                    <span class="source-name" title="${this.escapeHtml(archivo.nombre)}">${this.escapeHtml(archivo.nombre)}</span>
                    <span class="source-status ${(archivo.estado || 'procesado').toLowerCase()}">${archivo.estado || 'Procesado'}</span>
                </div>
                <div class="source-stats">
                    <div class="source-stat">
                        <span>Palabras:</span>
                        <span>${(archivo.palabras || 0).toLocaleString()}</span>
                    </div>
                    <div class="source-stat">
                        <span>Tamaño:</span>
                        <span>${archivo.tamaño || 'N/A'}</span>
                    </div>
                </div>
            </div>
        `).join('');
    }

    // ==========================================================================
    // SISTEMA DE PREDICCIÓN DE TEXTO EN TIEMPO REAL
    // ==========================================================================

    // Maneja la entrada de texto y activa predicciones
    async handleTextInput(event) {
        const text = event.target.value;

        // Cancela el timer anterior para evitar llamadas excesivas
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }

        // Implementa debounce para optimizar rendimiento
        this.debounceTimer = setTimeout(async () => {
            console.log('[Predicción] Texto ingresado:', text);

            if (text.trim().length > 0) {
                this.showSuggestions();
                await this.generateRealPredictions(text);
            } else {
                this.hideSuggestions();
            }
        }, 300); // Espera 300ms antes de procesar
    }

    // Genera predicciones reales usando el modelo entrenado
    async generateRealPredictions(texto) {
        if (!this.isModelReady) {
            this.showNotConnectedMessage();
            return;
        }

        if (!this.suggestionsGrid) {
            console.warn('[Predicción] suggestionsGrid no encontrado');
            return;
        }

        try {
            console.log('[Predicción] Enviando contexto al modelo:', texto);

            this.showLoadingPredictions();

            // Llamada real al API de predicción del backend
            const response = await fetch('/api/prediccion/predecir', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    contexto: texto,
                    topK: 8 // Solicita las 8 mejores predicciones
                })
            });

            if (!response.ok) {
                throw new Error(`Error HTTP: ${response.status} - ${response.statusText}`);
            }

            const result = await response.json();
            console.log('[Predicción] Respuesta del modelo:', result);

            if (result.success && result.predicciones && result.predicciones.length > 0) {
                this.displayRealSuggestions(result.predicciones);
                this.currentPredictions = result.predicciones;
            } else {
                this.displayNoSuggestionsMessage(result.message || 'No se encontraron predicciones');
                this.currentPredictions = [];
            }

        } catch (error) {
            console.error('[Predicción] Error generando predicciones:', error);
            this.displayErrorMessage(error.message);
            this.currentPredictions = [];
        }
    }

    // Muestra indicador de carga mientras se generan predicciones
    showLoadingPredictions() {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="loading-message">
                    <div class="loading-spinner"></div>
                    <div class="loading-content">
                        <strong>Generando predicciones...</strong>
                        <p>Analizando contexto con IA</p>
                    </div>
                </div>
            `;
        }
    }

    // Muestra las sugerencias reales obtenidas del modelo
    displayRealSuggestions(predicciones) {
        console.log('[Predicción] Mostrando sugerencias reales:', predicciones);

        if (!this.suggestionsGrid) {
            console.error('[Predicción] suggestionsGrid no disponible');
            return;
        }

        this.suggestionsGrid.innerHTML = predicciones.map((pred, index) => {
            // Formato de archivos origen para mostrar
            const archivosTexto = pred.archivos && pred.archivos.length > 0
                ? (pred.archivos.length > 2
                    ? `${pred.archivos.slice(0, 2).join(', ')} +${pred.archivos.length - 2} más`
                    : pred.archivos.join(', '))
                : 'Origen desconocido';

            // Badge del método usado (trigrama o bigrama)
            const metodoBadge = pred.metodo === 'trigrama'
                ? '<span class="metodo-badge trigrama" title="Predicción basada en 3 palabras">3-gram</span>'
                : '<span class="metodo-badge bigrama" title="Predicción basada en 2 palabras">2-gram</span>';

            // Información de confianza si está disponible
            const confianzaDisplay = pred.confianza
                ? `<div class="confianza">
                     <span class="confianza-label">Confianza:</span>
                     <span class="confianza-value">${pred.confianza}%</span>
                   </div>`
                : '';

            return `
                <div class="suggestion-card scale-in" 
                     style="animation-delay: ${index * 0.05}s"
                     onclick="textPredictionSystem.applySuggestion('${this.escapeHtml(pred.palabra).replace(/'/g, "\\'")}', ${index})"
                     data-index="${index}">
                    
                    <div class="suggestion-header">
                        <span class="suggestion-word">${this.escapeHtml(pred.palabra)}</span>
                        ${metodoBadge}
                    </div>
                    
                    <div class="suggestion-info">
                        <div class="relevancia">
                            <span class="relevancia-label">Relevancia:</span>
                            <span class="relevancia-value">${pred.relevancia || pred.archivos?.length || 0}</span>
                        </div>
                        
                        <div class="archivos-origen">
                            <span class="archivos-label">Origen:</span>
                            <span class="archivos-list" title="${this.escapeHtml(pred.archivos?.join(', ') || 'N/A')}">${this.escapeHtml(archivosTexto)}</span>
                        </div>

                        ${confianzaDisplay}
                    </div>
                </div>
            `;
        }).join('');

        this.ensureStyles();
    }

    // Aplica una sugerencia seleccionada al campo de texto
    applySuggestion(palabra, index = -1) {
        if (!this.textInput) {
            console.error('[Predicción] textInput no disponible');
            return;
        }

        const currentText = this.textInput.value;
        console.log('[Predicción] Aplicando sugerencia:', palabra);

        // Lógica para insertar la palabra correctamente
        let newText = '';
        if (currentText.endsWith(' ') || currentText === '') {
            // Si termina en espacio o está vacío, añade la palabra directamente
            newText = currentText + palabra + ' ';
        } else {
            // Si hay texto, reemplaza la última palabra
            const words = currentText.trim().split(/\s+/);
            words[words.length - 1] = palabra;
            newText = words.join(' ') + ' ';
        }

        this.textInput.value = newText;
        this.textInput.focus();

        // Posiciona el cursor al final
        this.textInput.setSelectionRange(newText.length, newText.length);

        // Efecto visual en la tarjeta seleccionada
        if (index >= 0 && this.suggestionsGrid) {
            const appliedCard = this.suggestionsGrid.querySelector(`[data-index="${index}"]`);
            if (appliedCard) {
                appliedCard.style.transform = 'scale(1.05)';
                appliedCard.style.background = 'rgba(0, 255, 136, 0.2)';
                appliedCard.style.borderColor = '#00ff88';
                appliedCard.style.boxShadow = '0 0 20px rgba(0, 255, 136, 0.4)';

                setTimeout(() => {
                    appliedCard.style.transform = 'scale(1)';
                    appliedCard.style.background = '';
                    appliedCard.style.borderColor = '';
                    appliedCard.style.boxShadow = '';
                }, 600);
            }
        }

        // Genera automáticamente nuevas predicciones para continuar
        setTimeout(() => {
            this.generateRealPredictions(this.textInput.value);
        }, 200);
    }

    // Maneja la navegación por teclado en las sugerencias
    handleKeyDown(event) {
        if (!this.suggestionsGrid) return;

        const suggestions = this.suggestionsGrid.querySelectorAll('.suggestion-card:not(.loading-message):not(.info-message):not(.error-message)');
        if (suggestions.length === 0) return;

        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                this.navigateSuggestions('down', suggestions);
                break;
            case 'ArrowUp':
                event.preventDefault();
                this.navigateSuggestions('up', suggestions);
                break;
            case 'Tab':
                event.preventDefault();
                if (this.currentPredictions.length > 0) {
                    const selectedIndex = Math.max(0, this.selectedSuggestionIndex);
                    this.applySuggestion(this.currentPredictions[selectedIndex].palabra, selectedIndex);
                }
                break;
            case 'Enter':
                if (this.selectedSuggestionIndex >= 0 && this.currentPredictions[this.selectedSuggestionIndex]) {
                    event.preventDefault();
                    this.applySuggestion(this.currentPredictions[this.selectedSuggestionIndex].palabra, this.selectedSuggestionIndex);
                }
                break;
            case 'Escape':
                this.hideSuggestions();
                this.selectedSuggestionIndex = -1;
                break;
        }
    }

    // Navega entre sugerencias usando las flechas del teclado
    navigateSuggestions(direction, suggestions) {
        // Limpia la selección anterior
        suggestions.forEach(s => s.classList.remove('selected'));

        // Calcula el nuevo índice según la dirección
        if (direction === 'down') {
            this.selectedSuggestionIndex = Math.min(this.selectedSuggestionIndex + 1, suggestions.length - 1);
        } else {
            this.selectedSuggestionIndex = Math.max(this.selectedSuggestionIndex - 1, -1);
        }

        // Aplica la nueva selección visual
        if (this.selectedSuggestionIndex >= 0) {
            suggestions[this.selectedSuggestionIndex].classList.add('selected');
            suggestions[this.selectedSuggestionIndex].scrollIntoView({ block: 'nearest' });
        }
    }

    // Muestra el contenedor de sugerencias
    showSuggestions() {
        if (this.suggestionsContainer) {
            this.suggestionsContainer.classList.add('visible');
        }
    }

    // Oculta el contenedor de sugerencias
    hideSuggestions() {
        if (this.suggestionsContainer) {
            this.suggestionsContainer.classList.remove('visible');
        }
        this.selectedSuggestionIndex = -1;
    }

    // Mensaje cuando el modelo no está entrenado
    showNotConnectedMessage() {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="warning-message">
                    <span class="warning-icon">Advertencia</span>
                    <div class="warning-content">
                        <strong>Modelo no entrenado</strong>
                        <p>Procese archivos primero para obtener predicciones inteligentes basadas en su contenido</p>
                    </div>
                </div>
            `;
        }
    }

    // Mensaje cuando no se encuentran sugerencias
    displayNoSuggestionsMessage(customMessage = '') {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="info-message">
                    <span class="info-icon">Búsqueda</span>
                    <div class="info-content">
                        <strong>Sin predicciones</strong>
                        <p>${customMessage || 'No se encontraron patrones para este contexto en los archivos procesados'}</p>
                        <small>Sugerencia: Intente con otras palabras o procese más archivos para mejorar las predicciones</small>
                    </div>
                </div>
            `;
        }
    }

    // Mensaje de error en las predicciones
    displayErrorMessage(errorMsg = '') {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="error-message">
                    <span class="error-icon">Error</span>
                    <div class="error-content">
                        <strong>Error de predicción</strong>
                        <p>${errorMsg || 'Error al comunicar con el servidor'}</p>
                        <small>Intente nuevamente en unos momentos</small>
                    </div>
                </div>
            `;
        }
    }

    // ==========================================================================
    // MÉTODOS AUXILIARES Y UTILIDADES
    // ==========================================================================

    // Sistema de notificaciones toast
    showNotification(message, type = 'info') {
        // Crea el elemento de notificación
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-icon">${this.getNotificationIcon(type)}</span>
                <span class="notification-text">${this.escapeHtml(message)}</span>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;

        // Estilos inline para garantizar funcionalidad
        notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            max-width: 400px;
            background: ${this.getNotificationColor(type)};
            color: white;
            padding: 16px 20px;
            border-radius: 12px;
            backdrop-filter: blur(10px);
            z-index: 10000;
            animation: slideInRight 0.4s cubic-bezier(0.68, -0.55, 0.265, 1.55);
            box-shadow: 0 8px 32px rgba(0,0,0,0.3);
            border: 1px solid rgba(255,255,255,0.1);
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        `;

        // Estilos para el contenido interno
        const content = notification.querySelector('.notification-content');
        content.style.cssText = `
            display: flex;
            align-items: center;
            gap: 12px;
        `;

        const closeBtn = notification.querySelector('.notification-close');
        closeBtn.style.cssText = `
            background: none;
            border: none;
            color: white;
            font-size: 18px;
            cursor: pointer;
            margin-left: auto;
            opacity: 0.7;
            transition: opacity 0.2s;
        `;

        // Efectos hover para el botón de cerrar
        closeBtn.addEventListener('mouseenter', () => closeBtn.style.opacity = '1');
        closeBtn.addEventListener('mouseleave', () => closeBtn.style.opacity = '0.7');

        document.body.appendChild(notification);

        // Auto-eliminación después de 5 segundos
        setTimeout(() => {
            if (notification.parentNode) {
                notification.style.animation = 'slideOutRight 0.4s ease';
                setTimeout(() => {
                    if (notification.parentNode) {
                        notification.parentNode.removeChild(notification);
                    }
                }, 400);
            }
        }, 5000);
    }

    // Iconos para diferentes tipos de notificación
    getNotificationIcon(type) {
        const icons = {
            'success': 'Éxito',
            'error': 'Error',
            'warning': 'Advertencia',
            'info': 'Info'
        };
        return icons[type] || 'Info';
    }

    // Colores de fondo para notificaciones
    getNotificationColor(type) {
        const colors = {
            'success': 'linear-gradient(135deg, rgba(40, 167, 69, 0.95), rgba(25, 135, 84, 0.95))',
            'error': 'linear-gradient(135deg, rgba(220, 53, 69, 0.95), rgba(176, 42, 55, 0.95))',
            'warning': 'linear-gradient(135deg, rgba(255, 193, 7, 0.95), rgba(255, 143, 0, 0.95))',
            'info': 'linear-gradient(135deg, rgba(23, 162, 184, 0.95), rgba(13, 110, 253, 0.95))'
        };
        return colors[type] || colors['info'];
    }

    // Función de seguridad para escapar HTML y prevenir XSS
    escapeHtml(unsafe) {
        if (typeof unsafe !== 'string') return '';
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    // Función auxiliar para crear delays
    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    // Asegura que los estilos CSS estén cargados
    ensureStyles() {
        if (document.getElementById('prediction-styles')) return;

        const styles = document.createElement('style');
        styles.id = 'prediction-styles';
        styles.textContent = `
            /* Estilos para las tarjetas de sugerencia */
            .suggestion-card {
                background: rgba(255, 255, 255, 0.05);
                border: 1px solid rgba(0, 255, 136, 0.2);
                border-radius: 12px;
                padding: 16px;
                cursor: pointer;
                transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
                backdrop-filter: blur(10px);
                position: relative;
                overflow: hidden;
            }

            .suggestion-card:hover, .suggestion-card.selected {
                background: rgba(0, 255, 136, 0.1);
                border-color: rgba(0, 255, 136, 0.5);
                transform: translateY(-2px);
                box-shadow: 0 8px 32px rgba(0, 255, 136, 0.3);
            }

            .suggestion-card::before {
                content: '';
                position: absolute;
                top: 0;
                left: -100%;
                width: 100%;
                height: 100%;
                background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
                transition: left 0.5s;
            }

            .suggestion-card:hover::before {
                left: 100%;
            }

            .suggestion-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 12px;
            }

            .suggestion-word {
                font-weight: bold;
                color: #00ff88;
                font-size: 1.2em;
                text-shadow: 0 0 10px rgba(0, 255, 136, 0.3);
                transition: text-shadow 0.3s ease;
            }

            .suggestion-card:hover .suggestion-word {
                text-shadow: 0 0 15px rgba(0, 255, 136, 0.6);
            }

            .metodo-badge {
                padding: 4px 8px;
                border-radius: 6px;
                font-size: 0.7em;
                font-weight: bold;
                text-transform: uppercase;
                letter-spacing: 0.5px;
                transition: all 0.3s ease;
            }

            .metodo-badge.trigrama {
                background: rgba(0, 255, 136, 0.2);
                color: #00ff88;
                border: 1px solid rgba(0, 255, 136, 0.3);
            }

            .metodo-badge.bigrama {
                background: rgba(136, 136, 255, 0.2);
                color: #8888ff;
                border: 1px solid rgba(136, 136, 255, 0.3);
            }

            .suggestion-info {
                font-size: 0.85em;
                color: rgba(255, 255, 255, 0.8);
            }

            .relevancia, .archivos-origen, .confianza {
                display: flex;
                justify-content: space-between;
                margin-bottom: 6px;
            }

            .relevancia-value {
                color: #ffaa00;
                font-weight: bold;
                background: linear-gradient(45deg, rgba(255, 170, 0, 0.2), rgba(255, 170, 0, 0.1));
                padding: 2px 6px;
                border-radius: 4px;
                border: 1px solid rgba(255, 170, 0, 0.3);
                transition: all 0.3s ease;
            }

            .confianza-value {
                color: #00dd88;
                font-weight: bold;
                background: rgba(0, 221, 136, 0.1);
                padding: 2px 6px;
                border-radius: 4px;
                font-size: 0.9em;
                border: 1px solid rgba(0, 221, 136, 0.2);
            }

            .archivos-list {
                color: rgba(255, 255, 255, 0.9);
                font-style: italic;
                max-width: 150px;
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: nowrap;
                cursor: help;
                transition: all 0.3s ease;
            }

            .archivos-list:hover {
                color: rgba(255, 255, 255, 1);
                text-decoration: underline;
                max-width: none;
                white-space: normal;
            }

            /* Mensajes de estado del sistema */
            .warning-message, .info-message, .error-message, .loading-message {
                text-align: center;
                padding: 24px;
                border-radius: 12px;
                display: flex;
                flex-direction: column;
                align-items: center;
                gap: 12px;
                backdrop-filter: blur(10px);
                animation: fadeIn 0.5s ease;
            }

            .warning-message {
                background: rgba(255, 193, 7, 0.1);
                border: 1px solid rgba(255, 193, 7, 0.3);
                color: #ffc107;
            }

            .info-message {
                background: rgba(0, 123, 255, 0.1);
                border: 1px solid rgba(0, 123, 255, 0.3);
                color: #007bff;
            }

            .error-message {
                background: rgba(220, 53, 69, 0.1);
                border: 1px solid rgba(220, 53, 69, 0.3);
                color: #dc3545;
            }

            .loading-message {
                background: rgba(0, 255, 136, 0.05);
                border: 1px solid rgba(0, 255, 136, 0.2);
                color: #00ff88;
            }

            .warning-content, .info-content, .error-content, .loading-content {
                text-align: center;
            }

            .warning-content strong, .info-content strong, .error-content strong, .loading-content strong {
                display: block;
                font-size: 1.1em;
                margin-bottom: 6px;
            }

            .warning-content small, .info-content small, .error-content small {
                display: block;
                margin-top: 8px;
                opacity: 0.8;
                font-size: 0.85em;
            }

            /* Spinner de carga animado */
            .loading-spinner {
                width: 32px;
                height: 32px;
                border: 3px solid rgba(0, 255, 136, 0.3);
                border-top: 3px solid #00ff88;
                border-radius: 50%;
                animation: spin 1s linear infinite;
            }

            /* Estilos para elementos de archivo */
            .file-item {
                display: flex;
                align-items: center;
                gap: 12px;
                padding: 12px;
                background: rgba(255, 255, 255, 0.05);
                border: 1px solid rgba(255, 255, 255, 0.1);
                border-radius: 8px;
                margin-bottom: 8px;
                transition: all 0.3s ease;
            }

            .file-item:hover {
                background: rgba(255, 255, 255, 0.1);
                transform: translateX(4px);
            }

            .file-remove {
                background: rgba(220, 53, 69, 0.2);
                border: 1px solid rgba(220, 53, 69, 0.4);
                color: #dc3545;
                border-radius: 50%;
                width: 28px;
                height: 28px;
                display: flex;
                align-items: center;
                justify-content: center;
                cursor: pointer;
                transition: all 0.3s ease;
            }

            .file-remove:hover {
                background: rgba(220, 53, 69, 0.4);
                transform: scale(1.1);
            }

            .remove-icon {
                font-size: 16px;
                font-weight: bold;
            }

            .file-info {
                flex: 1;
                min-width: 0;
            }

            .file-name {
                font-weight: 500;
                color: rgba(255, 255, 255, 0.9);
                overflow: hidden;
                text-overflow: ellipsis;
                white-space: nowrap;
            }

            .file-size {
                font-size: 0.85em;
                color: rgba(255, 255, 255, 0.6);
            }

            /* Estados visuales de archivos */
            .source-status {
                padding: 2px 8px;
                border-radius: 12px;
                font-size: 0.75em;
                font-weight: bold;
                text-transform: uppercase;
            }

            .source-status.procesado {
                background: rgba(40, 167, 69, 0.2);
                color: #28a745;
                border: 1px solid rgba(40, 167, 69, 0.3);
            }

            .source-status.pendiente {
                background: rgba(255, 193, 7, 0.2);
                color: #ffc107;
                border: 1px solid rgba(255, 193, 7, 0.3);
            }

            .source-status.error {
                background: rgba(220, 53, 69, 0.2);
                color: #dc3545;
                border: 1px solid rgba(220, 53, 69, 0.3);
            }

            /* Definición de animaciones CSS */
            @keyframes fadeIn {
                from { opacity: 0; transform: translateY(10px); }
                to { opacity: 1; transform: translateY(0); }
            }

            @keyframes scaleIn {
                from {
                    opacity: 0;
                    transform: scale(0.8) translateY(20px);
                }
                to {
                    opacity: 1;
                    transform: scale(1) translateY(0);
                }
            }

            @keyframes slideInRight {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }

            @keyframes slideOutRight {
                from { transform: translateX(0); opacity: 1; }
                to { transform: translateX(100%); opacity: 0; }
            }

            @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
            }

            @keyframes float {
                0%, 100% { transform: translateY(0px) rotate(0deg); }
                25% { transform: translateY(-10px) rotate(90deg); }
                50% { transform: translateY(0px) rotate(180deg); }
                75% { transform: translateY(10px) rotate(270deg); }
            }

            /* Clases para aplicar animaciones */
            .scale-in {
                animation: scaleIn 0.4s cubic-bezier(0.68, -0.55, 0.265, 1.55) forwards;
            }

            .fade-in {
                animation: fadeIn 0.5s ease forwards;
            }

            .slide-left {
                animation: slideLeft 0.3s ease forwards;
            }

            @keyframes slideLeft {
                from { transform: translateX(20px); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }

            /* Diseño responsivo para dispositivos móviles */
            @media (max-width: 768px) {
                .suggestion-card {
                    padding: 12px;
                }
                
                .suggestion-word {
                    font-size: 1.1em;
                }
                
                .archivos-list {
                    max-width: 100px;
                }
            }
        `;

        document.head.appendChild(styles);
    }

    // Función de diagnóstico para debugging
    getSystemStatus() {
        return {
            filesLoaded: this.files.length,
            modelReady: this.isModelReady,
            isProcessing: this.isProcessing,
            currentPredictions: this.currentPredictions.length,
            processedData: !!this.processedData
        };
    }
}

// ==========================================================================
// INICIALIZACIÓN Y CONFIGURACIÓN DEL SISTEMA
// ==========================================================================

// Variable global para acceso al sistema
let textPredictionSystem;

// Función principal de inicialización
function initializeSystem() {
    console.log('[Sistema] Inicializando Sistema de Predicción de Texto...');

    try {
        // Crear la instancia principal del sistema
        textPredictionSystem = new TextPredictionSystem();

        // Hacer accesible globalmente para debugging y uso desde HTML
        window.textPredictionSystem = textPredictionSystem;

        console.log('[Sistema] Sistema inicializado correctamente');
        console.log('[Sistema] Desarrollado por: CANDY • ANGEL • JASON • ISMER • INDI');

        // Verificar conectividad con el backend después de un breve delay
        setTimeout(() => {
            textPredictionSystem.checkModelStatus()
                .then(status => {
                    if (status) {
                        console.log('[Sistema] Conectividad con backend verificada');
                    } else {
                        console.log('[Sistema] Backend no responde, verifique controladores');
                    }
                });
        }, 1000);

    } catch (error) {
        console.error('[Sistema] Error inicializando sistema:', error);

        // Mostrar error visual si ocurre algún problema
        const errorDiv = document.createElement('div');
        errorDiv.style.cssText = `
            position: fixed;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            background: rgba(220, 53, 69, 0.95);
            color: white;
            padding: 20px;
            border-radius: 12px;
            text-align: center;
            z-index: 10001;
            max-width: 400px;
        `;
        errorDiv.innerHTML = `
            <h3>Error del Sistema</h3>
            <p>No se pudo inicializar el sistema de predicción.</p>
            <small>Error: ${error.message}</small>
            <br><br>
            <button onclick="this.parentElement.remove()">Cerrar</button>
        `;
        document.body.appendChild(errorDiv);
    }
}

// Inicialización cuando el DOM esté completamente cargado
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeSystem);
} else {
    initializeSystem();
}

// Sistema de respaldo para garantizar la inicialización
window.addEventListener('load', () => {
    if (!window.textPredictionSystem) {
        console.warn('[Sistema] Activando inicialización de respaldo');
        initializeSystem();
    }
});