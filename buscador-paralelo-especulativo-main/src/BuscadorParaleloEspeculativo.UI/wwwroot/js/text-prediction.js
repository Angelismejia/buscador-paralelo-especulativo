// ==========================================================================
// SISTEMA DE PREDICCIÓN DE TEXTO - CORREGIDO PARA FUNCIONAR CON BACKEND
// Developed by: CANDY • ANGEL • JASON • ISMER • INDI
// ==========================================================================

class TextPredictionSystem {
    constructor() {
        this.files = [];
        this.processedData = null;
        this.isProcessing = false;
        this.currentPredictions = [];
        this.isModelReady = false;
        this.selectedSuggestionIndex = -1;
        this.debounceTimer = null;

        this.initializeElements();
        this.bindEvents();
        this.startAnimations();
        this.checkModelStatus();
    }

    initializeElements() {
        // Upload elements //
        this.uploadZone = document.getElementById('uploadZone');
        this.fileInput = document.getElementById('fileInput');
        this.fileList = document.getElementById('fileList');
        this.processBtn = document.getElementById('processBtn');
        this.clearBtn = document.getElementById('clearBtn');

        // Progress elements
        this.progressSection = document.getElementById('progressSection');
        this.progressFill = document.getElementById('progressFill');
        this.progressText = document.getElementById('progressText');

        // Metrics elements
        this.sequentialTime = document.getElementById('sequentialTime');
        this.parallelTime = document.getElementById('parallelTime');
        this.speedup = document.getElementById('speedup');
        this.efficiency = document.getElementById('efficiency');
        this.wordsPerSec = document.getElementById('wordsPerSec');
        this.filesProcessed = document.getElementById('filesProcessed');
        this.uniqueWords = document.getElementById('uniqueWords');
        this.totalWords = document.getElementById('totalWords');

        // Text prediction elements
        this.textInput = document.getElementById('textInput');
        this.suggestionsContainer = document.getElementById('suggestionsContainer');
        this.suggestionsGrid = document.getElementById('suggestionsGrid');
        this.sourcesList = document.getElementById('sourcesList');

        console.log('[Sistema] Elementos inicializados correctamente');
    }

    bindEvents() {
        // File upload events
        if (this.uploadZone) {
            this.uploadZone.addEventListener('click', () => this.fileInput?.click());
        }

        if (this.fileInput) {
            this.fileInput.addEventListener('change', (e) => this.handleFileSelect(e));
        }

        // Drag and drop events
        if (this.uploadZone) {
            this.uploadZone.addEventListener('dragover', (e) => this.handleDragOver(e));
            this.uploadZone.addEventListener('dragleave', (e) => this.handleDragLeave(e));
            this.uploadZone.addEventListener('drop', (e) => this.handleFileDrop(e));
        }

        // Button events
        if (this.processBtn) {
            this.processBtn.addEventListener('click', () => this.processFiles());
        }

        if (this.clearBtn) {
            this.clearBtn.addEventListener('click', () => this.clearAll());
        }

        // Text prediction events
        if (this.textInput) {
            this.textInput.addEventListener('input', (e) => this.handleTextInput(e));
            this.textInput.addEventListener('keydown', (e) => this.handleKeyDown(e));
            this.textInput.addEventListener('focus', () => this.showSuggestions());
            this.textInput.addEventListener('blur', () => {
                setTimeout(() => this.hideSuggestions(), 200);
            });
        }

        this.bindTitleAnimations();
    }

    bindTitleAnimations() {
        const titleParts = document.querySelectorAll('.title-part');
        titleParts.forEach((part, index) => {
            part.addEventListener('mouseenter', () => {
                part.style.transform = `scale(1.1) rotate(${Math.random() * 10 - 5}deg)`;
                part.style.transition = 'all 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55)';
            });

            part.addEventListener('mouseleave', () => {
                part.style.transform = 'scale(1) rotate(0deg)';
            });
        });
    }

    startAnimations() {
        this.createFloatingParticles();
        this.initializePredictionArea();
    }

