window.zlaInterop = {
    initSwipeDeck: function (dotNetHelper) {
        if (window._swipeDeckInitialized) return;
        window._swipeDeckInitialized = true;

        let currentIndex = 0;
        let startY = 0;
        let currentY = 0;
        let isDragging = false;
        
        const getStack = () => document.querySelector('.card-stack');
        
        const updateTransform = (offset = 0, animate = false) => {
            const stack = getStack();
            if (!stack) return;
            stack.style.transition = animate ? 'transform 0.3s ease-out' : 'none';
            stack.style.transform = `translateY(calc(-${currentIndex * 100}vh + ${offset}px))`;
        };

        const handleStart = (y) => {
            if ('speechSynthesis' in window) {
                window.speechSynthesis.cancel();
            }
            startY = y;
            isDragging = true;
            currentY = y;
            updateTransform(0, false);
        };
        
        const handleMove = (y) => {
            if (!isDragging) return;
            currentY = y;
            const deltaY = currentY - startY;
            updateTransform(deltaY, false);
        };
        
        const handleEnd = () => {
            if (!isDragging) return;
            isDragging = false;
            
            const stack = getStack();
            if (!stack) return;
            
            const deltaY = currentY - startY;
            const cardsCount = stack.children.length;
            
            if (deltaY < -60 && currentIndex < cardsCount - 1) {
                currentIndex++;
                dotNetHelper.invokeMethodAsync('OnCardSnapped', currentIndex);
            } else if (deltaY > 60 && currentIndex > 0) {
                currentIndex--;
                dotNetHelper.invokeMethodAsync('OnCardSnapped', currentIndex);
            }
            
            updateTransform(0, true);
        };
        
        window.addEventListener('touchstart', (e) => handleStart(e.touches[0].clientY), { passive: true });
        window.addEventListener('touchmove', (e) => handleMove(e.touches[0].clientY), { passive: true });
        window.addEventListener('touchend', handleEnd);
        
        window.addEventListener('mousedown', (e) => handleStart(e.clientY));
        window.addEventListener('mousemove', (e) => handleMove(e.clientY));
        window.addEventListener('mouseup', handleEnd);
        
        window.addEventListener('keydown', (e) => {
            const stack = getStack();
            if (!stack) return;
            
            const cardsCount = stack.children.length;
            let changed = false;
            
            if (e.key === 'ArrowDown' && currentIndex < cardsCount - 1) {
                currentIndex++;
                changed = true;
            } else if (e.key === 'ArrowUp' && currentIndex > 0) {
                currentIndex--;
                changed = true;
            }
            
            if (changed) {
                if ('speechSynthesis' in window) window.speechSynthesis.cancel();
                updateTransform(0, true);
                dotNetHelper.invokeMethodAsync('OnCardSnapped', currentIndex);
            }
        });
        
        // Initial setup
        updateTransform(0, false);
    },

    // Scene 5: Celebration
    triggerConfetti: function (elementId) {
        const el = document.getElementById(elementId);
        if (!el) return;

        const canvas = document.createElement('canvas');
        canvas.style.position = 'absolute';
        canvas.style.top = '0';
        canvas.style.left = '0';
        canvas.style.width = '100%';
        canvas.style.height = '100%';
        canvas.style.pointerEvents = 'none';
        canvas.style.zIndex = '9999';
        el.appendChild(canvas);

        const ctx = canvas.getContext('2d');
        canvas.width = el.clientWidth;
        canvas.height = el.clientHeight;

        const particles = [];
        for (let i = 0; i < 150; i++) {
            particles.push({
                x: canvas.width / 2,
                y: canvas.height / 2,
                vx: (Math.random() - 0.5) * 15,
                vy: (Math.random() - 0.5) * 15 - 5,
                size: Math.random() * 6 + 4,
                color: Math.random() > 0.5 ? '#00f0ff' : (Math.random() > 0.5 ? '#39ff14' : '#ff0055')
            });
        }

        let animationFrame;
        function render() {
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            particles.forEach(p => {
                p.x += p.vx;
                p.y += p.vy;
                p.vy += 0.3; // gravity
                ctx.fillStyle = p.color;
                ctx.fillRect(p.x, p.y, p.size, p.size);
            });
            animationFrame = requestAnimationFrame(render);
        }
        render();

        setTimeout(() => {
            cancelAnimationFrame(animationFrame);
            if (canvas.parentNode) {
                canvas.parentNode.removeChild(canvas);
            }
        }, 4000);
    },

    snapToNext: function() {
        if (!window._swipeDeckInitialized) return;
        const stack = document.querySelector('.card-stack');
        if (!stack) return;
        
        let evt = new KeyboardEvent('keydown', {'key': 'ArrowDown'});
        window.dispatchEvent(evt);
    },
    
    triggerDissolve: function(elementId) {
        const el = document.getElementById(elementId);
        if (el) {
            el.classList.add('dissolving');
        }
    },

    // Whimsical TTS Narrator
    speakText: function (text) {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.pitch = 1.35; // Playful Seussian pitch
            utterance.rate = 1.05;  // Rhythmic pace
            window.speechSynthesis.speak(utterance);
        } else {
            console.warn("Speech Synthesis not supported.");
        }
    }
};
