/* ==========================================================================
   PROPIEDADES Y ESTILOS BASE
   ========================================================================== */

/* Variables: como atajos para colores, espacios, etc. Hacen el código más fácil de cambiar. */
:root {
    /* Paleta de colores */
    --primary - 900: #0f0f23;
    --primary - 800: #1a1a2e;
    --primary - 700: #16213e;
    --primary - 600: #0f3460;
    --primary - 500: #533483;

    --accent - cyan: #00d9ff;
    --accent - purple: #7b68ee;
    --accent - green: #00ff88;
    --accent - orange: #ff6b35;

    --glass - bg: rgba(255, 255, 255, 0.03);
    --glass - border: rgba(255, 255, 255, 0.1);
    --glass - shadow: rgba(0, 0, 0, 0.3);

    --text - primary: #ffffff;
    --text - secondary: rgba(255, 255, 255, 0.7);
    --text - tertiary: rgba(255, 255, 255, 0.5);

    /* Espacios y tamaños */
    --space - xs: 0.5rem;
    --space - sm: 1rem;
    --space - md: 1.5rem;
    --space - lg: 2rem;
    --space - xl: 3rem;
    --space - 2xl: 4rem;

    /* Bordes redondeados */
    --border - radius - sm: 8px;
    --border - radius - md: 12px;
    --border - radius - lg: 20px;
    --border - radius - xl: 24px;

    /* Velocidades de transiciones y animaciones */
    --transition - fast: 0.2s cubic - bezier(0.4, 0, 0.2, 1);
    --transition - normal: 0.3s cubic - bezier(0.4, 0, 0.2, 1);
    --transition - slow: 0.6s cubic - bezier(0.4, 0, 0.2, 1);
    --transition - elastic: 0.8s cubic - bezier(0.68, -0.55, 0.265, 1.55);
}

/* Reset básico para que los estilos no se vean raros en otros navegadores */
* {
    box- sizing: border - box;
margin: 0;
padding: 0;
}

html {
    /* Tamaño de letra que se ajusta a la pantalla */
    font - size: clamp(14px, 1.5vw, 18px);
    /* Para que el scroll sea suave al navegar */
    scroll - behavior: smooth;
}

body {
    /* Estilo principal de la página: tipografía, color de fondo, etc. */
    font - family: 'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans - serif;
    line - height: 1.6;
    color: var(--text - primary);
    background: var(--primary - 900);
    /* Esconde las barras de scroll horizontales */
    overflow - x: hidden;
    min - height: 100vh;
    position: relative;
}

/* Esconde el header y footer que vienen por defecto en la página */
header, footer {
    display: none;
}

/* ==========================================================================
   SECCIÓN HERO Y FONDO
   ========================================================================== */

/* La sección principal de la página, la que ocupa toda la pantalla al inicio */
.hero - section {
    min - height: 100vh;
    position: relative;
    /* Crea un fondo con degradados circulares de diferentes colores */
    background:
    radial - gradient(circle at 20 % 80 %, var(--primary - 600) 0 %, transparent 50 %),
    radial - gradient(circle at 80 % 20 %, var(--primary - 500) 0 %, transparent 50 %),
    radial - gradient(circle at 40 % 40 %, var(--primary - 700) 0 %, transparent 50 %),
    var(--primary - 900);
    overflow: hidden;
}

/* Los círculos de luz que flotan en el fondo */
.floating - particles {
    position: absolute;
    top: 0;
    left: 0;
    width: 100 %;
    height: 100 %;
    pointer - events: none;
    z - index: 1;
}

/* Crea los dos círculos con pseudo-elementos (no necesitan HTML extra) */
.floating - particles:: before,
.floating - particles::after {
    content: '';
    position: absolute;
    width: 200px;
    height: 200px;
    border - radius: 50 %;
    background: radial - gradient(circle, var(--accent - cyan) 0 %, transparent 70 %);
    opacity: 0.1;
    /* Asigna la animación de "flotar" */
    animation: float 20s ease -in -out infinite;
}

/* Posiciona el primer círculo */
.floating - particles::before {
    top: 20 %;
    left: 10 %;
    animation - delay: 0s;
}

/* Posiciona el segundo círculo, con otro color y retraso en la animación */
.floating - particles::after {
    bottom: 20 %;
    right: 10 %;
    animation - delay: -10s;
    background: radial - gradient(circle, var(--accent - purple) 0 %, transparent 70 %);
}

/* Define la animación para que los círculos se muevan y giren */
@keyframes float {
    0 %, 100 % { transform: translateY(0px) rotate(0deg); }
    33 % { transform: translateY(-30px) rotate(120deg); }
    66 % { transform: translateY(20px) rotate(240deg); }
}

/* ==========================================================================
   CONTENEDOR Y LAYOUT
   ========================================================================== */

