/* ==========================================================================
   CUSTOM PROPERTIES & RESET
   ========================================================================== */

:root {
    /* Color Palette */
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

    /* Spacing & Sizing */
    --space - xs: 0.5rem;
    --space - sm: 1rem;
    --space - md: 1.5rem;
    --space - lg: 2rem;
    --space - xl: 3rem;
    --space - 2xl: 4rem;

    --border - radius - sm: 8px;
    --border - radius - md: 12px;
    --border - radius - lg: 20px;
    --border - radius - xl: 24px;

    /* Animations */
    --transition - fast: 0.2s cubic - bezier(0.4, 0, 0.2, 1);
    --transition - normal: 0.3s cubic - bezier(0.4, 0, 0.2, 1);
    --transition - slow: 0.6s cubic - bezier(0.4, 0, 0.2, 1);
    --transition - elastic: 0.8s cubic - bezier(0.68, -0.55, 0.265, 1.55);
}

/* Reset and Base Styles */
* {
    box- sizing: border - box;
margin: 0;
padding: 0;
}

html {
    font - size: clamp(14px, 1.5vw, 18px);
    scroll - behavior: smooth;
}

body {
    font - family: 'Inter', 'Segoe UI', Tahoma, Geneva, Verdana, sans - serif;
    line - height: 1.6;
    color: var(--text - primary);
    background: var(--primary - 900);
    overflow - x: hidden;
    min - height: 100vh;
    position: relative;
}

/* Hide default navbar for this page */
header, footer {
    display: none;
}

/* ==========================================================================
   HERO SECTION & BACKGROUND
   ========================================================================== */

.hero - section {
    min - height: 100vh;
    position: relative;
    background:
    radial - gradient(circle at 20 % 80 %, var(--primary - 600) 0 %, transparent 50 %),
    radial - gradient(circle at 80 % 20 %, var(--primary - 500) 0 %, transparent 50 %),
    radial - gradient(circle at 40 % 40 %, var(--primary - 700) 0 %, transparent 50 %),
    var(--primary - 900);
    overflow: hidden;
}

.floating - particles {
    position: absolute;
    top: 0;
    left: 0;
    width: 100 %;
    height: 100 %;
    pointer - events: none;
    z - index: 1;
}

.floating - particles:: before,
.floating - particles::after {
    content: '';
    position: absolute;
    width: 200px;
    height: 200px;
    border - radius: 50 %;
    background: radial - gradient(circle, var(--accent - cyan) 0 %, transparent 70 %);
    opacity: 0.1;
    animation: float 20s ease -in -out infinite;
}

.floating - particles::before {
    top: 20 %;
    left: 10 %;
    animation - delay: 0s;
}

.floating - particles::after {
    bottom: 20 %;
    right: 10 %;
    animation - delay: -10s;
    background: radial - gradient(circle, var(--accent - purple) 0 %, transparent 70 %);
}

@keyframes float {
    0 %, 100 % { transform: translateY(0px) rotate(0deg); }
    33 % { transform: translateY(-30px) rotate(120deg); }
    66 % { transform: translateY(20px) rotate(240deg); }
}

/* ==========================================================================
   CONTENT WRAPPER & LAYOUT
   ========================================================================== */

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

.main - header {
    text - align: center;
    margin - bottom: var(--space - 2xl);
    padding: var(--space - lg) 0;
}

.system - title {
    font - size: clamp(2.5rem, 6vw, 4.5rem);
    font - weight: 800;
    line - height: 1.2;
    margin - bottom: var(--space - md);
    position: relative;
}

.title - part {
    display: inline - block;
    position: relative;
    margin: 0 0.2em;
    color: var(--text - primary);
    transition: all var(--transition - normal);
    cursor: default ;
}

