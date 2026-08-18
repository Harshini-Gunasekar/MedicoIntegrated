/* Medico Quantum-Cyber Live Background 60FPS Dynamic Engine */
(function () {
    let canvas, ctx;
    let particles = [];
    let waves = [];
    let pulses = [];
    let animationFrameId;
    let width, height;
    let mouse = { x: null, y: null, radius: 180, active: false };

    function initCanvas() {
        canvas = document.getElementById('medicoLiveCanvas');
        if (!canvas) return;

        ctx = canvas.getContext('2d');
        resize();

        window.removeEventListener('resize', resize);
        window.addEventListener('resize', resize);

        window.removeEventListener('mousemove', handleMouseMove);
        window.addEventListener('mousemove', handleMouseMove);

        window.removeEventListener('mouseleave', handleMouseLeave);
        window.addEventListener('mouseleave', handleMouseLeave);

        createParticles();
        createWaves();

        if (animationFrameId) cancelAnimationFrame(animationFrameId);
        animate();
    }

    function resize() {
        if (!canvas) return;
        width = canvas.width = window.innerWidth;
        height = canvas.height = window.innerHeight;
    }

    function handleMouseMove(e) {
        mouse.x = e.clientX;
        mouse.y = e.clientY;
        mouse.active = true;

        if (Math.random() < 0.25) {
            pulses.push({
                x: e.clientX,
                y: e.clientY,
                radius: 1,
                maxRadius: 60 + Math.random() * 40,
                alpha: 0.8,
                color: Math.random() > 0.5 ? '#06b6d4' : '#3b82f6'
            });
        }
    }

    function handleMouseLeave() {
        mouse.active = false;
    }

    function createParticles() {
        particles = [];
        const count = Math.min(Math.floor((width * height) / 12000), 90);
        for (let i = 0; i < count; i++) {
            particles.push({
                x: Math.random() * width,
                y: Math.random() * height,
                vx: (Math.random() - 0.5) * 0.9,
                vy: (Math.random() - 0.5) * 0.9,
                radius: Math.random() * 2.8 + 1.2,
                baseAlpha: Math.random() * 0.6 + 0.3,
                pulse: Math.random() * Math.PI * 2,
                pulseSpeed: 0.02 + Math.random() * 0.03,
                color: Math.random() > 0.4 ? 'rgba(6, 182, 212,' : (Math.random() > 0.5 ? 'rgba(59, 130, 246,' : 'rgba(16, 185, 129,')
            });
        }
    }

    function createWaves() {
        waves = [
            { amplitude: 45, wavelength: 0.008, speed: 0.025, y: 0.7, color: 'rgba(6, 182, 212, 0.15)', stroke: 'rgba(6, 182, 212, 0.35)' },
            { amplitude: 60, wavelength: 0.005, speed: -0.018, y: 0.75, color: 'rgba(59, 130, 246, 0.12)', stroke: 'rgba(59, 130, 246, 0.3)' },
            { amplitude: 35, wavelength: 0.012, speed: 0.035, y: 0.8, color: 'rgba(16, 185, 129, 0.08)', stroke: 'rgba(16, 185, 129, 0.25)' }
        ];
    }

    function drawWaves(time) {
        waves.forEach(w => {
            ctx.beginPath();
            const baseY = height * w.y;
            ctx.moveTo(0, baseY);

            for (let x = 0; x <= width; x += 15) {
                const y = baseY + Math.sin(x * w.wavelength + time * w.speed) * w.amplitude;
                ctx.lineTo(x, y);
            }

            ctx.lineTo(width, height);
            ctx.lineTo(0, height);
            ctx.closePath();

            ctx.fillStyle = w.color;
            ctx.fill();

            ctx.strokeStyle = w.stroke;
            ctx.lineWidth = 1.8;
            ctx.stroke();
        });
    }

    function drawDNAHelix(time) {
        const centerX = width * 0.22;
        const amplitude = 95;
        const spacing = 28;
        const totalNodes = Math.floor(height / spacing) + 2;

        for (let i = 0; i < totalNodes; i++) {
            const y = (i * spacing + (time * 35)) % (height + 50) - 25;
            const phase = i * 0.28 + time * 1.8;

            const x1 = centerX + Math.sin(phase) * amplitude;
            const x2 = centerX + Math.sin(phase + Math.PI) * amplitude;

            const depth1 = (Math.cos(phase) + 1) / 2;
            const depth2 = (Math.cos(phase + Math.PI) + 1) / 2;

            if (i % 2 === 0) {
                const grad = ctx.createLinearGradient(x1, y, x2, y);
                grad.addColorStop(0, `rgba(6, 182, 212, ${0.15 + depth1 * 0.3})`);
                grad.addColorStop(0.5, 'rgba(59, 130, 246, 0.45)');
                grad.addColorStop(1, `rgba(16, 185, 129, ${0.15 + depth2 * 0.3})`);

                ctx.strokeStyle = grad;
                ctx.lineWidth = 1.5;
                ctx.beginPath();
                ctx.moveTo(x1, y);
                ctx.lineTo(x2, y);
                ctx.stroke();
            }

            // Strand Node 1
            ctx.fillStyle = `rgba(6, 182, 212, ${0.45 + depth1 * 0.5})`;
            ctx.shadowBlur = 8;
            ctx.shadowColor = '#06b6d4';
            ctx.beginPath();
            ctx.arc(x1, y, 2.8 + depth1 * 2.2, 0, Math.PI * 2);
            ctx.fill();

            // Strand Node 2
            ctx.fillStyle = `rgba(59, 130, 246, ${0.45 + depth2 * 0.5})`;
            ctx.shadowColor = '#3b82f6';
            ctx.beginPath();
            ctx.arc(x2, y, 2.8 + depth2 * 2.2, 0, Math.PI * 2);
            ctx.fill();
            ctx.shadowBlur = 0;
        }
    }

    function animate() {
        if (!canvas || !ctx) return;

        ctx.clearRect(0, 0, width, height);

        const time = Date.now() * 0.0012;

        // Render Quantum Bio Waves
        drawWaves(time);

        // Render DNA Helix Strand
        drawDNAHelix(time);

        // Render Mouse Energy Ripples
        for (let i = pulses.length - 1; i >= 0; i--) {
            let p = pulses[i];
            p.radius += 2.2;
            p.alpha -= 0.02;

            if (p.alpha <= 0 || p.radius >= p.maxRadius) {
                pulses.splice(i, 1);
                continue;
            }

            ctx.strokeStyle = p.color;
            ctx.globalAlpha = p.alpha;
            ctx.lineWidth = 1.5;
            ctx.beginPath();
            ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
            ctx.stroke();
            ctx.globalAlpha = 1;
        }

        // Render Connected Neural Constellation
        for (let i = 0; i < particles.length; i++) {
            let p = particles[i];

            p.x += p.vx;
            p.y += p.vy;

            if (p.x < 0 || p.x > width) p.vx *= -1;
            if (p.y < 0 || p.y > height) p.vy *= -1;

            p.pulse += p.pulseSpeed;
            const currentAlpha = p.baseAlpha + Math.sin(p.pulse) * 0.25;

            if (mouse.active && mouse.x !== null && mouse.y !== null) {
                let dx = mouse.x - p.x;
                let dy = mouse.y - p.y;
                let dist = Math.sqrt(dx * dx + dy * dy);
                if (dist < mouse.radius) {
                    let force = (mouse.radius - dist) / mouse.radius;
                    p.x -= (dx / dist) * force * 3;
                    p.y -= (dy / dist) * force * 3;
                }
            }

            ctx.fillStyle = `${p.color}${Math.max(0.15, currentAlpha)})`;
            ctx.shadowBlur = 12;
            ctx.shadowColor = '#06b6d4';
            ctx.beginPath();
            ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
            ctx.fill();
            ctx.shadowBlur = 0;

            for (let j = i + 1; j < particles.length; j++) {
                let p2 = particles[j];
                let dx = p.x - p2.x;
                let dy = p.y - p2.y;
                let dist = Math.sqrt(dx * dx + dy * dy);

                if (dist < 140) {
                    let lineAlpha = (1 - dist / 140) * 0.32;
                    ctx.strokeStyle = `rgba(6, 182, 212, ${lineAlpha})`;
                    ctx.lineWidth = 1;
                    ctx.beginPath();
                    ctx.moveTo(p.x, p.y);
                    ctx.lineTo(p2.x, p2.y);
                    ctx.stroke();
                }
            }
        }

        animationFrameId = requestAnimationFrame(animate);
    }

    window.initMedicoLiveBackground = initCanvas;

    if (document.readyState === 'complete' || document.readyState === 'interactive') {
        setTimeout(initCanvas, 100);
    } else {
        document.addEventListener('DOMContentLoaded', initCanvas);
    }

    setInterval(() => {
        const c = document.getElementById('medicoLiveCanvas');
        if (c && c !== canvas) {
            initCanvas();
        }
    }, 1500);
})();