/* Contenedor principal que centra todo el contenido y lo separa de los bordes */
.content - wrapper {
    position: relative;
    z - index: 2;
    max - width: 1400px;
    margin: 0 auto;
    padding: var(--space - lg);
    min - height: 100vh;
    display: flex;
    flex - direction: column;
}

/* Título principal de la página */
.main - header {
    text - align: center;
    margin - bottom: var(--space - 2xl);
    padding: var(--space - lg) 0;
}

.system - title {
    /* Tamaño de letra que se adapta a la pantalla */
    font - size: clamp(2.5rem, 6vw, 4.5rem);
    font - weight: 800;
    line - height: 1.2;
    margin - bottom: var(--space - md);
    position: relative;
}

/* Cada palabra del título es un `<span>` para poderle dar estilo individual */
.title - part {
    display: inline - block;
    position: relative;
    margin: 0 0.2em;
    color: var(--text - primary);
    transition: all var(--transition - normal);
    cursor: default ;
}

/* Crea un texto con color de degradado encima del texto original */
.title - part::before {
    /* Toma el texto de un atributo HTML (`data-text="...""`) */
    content: attr(data - text);
    position: absolute;
    top: 0;
    left: 0;
    width: 100 %;
    height: 100 %;
    background: linear - gradient(45deg, var(--accent - cyan), var(--accent - purple));
    /* Esto es lo que hace que el texto tenga un degradado */
    -webkit - background - clip: text;
    -webkit - text - fill - color: transparent;
    background - clip: text;
    opacity: 0;
    transition: opacity var(--transition - normal);
}

/* Cuando pasas el mouse o se le añade la clase "highlight", se muestra el color degradado */
.title - part: hover:: before,
.title - part.highlight::before {
    opacity: 1;
}

/* La animación para que la palabra resalte y brille */
.title - part.highlight {
    animation: glow - pulse 2s ease -in -out infinite;
}

/* Define la animación para el brillo y el pulso */
@keyframes glow - pulse {
    0 %, 100 % {
        filter: drop - shadow(0 0 5px var(--accent - cyan));
    transform: scale(1);
}
50 % {
    filter: drop - shadow(0 0 20px var(--accent - cyan));
transform: scale(1.05);
    }
}

/* Sección para los créditos del equipo */
.team - credits {
    margin - top: var(--space - md);
    opacity: 0.8;
}

/* Cada nombre del equipo */
.credit - item {
    font - size: 1.1rem;
    font - weight: 500;
    letter - spacing: 2px;
    color: var(--text - secondary);
    position: relative;
    display: inline - block;
}

/* Línea que se expande por debajo de cada nombre al pasar el mouse */
.credit - item::after {
    content: '';
    position: absolute;
    bottom: -5px;
    left: 50 %;
    width: 0;
    height: 2px;
    background: var(--accent - cyan);
    transition: all var(--transition - normal);
    transform: translateX(-50 %);
}

/* Al pasar el mouse, la línea se hace visible */
.credit - item: hover::after {
    width: 100 %;
}

/* ==========================================================================
   LAYOUT PRINCIPAL (GRID)
   ========================================================================== */

/* Contenedor que usa CSS Grid para organizar las secciones */
.main - grid {
    display: grid;
    grid - template - columns: repeat(12, 1fr);
    gap: var(--space - lg);
    flex: 1;
}

/* Define el ancho de la sección de subir archivos */
.upload - section {
    grid - column: span 6;
    grid - row: span 1;
}

/* Define el ancho de la sección de métricas */
.metrics - section {
    grid - column: span 6;
    grid - row: span 1;
}

/* Define el ancho de la sección de predicción */
.prediction - section {
    grid - column: span 8;
    grid - row: span 1;
}

/* Define el ancho de la sección de fuentes */
.sources - section {
    grid - column: span 4;
    grid - row: span 1;
}

/* ==========================================================================
   TARJETAS (GLASS MORPHISM)
   ========================================================================== */

/* Estilo para todas las tarjetas con efecto "cristal esmerilado" */
.glass - card {
    /* Color semi-transparente */
    background: var(--glass - bg);
    /* El efecto de desenfoque que hace el cristal */
    backdrop - filter: blur(20px);
    -webkit - backdrop - filter: blur(20px);
    /* Borde y sombra para que se vea como un cristal */
    border: 1px solid var(--glass - border);
    border - radius: var(--border - radius - lg);
    box - shadow:
    0 8px 32px var(--glass - shadow),
        inset 0 1px 0 rgba(255, 255, 255, 0.1);
    position: relative;
    overflow: hidden;
    transition: all var(--transition - normal);
}

/* Efecto de brillo que se mueve por la tarjeta al pasar el mouse */
.glass - card::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100 %;
    width: 100 %;
    height: 100 %;
    background: linear - gradient(
        90deg,
        transparent,
        rgba(255, 255, 255, 0.02),
        transparent
    );
    transition: left 0.8s ease;
    pointer - events: none;
}