    createFloatingParticles() {
        const container = document.querySelector('.floating-particles');
        if (!container) return;

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

    initializePredictionArea() {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="info-message">
                    <span class="info-icon">💡</span>
                    <div class="info-content">
                        <strong>¡Listo para predecir!</strong>
                        <p>Procese archivos y comience a escribir para obtener sugerencias inteligentes</p>
                    </div>
                </div>
            `;
        }
    }

    // ==========================================================================
    // FILE HANDLING METHODS
    // ==========================================================================

    handleFileSelect(event) {
        const files = Array.from(event.target.files);
        this.addFiles(files);
    }

    handleDragOver(event) {
        event.preventDefault();
        if (this.uploadZone) {
            this.uploadZone.classList.add('dragover');
        }
    }

    handleDragLeave(event) {
        event.preventDefault();
        if (this.uploadZone) {
            this.uploadZone.classList.remove('dragover');
        }
    }

    handleFileDrop(event) {
        event.preventDefault();
        if (this.uploadZone) {
            this.uploadZone.classList.remove('dragover');
        }
        const files = Array.from(event.dataTransfer.files);
        this.addFiles(files);
    }

    addFiles(newFiles) {
        const validExtensions = ['.txt', '.docx', '.pdf'];
        const validFiles = newFiles.filter(file => {
            const extension = '.' + file.name.split('.').pop().toLowerCase();
            return validExtensions.includes(extension);
        });

        if (validFiles.length !== newFiles.length) {
            this.showNotification('Algunos archivos fueron ignorados. Solo se aceptan .txt, .docx y .pdf', 'warning');
        }

        // Evitar duplicados
        const existingNames = this.files.map(f => f.name);
        const uniqueFiles = validFiles.filter(file => !existingNames.includes(file.name));

        if (uniqueFiles.length !== validFiles.length) {
            this.showNotification('Algunos archivos ya estaban agregados', 'info');
        }

        this.files = [...this.files, ...uniqueFiles];
        this.updateFileList();
        this.updateProcessButton();
        this.animateNewFiles();
    }

    updateFileList() {
        if (!this.fileList) return;

        if (this.files.length === 0) {
            this.fileList.innerHTML = '';
            return;
        }

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

    removeFile(index) {
        if (index >= 0 && index < this.files.length) {
            const removedFile = this.files[index];
            this.files.splice(index, 1);
            this.updateFileList();
            this.updateProcessButton();
            this.showNotification(`Archivo "${removedFile.name}" eliminado`, 'info');
        }
    }

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

    formatFileSize(bytes) {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    }

    updateProcessButton() {
        if (this.processBtn && this.filesProcessed) {
            this.processBtn.disabled = this.files.length === 0 || this.isProcessing;
            this.filesProcessed.textContent = `0/${this.files.length}`;
        }
    }

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

        // Llamar al backend para limpiar datos
        this.clearBackendData();

        this.showNotification('Sistema limpiado correctamente', 'success');
    }

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
    // PROCESAMIENTO REAL DE ARCHIVOS CON BACKEND
    // ==========================================================================

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
            // PASO 1: Preparar FormData con archivos reales
            const formData = new FormData();
            this.files.forEach(file => {
                formData.append('files', file);
                console.log(`[Sistema] Agregando archivo: ${file.name} (${this.formatFileSize(file.size)})`);
            });

            this.updateProgressStep('Subiendo archivos al servidor...', 15);

            // PASO 2: Llamada REAL al backend para procesar archivos
            console.log('[Sistema] Enviando archivos al backend...');
            const response = await fetch('/api/archivos/procesar', {
                method: 'POST',
                body: formData
            });

            this.updateProgressStep('Procesando archivos...', 40);

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

            // PASO 3: Verificar que el modelo esté listo
            await this.delay(500); // Dar tiempo al modelo para entrenar
            const modelStatus = await this.checkModelStatus();

            this.updateProgressStep('¡Procesamiento completado!', 100);

            // PASO 4: Mostrar métricas REALES del backend
            this.showRealMetrics(result);
            this.updateSourcesList(result.archivos || []);

            this.isModelReady = result.modeloEntrenado || false;

            let message = '¡Archivos procesados exitosamente! ';
            if (this.isModelReady) {
                message += `Modelo entrenado con ${result.bigramas || 0} bigramas y ${result.trigramas || 0} trigramas 🎉`;
            } else {
                message += 'Advertencia: Modelo no se entrenó correctamente ⚠️';
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

    showRealMetrics(data) {
        console.log('[Sistema] Mostrando métricas reales:', data);

        // CORREGIDO: Usar nombres correctos de las propiedades
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

    getSpeedupLabel(speedup) {
        if (!speedup) return '';
        if (speedup >= 3.5) return '🚀 ¡Excelente!';
        if (speedup >= 2.5) return '⚡ ¡Muy Bueno!';
        if (speedup >= 1.5) return '✨ ¡Bueno!';
        if (speedup >= 1.1) return '📈 Aceptable';
        return '📊 Pobre';
    }

    showProgress() {
        if (this.progressSection) {
            this.progressSection.classList.add('visible');
        }
        if (this.progressFill) {
            this.progressFill.style.width = '0%';
        }
    }

    hideProgress() {
        setTimeout(() => {
            if (this.progressSection) {
                this.progressSection.classList.remove('visible');
            }
        }, 2000);
    }

    animateMetricUpdate(element, newValue) {
        if (!element) return;

        element.style.transform = 'scale(0.8)';
        element.style.opacity = '0.5';

        setTimeout(() => {
            element.textContent = newValue;
            element.style.transform = 'scale(1.1)';
            element.style.opacity = '1';

            setTimeout(() => {
                element.style.transform = 'scale(1)';
            }, 200);
        }, 100);
    }

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

        // Usar datos reales si están disponibles, sino usar archivos locales
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
                        <span>📝</span>
                        <span>${(archivo.palabras || 0).toLocaleString()} palabras</span>
                    </div>
                    <div class="source-stat">
                        <span>📦</span>
                        <span>${archivo.tamaño || 'N/A'}</span>
                    </div>
                </div>
            </div>
        `).join('');
    }