.title - part::before {
    content: attr(data - text);
    position: absolute;
    top: 0;
    left: 0;
    width: 100 %;
    height: 100 %;
    background: linear - gradient(45deg, var(--accent - cyan), var(--accent - purple));
    -webkit - background - clip: text;
    -webkit - text - fill - color: transparent;
    background - clip: text;
    opacity: 0;
    transition: opacity var(--transition - normal);
}

.title - part: hover:: before,
.title - part.highlight::before {
    opacity: 1;
}

.title - part.highlight {
    animation: glow - pulse 2s ease -in -out infinite;
}

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

.team - credits {
    margin - top: var(--space - md);
    opacity: 0.8;
}

.credit - item {
    font - size: 1.1rem;
    font - weight: 500;
    letter - spacing: 2px;
    color: var(--text - secondary);
    position: relative;
    display: inline - block;
}

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

.credit - item: hover::after {
    width: 100 %;
}

/* ==========================================================================
   MAIN GRID LAYOUT
   ========================================================================== */

.main - grid {
    display: grid;
    grid - template - columns: repeat(12, 1fr);
    gap: var(--space - lg);
    flex: 1;
}

.upload - section {
    grid - column: span 6;
    grid - row: span 1;
}

.metrics - section {
    grid - column: span 6;
    grid - row: span 1;
}

.prediction - section {
    grid - column: span 8;
    grid - row: span 1;
}

.sources - section {
    grid - column: span 4;
    grid - row: span 1;
}

/* ==========================================================================
   GLASS MORPHISM CARDS
   ========================================================================== */

.glass - card {
    background: var(--glass - bg);
    backdrop - filter: blur(20px);
    -webkit - backdrop - filter: blur(20px);
    border: 1px solid var(--glass - border);
    border - radius: var(--border - radius - lg);
    box - shadow:
    0 8px 32px var(--glass - shadow),
        inset 0 1px 0 rgba(255, 255, 255, 0.1);
    position: relative;
    overflow: hidden;
    transition: all var(--transition - normal);
}

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

.glass - card:hover {
    transform: translateY(-5px);
    border - color: rgba(255, 255, 255, 0.2);
    box - shadow:
    0 20px 40px rgba(0, 0, 0, 0.4),
        inset 0 1px 0 rgba(255, 255, 255, 0.2);
}

.glass - card: hover::before {
    left: 100 %;
}

.card - header {
    padding: var(--space - lg) var(--space - lg) 0;
    border - bottom: 1px solid rgba(255, 255, 255, 0.05);
    margin - bottom: var(--space - lg);
}

.card - title {
    font - size: 1.5rem;
    font - weight: 700;
    color: var(--text - primary);
    display: flex;
    align - items: center;
    gap: var(--space - sm);
    margin: 0;
}

.icon - wrapper {
    font - size: 1.8rem;
    line - height: 1;
    filter: drop - shadow(0 0 10px currentColor);
    animation: icon - float 3s ease -in -out infinite;
}

@keyframes icon - float {
    0 %, 100 % { transform: translateY(0px); }
    50 % { transform: translateY(-3px); }
}

.card - body {
    padding: 0 var(--space - lg) var(--space - lg);
}

/* ==========================================================================
   UPLOAD SECTION
   ========================================================================== */

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

.upload - zone:hover {
    border - color: var(--accent - cyan);
    background: rgba(0, 217, 255, 0.05);
    transform: scale(1.02);
}

.upload - zone.dragover {
    border - color: var(--accent - green);
    background: rgba(0, 255, 136, 0.1);
    animation: pulse - glow 1s ease -in -out infinite;
}

@keyframes pulse - glow {
    0 %, 100 % { box- shadow: 0 0 0 0 rgba(0, 255, 136, 0.4);
}
50 % { box- shadow: 0 0 0 20px rgba(0, 255, 136, 0); }
}

.upload - visual {
    position: relative;
    display: inline - block;
    margin - bottom: var(--space - md);
}

.upload - icon {
    font - size: 4rem;
    line - height: 1;
    position: relative;
    z - index: 2;
    animation: upload - bounce 2s ease -in -out infinite;
}