/* La tarjeta se mueve un poco y su sombra y borde cambian al pasar el mouse */
.glass - card:hover {
    transform: translateY(-5px);
    border - color: rgba(255, 255, 255, 0.2);
    box - shadow:
    0 20px 40px rgba(0, 0, 0, 0.4),
        inset 0 1px 0 rgba(255, 255, 255, 0.2);
}

/* El brillo se desliza por la tarjeta */
.glass - card: hover::before {
    left: 100 %;
}

/* Estilo para el encabezado de las tarjetas */
.card - header {
    padding: var(--space - lg) var(--space - lg) 0;
    border - bottom: 1px solid rgba(255, 255, 255, 0.05);
    margin - bottom: var(--space - lg);
}

/* Estilo para el título de las tarjetas */
.card - title {
    font - size: 1.5rem;
    font - weight: 700;
    color: var(--text - primary);
    display: flex;
    align - items: center;
    gap: var(--space - sm);
    margin: 0;
}

/* Estilo para el icono del título, con sombra y una animación de flotar */
.icon - wrapper {
    font - size: 1.8rem;
    line - height: 1;
    filter: drop - shadow(0 0 10px currentColor);
    animation: icon - float 3s ease -in -out infinite;
}

/* Define la animación para que el icono se mueva ligeramente hacia arriba y abajo */
@keyframes icon - float {
    0 %, 100 % { transform: translateY(0px); }
    50 % { transform: translateY(-3px); }
}

/* Cuerpo de la tarjeta */
.card - body {
    padding: 0 var(--space - lg) var(--space - lg);
}

/* ==========================================================================
   SECCIÓN DE SUBIR ARCHIVOS
   ========================================================================== */

/* El área donde se arrastran y sueltan los archivos */
.upload - zone {
    border: 2px dashed rgba(255, 255, 255, 0.2);
    border - radius: var(--border - radius - md);
    padding: var(--space - 2xl);
    text - align: center;
    cursor: pointer;
    position: relative;
    transition: all var(--transition - normal);
    background: rgba(255, 255, 255, 0.01);
    margin - bottom: var(--space - lg);
}

/* Al pasar el mouse, cambia el color del borde y el fondo y se agranda un poco */
.upload - zone:hover {
    border - color: var(--accent - cyan);
    background: rgba(0, 217, 255, 0.05);
    transform: scale(1.02);
}

/* Estilo cuando un archivo se está arrastrando sobre la zona, con una animación de brillo */
.upload - zone.dragover {
    border - color: var(--accent - green);
    background: rgba(0, 255, 136, 0.1);
    animation: pulse - glow 1s ease -in -out infinite;
}

/* Define la animación de brillo para la zona de arrastrar */
@keyframes pulse - glow {
    0 %, 100 % { box- shadow: 0 0 0 0 rgba(0, 255, 136, 0.4);
}
50 % { box- shadow: 0 0 0 20px rgba(0, 255, 136, 0); }
}

/* Contenedor del icono y los círculos */
.upload - visual {
    position: relative;
    display: inline - block;
    margin - bottom: var(--space - md);
}

/* El icono de subir archivos, con una animación que lo hace rebotar */
.upload - icon {
    font - size: 4rem;
    line - height: 1;
    position: relative;
    z - index: 2;
    animation: upload - bounce 2s ease -in -out infinite;
}

/* Define la animación de "rebote" para el icono */
@keyframes upload - bounce {
    0 %, 20 %, 50 %, 80 %, 100 % { transform: translateY(0); }
    40 % { transform: translateY(-10px); }
    60 % { transform: translateY(-5px); }
}

/* Contenedor de los círculos animados alrededor del icono */
.upload - rings {
    position: absolute;
    top: 50 %;
    left: 50 %;
    transform: translate(-50 %, -50 %);
    z - index: 1;
}

/* Estilo para cada círculo, que se expande y se desvanece */
.ring {
    position: absolute;
    border: 2px solid var(--accent - cyan);
    border - radius: 50 %;
    opacity: 0.3;
    animation: ring - expand 2s ease - out infinite;
}

/* Diferentes tamaños y retrasos para los tres círculos, para que se vean en cascada */
.ring: nth - child(1) {
    width: 80px;
    height: 80px;
    margin: -40px 0 0 - 40px;
    animation - delay: 0s;
}

.ring: nth - child(2) {
    width: 100px;
    height: 100px;
    margin: -50px 0 0 - 50px;
    animation - delay: 0.3s;
}

.ring: nth - child(3) {
    width: 120px;
    height: 120px;
    margin: -60px 0 0 - 60px;
    animation - delay: 0.6s;
}