    // ==========================================================================
    // SISTEMA DE PREDICCIÓN DE TEXTO REAL
    // ==========================================================================

    async handleTextInput(event) {
        const text = event.target.value;

        // Limpiar timer anterior
        if (this.debounceTimer) {
            clearTimeout(this.debounceTimer);
        }

        // Debounce para evitar demasiadas llamadas al API
        this.debounceTimer = setTimeout(async () => {
            console.log('[Predicción] Texto ingresado:', text);

            if (text.trim().length > 0) {
                this.showSuggestions();
                await this.generateRealPredictions(text);
            } else {
                this.hideSuggestions();
            }
        }, 300); // Esperar 300ms antes de hacer la predicción
    }

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

            // Mostrar indicador de carga
            this.showLoadingPredictions();

            // Llamada REAL al modelo de predicción de ANGEL
            const response = await fetch('/api/prediccion/predecir', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    contexto: texto,
                    topK: 8
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

    displayRealSuggestions(predicciones) {
        console.log('[Predicción] Mostrando sugerencias reales:', predicciones);

        if (!this.suggestionsGrid) {
            console.error('[Predicción] suggestionsGrid no disponible');
            return;
        }

        this.suggestionsGrid.innerHTML = predicciones.map((pred, index) => {
            // CORREGIDO: Acceder a las propiedades correctas de la respuesta
            const archivosTexto = pred.archivos && pred.archivos.length > 0
                ? (pred.archivos.length > 2
                    ? `${pred.archivos.slice(0, 2).join(', ')} +${pred.archivos.length - 2} más`
                    : pred.archivos.join(', '))
                : 'Origen desconocido';

            const metodoBadge = pred.metodo === 'trigrama'
                ? '<span class="metodo-badge trigrama" title="Predicción basada en 3 palabras">3-gram</span>'
                : '<span class="metodo-badge bigrama" title="Predicción basada en 2 palabras">2-gram</span>';

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
                            <span class="archivos-label">📁 Origen:</span>
                            <span class="archivos-list" title="${this.escapeHtml(pred.archivos?.join(', ') || 'N/A')}">${this.escapeHtml(archivosTexto)}</span>
                        </div>

                        ${confianzaDisplay}
                    </div>
                </div>
            `;
        }).join('');

        this.ensureStyles();
    }

    applySuggestion(palabra, index = -1) {
        if (!this.textInput) {
            console.error('[Predicción] textInput no disponible');
            return;
        }

        const currentText = this.textInput.value;
        console.log('[Predicción] Aplicando sugerencia:', palabra);

        // Aplicar la palabra sugerida
        let newText = '';
        if (currentText.endsWith(' ') || currentText === '') {
            newText = currentText + palabra + ' ';
        } else {
            const words = currentText.trim().split(/\s+/);
            words[words.length - 1] = palabra;
            newText = words.join(' ') + ' ';
        }

        this.textInput.value = newText;
        this.textInput.focus();

        // Colocar cursor al final
        this.textInput.setSelectionRange(newText.length, newText.length);

        // Efecto visual en la tarjeta aplicada
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


        setTimeout(() => {
            this.generateRealPredictions(this.textInput.value);
        }, 200);
    }

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

    navigateSuggestions(direction, suggestions) {
        // Limpiar selección anterior
        suggestions.forEach(s => s.classList.remove('selected'));

        // Calcular nueva selección
        if (direction === 'down') {
            this.selectedSuggestionIndex = Math.min(this.selectedSuggestionIndex + 1, suggestions.length - 1);
        } else {
            this.selectedSuggestionIndex = Math.max(this.selectedSuggestionIndex - 1, -1);
        }

        // Aplicar nueva selección
        if (this.selectedSuggestionIndex >= 0) {
            suggestions[this.selectedSuggestionIndex].classList.add('selected');
            suggestions[this.selectedSuggestionIndex].scrollIntoView({ block: 'nearest' });
        }
    }

    showSuggestions() {
        if (this.suggestionsContainer) {
            this.suggestionsContainer.classList.add('visible');
        }
    }

    hideSuggestions() {
        if (this.suggestionsContainer) {
            this.suggestionsContainer.classList.remove('visible');
        }
        this.selectedSuggestionIndex = -1;
    }

    showNotConnectedMessage() {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="warning-message">
                    <span class="warning-icon">⚠️</span>
                    <div class="warning-content">
                        <strong>Modelo no entrenado</strong>
                        <p>Procese archivos primero para obtener predicciones inteligentes basadas en su contenido</p>
                    </div>
                </div>
            `;
        }
    }

    displayNoSuggestionsMessage(customMessage = '') {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="info-message">
                    <span class="info-icon">🔍</span>
                    <div class="info-content">
                        <strong>Sin predicciones</strong>
                        <p>${customMessage || 'No se encontraron patrones para este contexto en los archivos procesados'}</p>
                        <small>💡 Intente con otras palabras o procese más archivos para mejorar las predicciones</small>
                    </div>
                </div>
            `;
        }
    }

    displayErrorMessage(errorMsg = '') {
        if (this.suggestionsGrid) {
            this.suggestionsGrid.innerHTML = `
                <div class="error-message">
                    <span class="error-icon">❌</span>
                    <div class="error-content">
                        <strong>Error de predicción</strong>
                        <p>${errorMsg || 'Error al comunicar con el servidor'}</p>
                        <small>🔄 Intente nuevamente en unos momentos</small>
                    </div>
                </div>
            `;
        }
    }

    // ==========================================================================
    // MÉTODOS AUXILIARES
    // ==========================================================================

    showNotification(message, type = 'info') {
        // Crear notificación toast
        const notification = document.createElement('div');
        notification.className = `notification ${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-icon">${this.getNotificationIcon(type)}</span>
                <span class="notification-text">${this.escapeHtml(message)}</span>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;

        // Estilos inline para asegurar compatibilidad
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

        // Estilos para el contenido
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

        closeBtn.addEventListener('mouseenter', () => closeBtn.style.opacity = '1');
        closeBtn.addEventListener('mouseleave', () => closeBtn.style.opacity = '0.7');

        document.body.appendChild(notification);

        // Auto-remove después de 5 segundos
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

    getNotificationIcon(type) {
        const icons = {
            'success': '✅',
            'error': '❌',
            'warning': '⚠️',
            'info': 'ℹ️'
        };
        return icons[type] || 'ℹ️';
    }

    getNotificationColor(type) {
        const colors = {
            'success': 'linear-gradient(135deg, rgba(40, 167, 69, 0.95), rgba(25, 135, 84, 0.95))',
            'error': 'linear-gradient(135deg, rgba(220, 53, 69, 0.95), rgba(176, 42, 55, 0.95))',
            'warning': 'linear-gradient(135deg, rgba(255, 193, 7, 0.95), rgba(255, 143, 0, 0.95))',
            'info': 'linear-gradient(135deg, rgba(23, 162, 184, 0.95), rgba(13, 110, 253, 0.95))'
        };
        return colors[type] || colors['info'];
    }

    escapeHtml(unsafe) {
        if (typeof unsafe !== 'string') return '';
        return unsafe
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

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

            /* Mensajes de estado */
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

            /* Spinner de carga */
            .loading-spinner {
                width: 32px;
                height: 32px;
                border: 3px solid rgba(0, 255, 136, 0.3);
                border-top: 3px solid #00ff88;
                border-radius: 50%;
                animation: spin 1s linear infinite;
            }

            /* Estilos para archivos */
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

            /* Estados de archivos */
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

            /* Animaciones */
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

            /* Responsive */
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

    // Debug y testing
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
// INICIALIZACIÓN DEL SISTEMA
// ==========================================================================

// Variable global para el sistema
let textPredictionSystem;

// Inicializar cuando el DOM esté listo
function initializeSystem() {
    console.log('[Sistema] Inicializando Sistema de Predicción de Texto Especulativo...');

    try {
        textPredictionSystem = new TextPredictionSystem();

        // Exponer globalmente para debugging y uso desde HTML
        window.textPredictionSystem = textPredictionSystem;

        console.log('[Sistema] ✅ Sistema inicializado correctamente');
        console.log('[Sistema] Desarrollado por: CANDY • ANGEL • JASON • ISMER • INDI');

        // Test de conectividad con backend
        setTimeout(() => {
            textPredictionSystem.checkModelStatus()
                .then(status => {
                    if (status) {
                        console.log('[Sistema] ✅ Conectividad con backend verificada');
                    } else {
                        console.log('[Sistema] ⚠️ Backend no responde, verifique controladores');
                    }
                });
        }, 1000);

    } catch (error) {
        console.error('[Sistema] ❌ Error inicializando sistema:', error);

        // Mostrar error en UI si es posible
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

// Esperar a que el DOM esté completamente cargado
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initializeSystem);
} else {
    initializeSystem();
}

// Backup para asegurar inicialización
window.addEventListener('load', () => {
    if (!window.textPredictionSystem) {
        console.warn('[Sistema] Backup initialization triggered');
        initializeSystem();
    }
});

