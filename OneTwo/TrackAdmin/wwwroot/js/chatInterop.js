window.chatInterop = {
    mediaRecorder: null,
    audioChunks: [],

    startRecording: async function () {
        this.audioChunks = [];
        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            
            // Fallback mimeTypes
            let mimeType = 'audio/webm';
            if (!MediaRecorder.isTypeSupported(mimeType)) {
                mimeType = 'audio/mp4';
                if (!MediaRecorder.isTypeSupported(mimeType)) {
                    mimeType = ''; // Let browser choose
                }
            }

            this.mediaRecorder = new MediaRecorder(stream, { mimeType });
            this.mediaRecorder.ondataavailable = (event) => {
                if (event.data.size > 0) {
                    this.audioChunks.push(event.data);
                }
            };
            this.mediaRecorder.start();
            return true;
        } catch (err) {
            console.error("Error accessing microphone: ", err);
            return false;
        }
    },

    stopRecording: function () {
        return new Promise((resolve) => {
            if (!this.mediaRecorder || this.mediaRecorder.state === "inactive") {
                resolve("");
                return;
            }

            this.mediaRecorder.onstop = () => {
                if (!this.audioChunks || this.audioChunks.length === 0) {
                    resolve("");
                    return;
                }
                const mimeType = this.mediaRecorder.mimeType || 'audio/webm';
                const audioBlob = new Blob(this.audioChunks, { type: mimeType });
                const reader = new FileReader();
                reader.readAsDataURL(audioBlob);
                reader.onloadend = () => {
                    const base64String = reader.result ? reader.result.split(',')[1] : '';
                    // Release microphone icon after data is read
                    try {
                        this.mediaRecorder.stream.getTracks().forEach(track => track.stop());
                    } catch (e) {}
                    resolve(base64String || "");
                };
                reader.onerror = () => { resolve(""); };
            };

            this.mediaRecorder.onerror = () => { resolve(""); };

            try {
                this.mediaRecorder.stop();
            } catch (e) {
                resolve("");
            }
        });
    },

    playNotificationSound: function () {
        try {
            const ctx = new (window.AudioContext || window.webkitAudioContext)();
            // Two-tone pleasant chime: 880 Hz then 1100 Hz
            const playTone = (frequency, startTime, duration, gain) => {
                const osc = ctx.createOscillator();
                const gainNode = ctx.createGain();
                osc.connect(gainNode);
                gainNode.connect(ctx.destination);
                osc.type = 'sine';
                osc.frequency.setValueAtTime(frequency, startTime);
                gainNode.gain.setValueAtTime(0, startTime);
                gainNode.gain.linearRampToValueAtTime(gain, startTime + 0.01);
                gainNode.gain.exponentialRampToValueAtTime(0.001, startTime + duration);
                osc.start(startTime);
                osc.stop(startTime + duration);
            };
            const now = ctx.currentTime;
            playTone(880, now, 0.18, 0.25);
            playTone(1100, now + 0.12, 0.22, 0.18);
        } catch (e) {
            // Audio context may be blocked until user interaction - silently ignore
        }
    },

    audioPlayers: {},

    // Pre-load audio duration into the player label WITHOUT auto-playing.
    // Called right after a voice message bubble is rendered so the user sees
    // e.g. "0:12" instead of "--:--" before they tap play.
    preloadDuration: function (url, playerElementId, retryCount = 0) {
        const player = document.getElementById(playerElementId);
        if (!player) {
            if (retryCount < 10) {
                setTimeout(() => {
                    this.preloadDuration(url, playerElementId, retryCount + 1);
                }, 100);
            }
            return;
        }
        const durationText = player.querySelector('.audio-duration');
        if (!durationText) return;

        // If we already have an audio object for this player, duration may already be known
        if (this.audioPlayers[playerElementId]) return;

        const probe = new Audio();
        probe.preload = 'metadata';
        probe.src = url;

        const self = this;
        let fixed = false;

        probe.addEventListener('loadedmetadata', () => {
            if (!isFinite(probe.duration)) {
                // WebM Infinity workaround: seek to end to trigger duration calculation
                probe.currentTime = 1e101;
            } else {
                fixed = true;
                durationText.textContent = self.formatTime(probe.duration);
            }
        });

        probe.addEventListener('durationchange', () => {
            if (isFinite(probe.duration) && !fixed) {
                fixed = true;
                durationText.textContent = self.formatTime(probe.duration);
                probe.src = ''; // Release resource
            }
        });
    },

    playAudio: function (url, playerElementId) {
        const player = document.getElementById(playerElementId);
        if (!player) return;

        const playBtn = player.querySelector('.audio-play-btn');
        const playIcon = playBtn ? playBtn.querySelector('i') : null;
        const progressLine = player.querySelector('.audio-progress-line');
        const progressThumb = player.querySelector('.audio-progress-thumb');
        const durationText = player.querySelector('.audio-duration');

        // Check if already playing
        let audio = this.audioPlayers[playerElementId];
        if (audio) {
            if (!audio.paused) {
                audio.pause();
                if (playIcon) playIcon.className = "bi bi-play-fill";
                return;
            } else {
                audio.play();
                if (playIcon) playIcon.className = "bi bi-pause-fill";
                return;
            }
        }

        // Create new Audio
        audio = new Audio(url);
        this.audioPlayers[playerElementId] = audio;

        const self = this;
        let durationFixed = false;

        audio.addEventListener('loadedmetadata', () => {
            if (!isFinite(audio.duration)) {
                // WebM files from MediaRecorder don't embed a duration header — browser reports Infinity.
                // Seeking to a very large time forces the browser to scan to the end and calculate duration.
                audio.currentTime = 1e101;
            } else {
                durationFixed = true;
                if (durationText) durationText.textContent = self.formatTime(audio.duration);
            }
        });

        audio.addEventListener('durationchange', () => {
            // Fires after the seek-to-end trick resolves the real duration
            if (isFinite(audio.duration) && !durationFixed) {
                durationFixed = true;
                if (durationText) durationText.textContent = self.formatTime(audio.duration);
                // Seek back to start so the audio plays from beginning
                audio.currentTime = 0;
                audio.play();
                if (playIcon) playIcon.className = "bi bi-pause-fill";
            }
        });

        audio.addEventListener('timeupdate', () => {
            if (!isFinite(audio.duration)) return;
            const pct = (audio.currentTime / audio.duration) * 100;
            if (progressLine) progressLine.style.width = pct + '%';
            if (progressThumb) progressThumb.style.left = pct + '%';
            if (durationText) {
                // Show elapsed time while playing
                durationText.textContent = self.formatTime(audio.currentTime);
            }
        });

        audio.addEventListener('ended', () => {
            if (playIcon) playIcon.className = "bi bi-play-fill";
            if (progressLine) progressLine.style.width = '0%';
            if (progressThumb) progressThumb.style.left = '0%';
            // Restore total duration after playback ends
            if (durationText) durationText.textContent = self.formatTime(audio.duration);
            delete self.audioPlayers[playerElementId];
        });

        audio.play();
        if (playIcon) playIcon.className = "bi bi-pause-fill";
    },

    formatTime: function (secs) {
        if (isNaN(secs) || !isFinite(secs)) return "0:00";
        const m = Math.floor(secs / 60);
        const s = Math.floor(secs % 60);
        return m + ":" + (s < 10 ? "0" : "") + s;
    }
};