/* Define la animación para que los círculos se expandan y se desvanezcan */
@keyframes ring - expand {
    0 % {
        transform: scale(0.8);
        opacity: 0.8;
    }
    100 % {
        transform: scale(1.2);
        opacity: 0;
    }
}

/* Título de la zona de subida */
.upload - title {
    font - size: 1.5rem;
    font - weight: 600;
    color: var(--text - primary);
    margin - bottom: var(--space - xs);
}

/* Subtítulo de la zona de subida */
.upload - subtitle {
    color: var(--text - secondary);
    margin - bottom: var(--space - md);
}

/* Contenedor para las etiquetas de los formatos soportados */
.supported - formats {
    display: flex;
    gap: var(--space - xs);
    justify - content: center;
}

/* Estilo para cada etiqueta de formato de archivo */
.format - tag {
    background: rgba(255, 255, 255, 0.1);
    color: var(--accent - cyan);
    padding: 4px 12px;
    border - radius: 15px;
    font - size: 0.8rem;
    font - weight: 500;
    border: 1px solid rgba(0, 217, 255, 0.3);
    transition: all var(--transition - fast);
}

/* Efecto al pasar el mouse sobre la etiqueta */
.format - tag:hover {
    background: rgba(0, 217, 255, 0.2);
    transform: scale(1.1);
}

/* Esconde el input de tipo archivo para que se pueda usar la zona de arrastrar */
.file - input {
    display: none;
}

/* ==========================================================================
   LISTA DE ARCHIVOS
   ========================================================================== */

/* Contenedor para la lista de archivos, con scroll y un tamaño máximo */
.file - list {
    margin: var(--space - lg) 0;
    max - height: 200px;
    overflow - y: auto;
    /* Personaliza el scrollbar para Firefox */
    scrollbar - width: thin;
    scrollbar - color: var(--accent - cyan) transparent;
}

/* Personaliza el scrollbar para Chrome y otros navegadores basados en WebKit */
.file - list:: -webkit - scrollbar {
    width: 6px;
}

.file - list:: -webkit - scrollbar - track {
    background: transparent;
}

.file - list:: -webkit - scrollbar - thumb {
    background: var(--accent - cyan);
    border - radius: 3px;
}

/* Estilo para cada archivo en la lista */
.file - item {
    display: flex;
    align - items: center;
    gap: var(--space - sm);
    padding: var(--space - sm);
    background: rgba(255, 255, 255, 0.05);
    border - radius: var(--border - radius - sm);
    margin - bottom: var(--space - xs);
    transition: all var(--transition - fast);
    border - left: 3px solid var(--accent - cyan);
}

/* Se mueve un poco hacia la derecha al pasar el mouse */
.file - item:hover {
    background: rgba(255, 255, 255, 0.08);
    transform: translateX(5px);
}

/* Estilo para el icono del archivo */
.file - icon {
    font - size: 1.2rem;
}

/* Contenedor de la información del archivo */
.file - info {
    flex: 1;
}

/* Nombre del archivo */
.file - name {
    font - weight: 500;
    color: var(--text - primary);
    font - size: 0.9rem;
}

/* Tamaño del archivo */
.file - size {
    font - size: 0.8rem;
    color: var(--text - tertiary);
}

/* Botón para eliminar un archivo */
.file - remove {
    background: none;
    border: none;
    color: var(--accent - orange);
    cursor: pointer;
    padding: 4px;
    border - radius: 4px;
    transition: all var(--transition - fast);
}

/* Efecto al pasar el mouse sobre el botón de eliminar */
.file - remove:hover {
    background: rgba(255, 107, 53, 0.2);
    transform: scale(1.1);
}

/* ==========================================================================
   BOTONES DE ACCIÓN
   ========================================================================== */

/* Contenedor para los botones de "procesar" y "borrar" */
.action - buttons {
    display: flex;
    gap: var(--space - sm);
    justify - content: center;
    margin: var(--space - lg) 0;
}

/* Estilo base para todos los botones "neo-morphism" */
.neo - btn {
    position: relative;
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid rgba(255, 255, 255, 0.1);
    color: var(--text - primary);
    padding: var(--space - sm) var(--space - lg);
    border - radius: var(--border - radius - xl);
    cursor: pointer;
    font - size: 1rem;
    font - weight: 600;
    text - transform: uppercase;
    letter - spacing: 1px;
    overflow: hidden;
    transition: all var(--transition - normal);
    backdrop - filter: blur(10px);
    min - width: 160px;
}

/* El brillo que se desliza por el botón al pasar el mouse */
.neo - btn::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100 %;
    height: 100 %;
    background: linear - gradient(45deg, transparent, rgba(255, 255, 255, 0.1), transparent);
    transform: translateX(-100 %);
    transition: transform 0.6s ease;
}

/* El brillo se mueve a la derecha al pasar el mouse */
.neo - btn: hover::before {
    transform: translateX(100 %);
}

