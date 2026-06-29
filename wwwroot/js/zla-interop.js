window.zlaInterop = {
    initSwipeDeck: function (dotNetHelper) {
        if (window._swipeDeckInitialized) return;
        window._swipeDeckInitialized = true;

        let currentIndex = 0;
        let startY = 0;
        let currentY = 0;
        let isDragging = false;
        
        // Initialize HTML5 Audio elements
        window.zlaAudio = {
            track1: new Audio('audio/sneeker_mc_snoot.mp4'),
            track2: new Audio('audio/snoots_last_confession.mp4'),
            currentTrack: null
        };
        
        window.zlaAudio.track1.loop = true;
        window.zlaAudio.track2.loop = true;

        const playTrack = (trackName) => {
            const track = window.zlaAudio[trackName];
            const otherTrack = trackName === 'track1' ? window.zlaAudio.track2 : window.zlaAudio.track1;
            
            if (window.zlaAudio.currentTrack !== track) {
                otherTrack.pause();
                otherTrack.currentTime = 0;
                
                track.play().catch(e => {
                    console.log("Audio play blocked by browser, waiting for user interaction.", e);
                });
                window.zlaAudio.currentTrack = track;
            }
        };

        const handleAudioForCard = (index) => {
            // Card 1-2 (0, 1): Play Sneeker Mc Snoot
            // Card 3-6 (2, 3, 4, 5): Play Snoot's Last Confession
            if (index === 0 || index === 1) {
                playTrack('track1');
            } else {
                playTrack('track2');
            }
        };

        // Start audio on first touch/click interaction to bypass browser autoplay blocks
        const initAudioOnInteraction = () => {
            handleAudioForCard(currentIndex);
            window.removeEventListener('click', initAudioOnInteraction);
            window.removeEventListener('touchstart', initAudioOnInteraction);
        };
        window.addEventListener('click', initAudioOnInteraction);
        window.addEventListener('touchstart', initAudioOnInteraction);

        const getStack = () => document.querySelector('.card-stack');
        
        const updateTransform = (offset = 0, animate = false) => {
            const stack = getStack();
            if (!stack) return;
            stack.style.transition = animate ? 'transform 0.3s ease-out' : 'none';
            stack.style.transform = `translateY(calc(-${currentIndex * 100}vh + ${offset}px))`;
        };

        const handleStart = (y) => {
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
                handleAudioForCard(currentIndex);
            } else if (deltaY > 60 && currentIndex > 0) {
                currentIndex--;
                dotNetHelper.invokeMethodAsync('OnCardSnapped', currentIndex);
                handleAudioForCard(currentIndex);
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
                updateTransform(0, true);
                dotNetHelper.invokeMethodAsync('OnCardSnapped', currentIndex);
                handleAudioForCard(currentIndex);
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

    // Legacy Whimsical TTS Narrator (No-op since we are using MP4 music files now)
    speakText: function (text) {
        console.log("TTS disabled in favor of background MP4 audio tracks.");
    }
};