@keyframes upload - bounce {
    0 %, 20 %, 50 %, 80 %, 100 % { transform: translateY(0); }
    40 % { transform: translateY(-10px); }
    60 % { transform: translateY(-5px); }
}

.upload - rings {
    position: absolute;
    top: 50 %;
    left: 50 %;
    transform: translate(-50 %, -50 %);
    z - index: 1;
}

.ring {
    position: absolute;
    border: 2px solid var(--accent - cyan);
    border - radius: 50 %;
    opacity: 0.3;
    animation: ring - expand 2s ease - out infinite;
}

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

.upload - title {
    font - size: 1.5rem;
    font - weight: 600;
    color: var(--text - primary);
    margin - bottom: var(--space - xs);
}

.upload - subtitle {
    color: var(--text - secondary);
    margin - bottom: var(--space - md);
}

.supported - formats {
    display: flex;
    gap: var(--space - xs);
    justify - content: center;
}

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

.format - tag:hover {
    background: rgba(0, 217, 255, 0.2);
    transform: scale(1.1);
}

.file - input {
    display: none;
}

/* ==========================================================================
   FILE LIST
   ========================================================================== */

.file - list {
    margin: var(--space - lg) 0;
    max - height: 200px;
    overflow - y: auto;
    scrollbar - width: thin;
    scrollbar - color: var(--accent - cyan) transparent;
}

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

.file - item:hover {
    background: rgba(255, 255, 255, 0.08);
    transform: translateX(5px);
}

.file - icon {
    font - size: 1.2rem;
}

.file - info {
    flex: 1;
}

.file - name {
    font - weight: 500;
    color: var(--text - primary);
    font - size: 0.9rem;
}

.file - size {
    font - size: 0.8rem;
    color: var(--text - tertiary);
}

.file - remove {
    background: none;
    border: none;
    color: var(--accent - orange);
    cursor: pointer;
    padding: 4px;
    border - radius: 4px;
    transition: all var(--transition - fast);
}

.file - remove:hover {
    background: rgba(255, 107, 53, 0.2);
    transform: scale(1.1);
}

/* ==========================================================================
   ACTION BUTTONS
   ========================================================================== */

.action - buttons {
    display: flex;
    gap: var(--space - sm);
    justify - content: center;
    margin: var(--space - lg) 0;
}

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

.neo - btn: hover::before {
    transform: translateX(100 %);
}

.neo - btn:hover {
    transform: translateY(-3px);
    box - shadow: 0 15px 35px rgba(0, 0, 0, 0.3);
    border - color: rgba(255, 255, 255, 0.3);
}

.neo - btn:active {
    transform: translateY(0);
}

.neo - btn.primary {
    border - color: var(--accent - cyan);
    box - shadow: 0 0 20px rgba(0, 217, 255, 0.2);
}

.neo - btn.primary:hover {
    background: rgba(0, 217, 255, 0.1);
    box - shadow: 0 15px 35px rgba(0, 217, 255, 0.3);
}

.neo - btn.secondary {
    border - color: var(--accent - purple);
    box - shadow: 0 0 20px rgba(123, 104, 238, 0.2);
}

.neo - btn.secondary:hover {
    background: rgba(123, 104, 238, 0.1);
    box - shadow: 0 15px 35px rgba(123, 104, 238, 0.3);
}

.neo - btn:disabled {
    opacity: 0.5;
    cursor: not - allowed;
    transform: none!important;
}

.btn - content {
    display: flex;
    align - items: center;
    gap: var(--space - xs);
    position: relative;
    z - index: 2;
}

.btn - icon {
    font - size: 1.1rem;
}

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
   PROGRESS SECTION
   ========================================================================== */

.progress - section {
    margin - top: var(--space - lg);
    opacity: 0;
    transform: translateY(20px);
    transition: all var(--transition - normal);
}

.progress - section.visible {
    opacity: 1;
    transform: translateY(0);
}

.progress - wrapper {
    position: relative;
}