/* El botón se levanta y la sombra cambia al pasar el mouse */
.neo - btn:hover {
    transform: translateY(-3px);
    box - shadow: 0 15px 35px rgba(0, 0, 0, 0.3);
    border - color: rgba(255, 255, 255, 0.3);
}

/* El botón regresa a su posición al hacer clic */
.neo - btn:active {
    transform: translateY(0);
}

/* Estilo para el botón principal ("primary") */
.neo - btn.primary {
    border - color: var(--accent - cyan);
    box - shadow: 0 0 20px rgba(0, 217, 255, 0.2);
}

/* Efectos para el botón principal al pasar el mouse */
.neo - btn.primary:hover {
    background: rgba(0, 217, 255, 0.1);
    box - shadow: 0 15px 35px rgba(0, 217, 255, 0.3);
}

/* Estilo para el botón secundario */
.neo - btn.secondary {
    border - color: var(--accent - purple);
    box - shadow: 0 0 20px rgba(123, 104, 238, 0.2);
}

/* Efectos para el botón secundario al pasar el mouse */
.neo - btn.secondary:hover {
    background: rgba(123, 104, 238, 0.1);
    box - shadow: 0 15px 35px rgba(123, 104, 238, 0.3);
}

/* Estilo para los botones deshabilitados */
.neo - btn:disabled {
    opacity: 0.5;
    cursor: not - allowed;
    transform: none!important;
}

/* Contenedor del texto y el icono del botón */
.btn - content {
    display: flex;
    align - items: center;
    gap: var(--space - xs);
    position: relative;
    z - index: 2;
}

/* Estilo para el icono del botón */
.btn - icon {
    font - size: 1.1rem;
}

/* Brillo interior del botón (no se usa en este caso, pero ahí está) */
.btn - shine {
    position: absolute;
    top: 0;
    left: -100 %;
    width: 100 %;
    height: 100 %;
    background: linear - gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
    transition: left 0.5s ease;
    z - index: 1;
}

/* ==========================================================================
   SECCIÓN DE PROGRESO
   ========================================================================== */

/* Contenedor de la barra de progreso, inicialmente oculto */
.progress - section {
    margin - top: var(--space - lg);
    opacity: 0;
    transform: translateY(20px);
    transition: all var(--transition - normal);
}

/* Estilo para cuando la sección se hace visible */
.progress - section.visible {
    opacity: 1;
    transform: translateY(0);
}

/* Contenedor de la barra de progreso */
.progress - wrapper {
    position: relative;
}

/* La barra de progreso, la parte de fondo */
.progress - bar {
    width: 100 %;
    height: 8px;
    background: rgba(255, 255, 255, 0.1);
    border - radius: 4px;
    overflow: hidden;
    position: relative;
    margin - bottom: var(--space - sm);
}

/* La barra de progreso que se llena. Su ancho se cambia con JavaScript */
.progress - fill {
    height: 100 %;
    background: linear - gradient(90deg, var(--accent - cyan), var(--accent - green));
    width: 0 %;
    transition: width var(--transition - normal);
    position: relative;
    border - radius: 4px;
}

/* Las "rayas" animadas dentro de la barra que le dan un efecto de movimiento */
.progress - fill::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    bottom: 0;
    right: 0;
    background - image: linear - gradient(
        -45deg,
        rgba(255, 255, 255, .2) 25 %,
        transparent 25 %,
        transparent 50 %,
        rgba(255, 255, 255, .2) 50 %,
        rgba(255, 255, 255, .2) 75 %,
        transparent 75 %,
        transparent
    );
    background - size: 30px 30px;
    animation: progress - stripes 1s linear infinite;
}

/* Define la animación para que las "rayas" se muevan */
@keyframes progress - stripes {
    from { background - position: 0 0; }
    to { background - position: 30px 30px; }
}

/* Un brillo detrás de la barra de progreso para un efecto más moderno */
.progress - glow {
    position: absolute;
    top: -2px;
    left: 0;
    right: 0;
    bottom: -2px;
    background: linear - gradient(90deg, var(--accent - cyan), var(--accent - green));
    border - radius: 6px;
    opacity: 0.3;
    filter: blur(8px);
    z - index: -1;
}

/* Texto que indica el progreso */
.progress - text {
    text - align: center;
    font - size: 0.9rem;
    color: var(--text - secondary);
    font - weight: 500;
}

/* ==========================================================================
   SECCIÓN DE MÉTRICAS
   ========================================================================== */

/* Contenedor que usa CSS Grid para organizar las tarjetas de métricas */
.metrics - grid {
    display: grid;
    grid - template - columns: repeat(auto - fit, minmax(160px, 1fr));
    gap: var(--space - sm);
    margin - bottom: var(--space - lg);
}

