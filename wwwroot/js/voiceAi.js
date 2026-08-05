window.MedicoSpeech = {
    recognition: null,
    isListening: false,

    startListening: function (dotNetRef, language) {
        var SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            alert("Speech recognition is not supported in this browser. Please use Google Chrome or Microsoft Edge.");
            return false;
        }

        if (this.recognition) {
            try { this.recognition.stop(); } catch (e) {}
        }

        this.recognition = new SpeechRecognition();
        this.recognition.continuous = false;
        this.recognition.interimResults = true;
        this.recognition.lang = language || 'en-US';

        var self = this;
        this.isListening = true;

        this.recognition.onresult = function (event) {
            var transcript = '';
            var isFinal = false;
            for (var i = event.resultIndex; i < event.results.length; i++) {
                transcript += event.results[i][0].transcript;
                if (event.results[i].isFinal) isFinal = true;
            }
            if (transcript) {
                dotNetRef.invokeMethodAsync('OnSpeechTranscript', transcript, isFinal);
            }
        };

        this.recognition.onerror = function (event) {
            console.warn('Speech recognition error:', event.error);
            self.isListening = false;
            dotNetRef.invokeMethodAsync('OnSpeechError', event.error);
        };

        this.recognition.onend = function () {
            self.isListening = false;
            dotNetRef.invokeMethodAsync('OnSpeechEnd');
        };

        try {
            this.recognition.start();
            return true;
        } catch (err) {
            console.error('Failed to start speech recognition:', err);
            return false;
        }
    },

    stopListening: function () {
        if (this.recognition) {
            try { this.recognition.stop(); } catch (e) {}
            this.isListening = false;
        }
    },

    speakText: function (text, language) {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();

            var cleanText = text.replace(/<[^>]*>?/gm, '').replace(/\*\*/g, '').replace(/#/g, '').replace(/•/g, '');

            var utterance = new SpeechSynthesisUtterance(cleanText);
            utterance.lang = language || 'ta-IN';
            utterance.rate = 1.0;
            utterance.pitch = 1.0;

            window.speechSynthesis.speak(utterance);
        }
    },

    stopSpeaking: function () {
        if ('speechSynthesis' in window) {
            window.speechSynthesis.cancel();
        }
    }
};

window.playAlertSound = function (type) {
    try {
        var AudioCtx = window.AudioContext || window.webkitAudioContext;
        if (!AudioCtx) return;
        var ctx = new AudioCtx();
        if (ctx.state === 'suspended') {
            ctx.resume();
        }

        var now = ctx.currentTime;
        var osc = ctx.createOscillator();
        var gain = ctx.createGain();

        if (type === 'wait_30m') {
            // Urgent 30+ min wait alert chime (Double beep high pitch)
            osc.type = 'sine';
            osc.frequency.setValueAtTime(880, now);
            osc.frequency.setValueAtTime(1174.66, now + 0.15);
            gain.gain.setValueAtTime(0.35, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.5);

            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(now);
            osc.stop(now + 0.5);
        } else if (type === 'wait_20m') {
            // 20+ min wait alert chime (Warning tone)
            osc.type = 'sine';
            osc.frequency.setValueAtTime(659.25, now);
            osc.frequency.setValueAtTime(880, now + 0.12);
            gain.gain.setValueAtTime(0.3, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.45);

            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(now);
            osc.stop(now + 0.45);
        } else {
            // New Token / General info chime (Gentle notification ding)
            osc.type = 'sine';
            osc.frequency.setValueAtTime(587.33, now);
            osc.frequency.setValueAtTime(880, now + 0.1);
            gain.gain.setValueAtTime(0.25, now);
            gain.gain.exponentialRampToValueAtTime(0.001, now + 0.35);

            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(now);
            osc.stop(now + 0.35);
        }
    } catch (e) {
        console.warn('Could not play alert audio chime:', e);
    }
};