.progress - bar {
    width: 100 %;
    height: 8px;
    background: rgba(255, 255, 255, 0.1);
    border - radius: 4px;
    overflow: hidden;
    position: relative;
    margin - bottom: var(--space - sm);
}

.progress - fill {
    height: 100 %;
    background: linear - gradient(90deg, var(--accent - cyan), var(--accent - green));
    width: 0 %;
    transition: width var(--transition - normal);
    position: relative;
    border - radius: 4px;
}

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

@keyframes progress - stripes {
    from { background - position: 0 0; }
    to { background - position: 30px 30px; }
}

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

.progress - text {
    text - align: center;
    font - size: 0.9rem;
    color: var(--text - secondary);
    font - weight: 500;
}

/* ==========================================================================
   METRICS SECTION
   ========================================================================== */

.metrics - grid {
    display: grid;
    grid - template - columns: repeat(auto - fit, minmax(160px, 1fr));
    gap: var(--space - sm);
    margin - bottom: var(--space - lg);
}

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

.metric - card:hover {
    transform: translateY(-5px) scale(1.05);
    background: rgba(255, 255, 255, 0.08);
    border - color: rgba(255, 255, 255, 0.2);
}

.metric - card: hover::before {
    transform: scaleX(1);
}

.metric - card.highlight {
    border - color: var(--accent - green);
    background: rgba(0, 255, 136, 0.05);
}

.metric - card.highlight::before {
    background: var(--accent - green);
    transform: scaleX(1);
}

.metric - icon {
    font - size: 2rem;
    margin - bottom: var(--space - xs);
    opacity: 0.8;
}

.metric - content {
    display: flex;
    flex - direction: column;
    gap: var(--space - xs);
}

.metric - label {
    font - size: 0.8rem;
    color: var(--text - tertiary);
    text - transform: uppercase;
    letter - spacing: 1px;
    font - weight: 500;
}

.metric - value {
    font - size: 1.5rem;
    font - weight: 700;
    color: var(--accent - cyan);
    line - height: 1;
    transition: all var(--transition - fast);
}

.metric - card: hover.metric - value {
    transform: scale(1.1);
}

.metric - card.highlight.metric - value {
    color: var(--accent - green);
}

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

.stat - item {
    text - align: center;
}

.stat - number {
    display: block;
    font - size: 1.8rem;
    font - weight: 800;
    color: var(--accent - purple);
    line - height: 1;
    margin - bottom: 4px;
}

.stat - label {
    font - size: 0.8rem;
    color: var(--text - tertiary);
    text - transform: uppercase;
    letter - spacing: 1px;
}

.stat - divider {
    width: 1px;
    height: 40px;
    background: rgba(255, 255, 255, 0.1);
}

/* ==========================================================================
   TEXT PREDICTION SECTION
   ========================================================================== */

.text - input - wrapper {
    position: relative;
    margin - bottom: var(--space - lg);
}

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

.text - input:focus {
    outline: none;
    border - color: var(--accent - cyan);
    background: rgba(255, 255, 255, 0.08);
    box - shadow: 0 0 0 3px rgba(0, 217, 255, 0.1);
}

.text - input::placeholder {
    color: var(--text - tertiary);
    opacity: 0.8;
}

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

.text - input: focus + .input - glow {
    opacity: 0.2;
}

.suggestions - container {
    opacity: 0;
    transform: translateY(20px);
    transition: all var(--transition - normal);
}

.suggestions - container.visible {
    opacity: 1;
    transform: translateY(0);
}

.suggestions - header {
    display: flex;
    align - items: center;
    justify - content: space - between;
    margin - bottom: var(--space - md);
}

.suggestions - title {
    font - size: 1.1rem;
    font - weight: 600;
    color: var(--text - secondary);
}

.typing - indicator {
    display: flex;
    gap: 4px;
    align - items: center;
}