/* Estilo para cada tarjeta de métrica */
.metric - card {
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border - radius: var(--border - radius - md);
    padding: var(--space - md);
    text - align: center;
    transition: all var(--transition - normal);
    position: relative;
    overflow: hidden;
}

/* Línea superior que aparece al pasar el mouse */
.metric - card::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 100 %;
    height: 2px;
    background: var(--accent - cyan);
    transform: scaleX(0);
    transition: transform var(--transition - normal);
}

/* Efecto al pasar el mouse sobre la tarjeta */
.metric - card:hover {
    transform: translateY(-5px) scale(1.05);
    background: rgba(255, 255, 255, 0.08);
    border - color: rgba(255, 255, 255, 0.2);
}

/* La línea superior se expande al pasar el mouse */
.metric - card: hover::before {
    transform: scaleX(1);
}

/* Estilo especial para una tarjeta "resaltada" */
.metric - card.highlight {
    border - color: var(--accent - green);
    background: rgba(0, 255, 136, 0.05);
}

/* La línea superior se pone verde si la tarjeta está resaltada */
.metric - card.highlight::before {
    background: var(--accent - green);
    transform: scaleX(1);
}

/* Estilo para el icono de la métrica */
.metric - icon {
    font - size: 2rem;
    margin - bottom: var(--space - xs);
    opacity: 0.8;
}

/* Contenedor del contenido de la métrica */
.metric - content {
    display: flex;
    flex - direction: column;
    gap: var(--space - xs);
}

/* La etiqueta (título) de la métrica */
.metric - label {
    font - size: 0.8rem;
    color: var(--text - tertiary);
    text - transform: uppercase;
    letter - spacing: 1px;
    font - weight: 500;
}

/* El valor numérico de la métrica */
.metric - value {
    font - size: 1.5rem;
    font - weight: 700;
    color: var(--accent - cyan);
    line - height: 1;
    transition: all var(--transition - fast);
}

/* El valor de la métrica se agranda al pasar el mouse */
.metric - card: hover.metric - value {
    transform: scale(1.1);
}

/* El color del valor de la métrica cambia si la tarjeta está resaltada */
.metric - card.highlight.metric - value {
    color: var(--accent - green);
}

/* Estilo para la barra de estadísticas de resumen */
.summary - stats {
    display: flex;
    align - items: center;
    justify - content: center;
    gap: var(--space - lg);
    padding: var(--space - md);
    background: rgba(255, 255, 255, 0.02);
    border - radius: var(--border - radius - md);
    border: 1px solid rgba(255, 255, 255, 0.05);
}

/* Cada ítem dentro de la barra de resumen */
.stat - item {
    text - align: center;
}

/* El número grande en la barra de resumen */
.stat - number {
    display: block;
    font - size: 1.8rem;
    font - weight: 800;
    color: var(--accent - purple);
    line - height: 1;
    margin - bottom: 4px;
}

/* La etiqueta del número en la barra de resumen */
.stat - label {
    font - size: 0.8rem;
    color: var(--text - tertiary);
    text - transform: uppercase;
    letter - spacing: 1px;
}

/* La línea vertical que separa los números */
.stat - divider {
    width: 1px;
    height: 40px;
    background: rgba(255, 255, 255, 0.1);
}

/* ==========================================================================
   SECCIÓN DE PREDICCIÓN DE TEXTO
   ========================================================================== */

/* Contenedor del área de texto */
.text - input - wrapper {
    position: relative;
    margin - bottom: var(--space - lg);
}

/* El área de texto donde se escribe la predicción */
.text - input {
    width: 100 %;
    background: rgba(255, 255, 255, 0.05);
    border: 2px solid rgba(255, 255, 255, 0.1);
    border - radius: var(--border - radius - md);
    padding: var(--space - md);
    color: var(--text - primary);
    font - size: 1.1rem;
    line - height: 1.6;
    resize: vertical;
    min - height: 120px;
    font - family: inherit;
    transition: all var(--transition - normal);
    backdrop - filter: blur(5px);
}

/* El borde y la sombra cambian al hacer clic en el área de texto */
.text - input:focus {
    outline: none;
    border - color: var(--accent - cyan);
    background: rgba(255, 255, 255, 0.08);
    box - shadow: 0 0 0 3px rgba(0, 217, 255, 0.1);
}

/* Estilo para el texto de ejemplo ("placeholder") */
.text - input::placeholder {
    color: var(--text - tertiary);
    opacity: 0.8;
}

/* El brillo que aparece detrás del área de texto al hacer clic */
.input - glow {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    border - radius: var(--border - radius - md);
    background: linear - gradient(45deg, var(--accent - cyan), var(--accent - purple));
    opacity: 0;
    filter: blur(20px);
    z - index: -1;
    transition: opacity var(--transition - normal);
}

/* Muestra el brillo al enfocar el área de texto */
.text - input: focus + .input - glow {
    opacity: 0.2;
}

