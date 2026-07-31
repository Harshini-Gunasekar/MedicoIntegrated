// ── AUTO PAGE RELOAD EVERY 60 SECONDS ─────────────────────────────────────
// This ensures the display is always fresh (full browser reload every minute)
window.hospitalDisplay = {
    startAutoReload: function (intervalMs) {
        if (window._autoReloadTimer) clearInterval(window._autoReloadTimer);
        window._autoReloadTimer = setInterval(function () {
            console.log('[HospitalDisplay] Auto-reloading page...');
            window.location.reload();
        }, intervalMs || 60000);
        console.log('[HospitalDisplay] Auto-reload armed: every ' + (intervalMs || 60000) / 1000 + 's');
    },
    stopAutoReload: function () {
        if (window._autoReloadTimer) {
            clearInterval(window._autoReloadTimer);
            window._autoReloadTimer = null;
        }
    }
};

window.hospitalAudio = {
    playChimeAndSpeak: function (text, language, rate, pitch) {
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            if (!AudioContext) {
                this.speak(text, language, rate, pitch);
                return;
            }

            const ctx = new AudioContext();

            const playTone = (freq, startTime, duration) => {
                const osc = ctx.createOscillator();
                const gain = ctx.createGain();

                osc.type = 'sine';
                osc.frequency.setValueAtTime(freq, startTime);

                gain.gain.setValueAtTime(0, startTime);
                gain.gain.linearRampToValueAtTime(0.2, startTime + 0.05); // Attack
                gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration); // Release

                osc.connect(gain);
                gain.connect(ctx.destination);

                osc.start(startTime);
                osc.stop(startTime + duration);
            };

            // Double pleasant chime
            playTone(523.25, ctx.currentTime, 0.4); // C5
            playTone(659.25, ctx.currentTime + 0.18, 0.5); // E5
            playTone(783.99, ctx.currentTime + 0.36, 0.6); // G5

            setTimeout(() => {
                this.speak(text, language, rate, pitch);
            }, 850);

        } catch (e) {
            console.error("Audio chime error:", e);
            this.speak(text, language, rate, pitch);
        }
    },

    getFemaleVoice: function (voices, targetLang) {
        if (!voices || voices.length === 0) return null;

        const femaleKeywords = ['female', 'heera', 'kalpana', 'google', 'zira', 'hazel', 'susan', 'haruka', 'luna', 'yuki', 'en-in-x-dfy', 'ta-in-x-taf'];
        const maleKeywords = ['male', 'david', 'ravi', 'valluvar', 'george', 'mark', 'hemant', 'prakash', 'stefan', 'linus', 'pavel', 'hari'];

        let matches = [];

        if (targetLang) {
            const langLower = targetLang.toLowerCase();
            matches = voices.filter(v => v.lang.toLowerCase().includes(langLower));

            // Prefer female voice in target language
            let voice = matches.find(v => femaleKeywords.some(kw => v.name.toLowerCase().includes(kw)));
            if (voice) return voice;

            // Prefer any voice in target language that is not explicitly male
            voice = matches.find(v => !maleKeywords.some(kw => v.name.toLowerCase().includes(kw)));
            if (voice) return voice;

            // If target language match exists but is male, use it rather than falling back to English (which cannot speak target language)
            if (matches.length > 0) return matches[0];
        }

        // Fallback to standard English (India) or general English female voice
        let enInFemale = voices.find(v => v.lang.toLowerCase().includes('en-in') && femaleKeywords.some(kw => v.name.toLowerCase().includes(kw)));
        if (enInFemale) return enInFemale;

        let enFemale = voices.find(v => v.lang.toLowerCase().includes('en') && femaleKeywords.some(kw => v.name.toLowerCase().includes(kw)));
        if (enFemale) return enFemale;

        // Try to find any female voice on system
        let absoluteFemale = voices.find(v => femaleKeywords.some(kw => v.name.toLowerCase().includes(kw)));
        if (absoluteFemale) return absoluteFemale;

        return voices[0];
    },

    speak: function (text, language, rate, pitch) {
        if (!('speechSynthesis' in window)) return;

        window.speechSynthesis.cancel();

        const utterance = new SpeechSynthesisUtterance(text);
        utterance.lang = language || 'en-IN';
        utterance.rate = rate || 0.9;
        utterance.pitch = pitch || 1.0;

        const voices = window.speechSynthesis.getVoices();

        if (voices.length > 0) {
            let selectedVoice = this.getFemaleVoice(voices, language || 'en-IN');
            if (selectedVoice) {
                utterance.voice = selectedVoice;
            }
        }

        window.speechSynthesis.speak(utterance);
    },

    getVoicesAsync: function () {
        return new Promise(resolve => {
            let voices = window.speechSynthesis.getVoices();
            if (voices.length > 0) {
                resolve(voices);
                return;
            }
            window.speechSynthesis.onvoiceschanged = () => {
                resolve(window.speechSynthesis.getVoices());
            };
            setTimeout(() => {
                resolve(window.speechSynthesis.getVoices());
            }, 500);
        });
    },

    playBilingualNextPatientAnnouncement: async function (tokenNumber, roomNumber) {
        if (!('speechSynthesis' in window)) return;
        window.speechSynthesis.cancel();

        let engText = `Next token number ${tokenNumber}.`;
        let tamText = `டோக்கன் ${tokenNumber}, தயவுசெய்து ஆலோசனை அறைக்கு வரவும்.`;

        if (roomNumber && String(roomNumber).trim() !== '' && String(roomNumber).trim().toLowerCase() !== 'null') {
            engText = `Next token number ${tokenNumber}, Room number ${roomNumber}.`;
            tamText = `டோக்கன் எண் ${tokenNumber}, தயவுசெய்து அறை எண் ${roomNumber} க்கு வரவும்.`;
        }

        const voices = await this.getVoicesAsync();

        // 1. English
        window._engUtterance = new SpeechSynthesisUtterance(engText);
        window._engUtterance.lang = 'en-IN';
        window._engUtterance.rate = 0.9;
        let engVoice = this.getFemaleVoice(voices, 'en-IN');
        if (engVoice) { window._engUtterance.voice = engVoice; }

        // 2. Tamil
        window._tamUtterance = new SpeechSynthesisUtterance(tamText);
        window._tamUtterance.lang = 'ta-IN';
        window._tamUtterance.rate = 0.85;
        let tamVoice = this.getFemaleVoice(voices, 'ta-IN');
        if (tamVoice) { window._tamUtterance.voice = tamVoice; }

        // Chain the Tamil announcement after English finishes
        window._engUtterance.onend = function () {
            window.speechSynthesis.speak(window._tamUtterance);
        };

        window._engUtterance.onerror = function () {
            window.speechSynthesis.speak(window._tamUtterance);
        };

        window.speechSynthesis.speak(window._engUtterance);
    }
};