.typing - indicator.dot {
    width: 6px;
    height: 6px;
    background: var(--accent - cyan);
    border - radius: 50 %;
    animation: typing - bounce 1.4s ease -in -out infinite both;
}

.typing - indicator.dot: nth - child(1) { animation - delay: -0.32s; }
.typing - indicator.dot: nth - child(2) { animation - delay: -0.16s; }

@keyframes typing - bounce {
    0 %, 80 %, 100 % { transform: scale(0.8); opacity: 0.5; }
    40 % { transform: scale(1.2); opacity: 1; }
}

.suggestions - grid {
    display: flex;
    flex - wrap: wrap;
    gap: var(--space - xs);
    animation: slide - up var(--transition - normal) ease - out;
}

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

.suggestion - chip:hover {
    background: rgba(0, 217, 255, 0.1);
    border - color: var(--accent - cyan);
    transform: translateY(-2px) scale(1.05);
    box - shadow: 0 8px 25px rgba(0, 217, 255, 0.2);
}

.suggestion - chip: hover::before {
    left: 100 %;
}

.suggestion - chip:active {
    transform: scale(0.95);
}

/* ==========================================================================
   SOURCES SECTION
   ========================================================================== */

.sources - list {
    max - height: 400px;
    overflow - y: auto;
    scrollbar - width: thin;
    scrollbar - color: var(--accent - purple) transparent;
}

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

.source - item:hover {
    background: rgba(255, 255, 255, 0.08);
    border - color: rgba(255, 255, 255, 0.2);
    transform: translateX(5px);
}

.source - item: hover::before {
    transform: scaleY(1);
}

.source - header {
    display: flex;
    align - items: center;
    gap: var(--space - sm);
    margin - bottom: var(--space - xs);
}

.source - icon {
    font - size: 1.2rem;
    color: var(--accent - purple);
}

.source - name {
    font - weight: 600;
    color: var(--text - primary);
    font - size: 0.9rem;
}

.source - stats {
    display: flex;
    gap: var(--space - md);
    font - size: 0.8rem;
    color: var(--text - tertiary);
}

.source - stat {
    display: flex;
    align - items: center;
    gap: 4px;
}

.empty - state {
    text - align: center;
    padding: var(--space - 2xl) var(--space - md);
    color: var(--text - tertiary);
}

.empty - icon {
    font - size: 3rem;
    margin - bottom: var(--space - md);
    opacity: 0.5;
    animation: float 3s ease -in -out infinite;
}

/* ==========================================================================
   RESPONSIVE DESIGN
   ========================================================================== */

@media(max - width: 1200px) {
    .main - grid {
        grid - template - columns: repeat(8, 1fr);
    }
    
    .upload - section,
    .metrics - section {
        grid - column: span 4;
    }
    
    .prediction - section {
        grid - column: span 6;
    }
    
    .sources - section {
        grid - column: span 2;
    }
}

@media(max - width: 900px) {
    .main - grid {
        grid - template - columns: repeat(4, 1fr);
    }
    
    .upload - section,
    .metrics - section,
    .prediction - section,
    .sources - section {
        grid - column: span 4;
    }
    
    .metrics - grid {
        grid - template - columns: repeat(3, 1fr);
    }
}

@media(max - width: 600px) {
    .content - wrapper {
        padding: var(--space - md);
    }
    
    .main - grid {
        gap: var(--space - md);
    }
    
    .system - title {
        font - size: 2.5rem;
    }
    
    .metrics - grid {
        grid - template - columns: repeat(2, 1fr);
    }
    
    .action - buttons {
        flex - direction: column;
        align - items: center;
    }
    
    .neo - btn {
        width: 100 %;
        max - width: 200px;
    }
    
    .summary - stats {
        flex - direction: column;
        gap: var(--space - md);
    }
    
    .stat - divider {
        width: 100 %;
        height: 1px;
    }
}

/* ==========================================================================
   UTILITY CLASSES & ANIMATIONS
   ========================================================================== */

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

/* Hidden but accessible */
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