/* Contenedor de las sugerencias, inicialmente invisible */
.suggestions - container {
    opacity: 0;
    transform: translateY(20px);
    transition: all var(--transition - normal);
}

/* El contenedor se hace visible con una animación al tener sugerencias */
.suggestions - container.visible {
    opacity: 1;
    transform: translateY(0);
}

/* Encabezado de la sección de sugerencias */
.suggestions - header {
    display: flex;
    align - items: center;
    justify - content: space - between;
    margin - bottom: var(--space - md);
}

/* Título de la sección de sugerencias */
.suggestions - title {
    font - size: 1.1rem;
    font - weight: 600;
    color: var(--text - secondary);
}

/* El efecto de "escribiendo..." con puntos que rebotan */
.typing - indicator {
    display: flex;
    gap: 4px;
    align - items: center;
}

/* Estilo para cada punto del indicador de escritura */
.typing - indicator.dot {
    width: 6px;
    height: 6px;
    background: var(--accent - cyan);
    border - radius: 50 %;
    animation: typing - bounce 1.4s ease -in -out infinite both;
}

/* Retrasos para que los puntos reboten en secuencia */
.typing - indicator.dot: nth - child(1) { animation - delay: -0.32s; }
.typing - indicator.dot: nth - child(2) { animation - delay: -0.16s; }

/* Define la animación para que los puntos reboten */
@keyframes typing - bounce {
    0 %, 80 %, 100 % { transform: scale(0.8); opacity: 0.5; }
    40 % { transform: scale(1.2); opacity: 1; }
}

/* Contenedor para los "chips" de sugerencia */
.suggestions - grid {
    display: flex;
    flex - wrap: wrap;
    gap: var(--space - xs);
    /* Animación para que aparezcan suavemente */
    animation: slide - up var(--transition - normal) ease - out;
}

/* Define la animación para que los elementos aparezcan deslizando hacia arriba */
@keyframes slide - up {
    from {
        opacity: 0;
        transform: translateY(10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

/* Estilo para cada "chip" de sugerencia */
.suggestion - chip {
    background: rgba(255, 255, 255, 0.05);
    border: 1px solid rgba(255, 255, 255, 0.1);
    color: var(--text - primary);
    padding: 8px 16px;
    border - radius: 20px;
    cursor: pointer;
    font - size: 0.9rem;
    font - weight: 500;
    transition: all var(--transition - fast);
    position: relative;
    overflow: hidden;
}

/* Brillo interior del chip que se mueve al pasar el mouse */
.suggestion - chip::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100 %;
    width: 100 %;
    height: 100 %;
    background: linear - gradient(90deg, transparent, rgba(255, 255, 255, 0.1), transparent);
    transition: left 0.5s ease;
}

/* Efectos al pasar el mouse sobre un chip */
.suggestion - chip:hover {
    background: rgba(0, 217, 255, 0.1);
    border - color: var(--accent - cyan);
    transform: translateY(-2px) scale(1.05);
    box - shadow: 0 8px 25px rgba(0, 217, 255, 0.2);
}

/* El brillo se desliza por el chip al pasar el mouse */
.suggestion - chip: hover::before {
    left: 100 %;
}

/* El chip se comprime al hacer clic */
.suggestion - chip:active {
    transform: scale(0.95);
}

/* ==========================================================================
   SECCIÓN DE FUENTES
   ========================================================================== */

/* Contenedor de la lista de fuentes, con scroll y tamaño máximo */
.sources - list {
    max - height: 400px;
    overflow - y: auto;
    /* Personaliza el scrollbar para Firefox */
    scrollbar - width: thin;
    scrollbar - color: var(--accent - purple) transparent;
}

/* Personaliza el scrollbar para Chrome y otros navegadores */
.sources - list:: -webkit - scrollbar {
    width: 6px;
}

.sources - list:: -webkit - scrollbar - track {
    background: transparent;
}

.sources - list:: -webkit - scrollbar - thumb {
    background: var(--accent - purple);
    border - radius: 3px;
}

/* Estilo para cada ítem de fuente */
.source - item {
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border - radius: var(--border - radius - sm);
    padding: var(--space - md);
    margin - bottom: var(--space - sm);
    transition: all var(--transition - fast);
    position: relative;
    overflow: hidden;
}

/* Línea lateral que se expande al pasar el mouse */
.source - item::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    width: 4px;
    height: 100 %;
    background: var(--accent - purple);
    transform: scaleY(0);
    transition: transform var(--transition - normal);
}

/* Efectos al pasar el mouse sobre un ítem de fuente */
.source - item:hover {
    background: rgba(255, 255, 255, 0.08);
    border - color: rgba(255, 255, 255, 0.2);
    transform: translateX(5px);
}

/* La línea lateral se expande al pasar el mouse */
.source - item: hover::before {
    transform: scaleY(1);
}

/* Encabezado del ítem de la fuente */
.source - header {
    display: flex;
    align - items: center;
    gap: var(--space - sm);
    margin - bottom: var(--space - xs);
}

/* Estilo del icono de la fuente */
.source - icon {
    font - size: 1.2rem;
    color: var(--accent - purple);
}

/* Nombre de la fuente */
.source - name {
    font - weight: 600;
    color: var(--text - primary);
    font - size: 0.9rem;
}

/* Estadísticas de la fuente (ej. tiempo de lectura) */
.source - stats {
    display: flex;
    gap: var(--space - md);
    font - size: 0.8rem;
    color: var(--text - tertiary);
}

/* Cada estadística individual */
.source - stat {
    display: flex;
    align - items: center;
    gap: 4px;
}

/* Estilo para el estado "vacío", cuando no hay fuentes */
.empty - state {
    text - align: center;
    padding: var(--space - 2xl) var(--space - md);
    color: var(--text - tertiary);
}

/* Icono para el estado vacío, con animación de flotar */
.empty - icon {
    font - size: 3rem;
    margin - bottom: var(--space - md);
    opacity: 0.5;
    animation: float 3s ease -in -out infinite;
}

/* ==========================================================================
   DISEÑO RESPONSIVO
   ========================================================================== */

/* Estilos para pantallas de 1200px o menos */
@media(max - width: 1200px) {
    /* Cambia el layout de la cuadrícula a 8 columnas */
    .main - grid {
        grid - template - columns: repeat(8, 1fr);
    }

    /* Las secciones de carga y métricas ocupan la mitad de la pantalla */
    .upload - section,
    .metrics - section {
        grid - column: span 4;
    }

    /* La sección de predicción es más ancha */
    .prediction - section {
        grid - column: span 6;
    }

    /* La sección de fuentes es más estrecha */
    .sources - section {
        grid - column: span 2;
    }
}

/* Estilos para pantallas de 900px o menos (tablets) */
@media(max - width: 900px) {
    /* Cambia el layout a 4 columnas */
    .main - grid {
        grid - template - columns: repeat(4, 1fr);
    }

    /* Todas las secciones ocupan todo el ancho */
    .upload - section,
    .metrics - section,
    .prediction - section,
    .sources - section {
        grid - column: span 4;
    }

    /* Las tarjetas de métricas se reorganizan en 3 columnas */
    .metrics - grid {
        grid - template - columns: repeat(3, 1fr);
    }
}

/* Estilos para pantallas de 600px o menos (móviles) */
@media(max - width: 600px) {
    /* Menos relleno en los bordes */
    .content - wrapper {
        padding: var(--space - md);
    }

    /* Menos espacio entre las secciones */
    .main - grid {
        gap: var(--space - md);
    }

    /* El título es más pequeño */
    .system - title {
        font - size: 2.5rem;
    }

    /* Las tarjetas de métricas se reorganizan en 2 columnas */
    .metrics - grid {
        grid - template - columns: repeat(2, 1fr);
    }

    /* Los botones se apilan uno encima del otro */
    .action - buttons {
        flex - direction: column;
        align - items: center;
    }

    /* Los botones ocupan todo el ancho, con un límite */
    .neo - btn {
        width: 100 %;
        max - width: 200px;
    }

    /* Las estadísticas de resumen se apilan una encima de otra */
    .summary - stats {
        flex - direction: column;
        gap: var(--space - md);
    }

    /* El divisor entre las estadísticas se vuelve horizontal */
    .stat - divider {
        width: 100 %;
        height: 1px;
    }
}

/* ==========================================================================
   CLASES DE UTILIDAD Y ANIMACIONES
   ========================================================================== */

/* Animación para hacer aparecer elementos suavemente */
.fade -in {
    animation: fade -in 0.6s ease- out;
}

@keyframes fade -in {
    from {
    opacity: 0;
    transform: translateY(20px);
}
    to {
    opacity: 1;
    transform: translateY(0);
}
}

/* Animación para hacer aparecer elementos agrandándolos */
.scale -in {
    animation: scale -in 0.4s cubic- bezier(0.175, 0.885, 0.32, 1.275);
}

@keyframes scale -in {
    from {
    opacity: 0;
    transform: scale(0.8);
}
    to {
    opacity: 1;
    transform: scale(1);
}
}

/* Animación para hacer aparecer elementos desde la derecha */
.slide - left {
    animation: slide - left 0.5s ease - out;
}

@keyframes slide - left {
    from {
        opacity: 0;
        transform: translateX(30px);
    }
    to {
        opacity: 1;
        transform: translateX(0);
    }
}

/* Clase para esconder visualmente elementos, pero mantenerlos accesibles para lectores de pantalla */
.sr - only {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white - space: nowrap;
    border: 0;
}