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

// ── GLOBAL FORM ENTER KEY NAVIGATION ─────────────────────────────────────
// Automatically moves focus to the next input field on Enter key press in forms & modals
document.addEventListener('keydown', function (e) {
    if (e.key !== 'Enter') return;

    const target = e.target;
    if (!target) return;

    const tagName = target.tagName ? target.tagName.toUpperCase() : '';
    if (tagName !== 'INPUT' && tagName !== 'SELECT' && tagName !== 'TEXTAREA') {
        return;
    }

    // Ignore Enter on buttons or submit elements so click events trigger normally
    if (target.type === 'button' || target.type === 'submit') {
        return;
    }

    // Allow Enter key multiline in textareas if Shift key is pressed
    if (tagName === 'TEXTAREA' && e.shiftKey) {
        return;
    }

    // Ignore if target is part of an autocomplete dropdown list item or search option
    if (target.classList.contains('master-search-item') || 
        target.classList.contains('search-dropdown-item') || 
        target.classList.contains('autocomplete-item') ||
        target.classList.contains('inline-dropdown')) {
        return;
    }

    // Special handling for Prescription table row:
    // If target is in the Instructions column (.col-instructions input) or last column of medicine row,
    // pressing Enter automatically triggers "Add Medicine" to add the next row (S.No 2, 3...)!
    if (target.closest('.col-instructions, td.col-instructions')) {
        const addMedBtn = target.closest('.prescription-tab-form, .casesheet-modal, .structured-table-card')
            ?.querySelector('.btn-add-medicine-dashed, button.btn-add-item');
        if (addMedBtn && typeof addMedBtn.click === 'function') {
            e.preventDefault();
            e.stopPropagation();
            addMedBtn.click();
            return;
        }
    }

    // Find the closest container card/modal/form section for scoped navigation
    const container = target.closest(
        '.casesheet-modal, .casesheet-backdrop, .casesheet-body, .notes-tab-form, .vitals-tab-form, .symptoms-tab-form, .diagnosis-tab-form, .prescription-tab-form, .investigation-tab-form, .tab-content, .modal-overlay, .modal-form-card, .modal-dialog-custom, .modal-content, .modal-body, .glass-modal, .large-modal-custom, .form-grid-layout, .filter-card, .dashboard-container, .referral-page, form'
    ) || document.body;

    // Selector for focusable input controls
    const selector = 'input:not([type="hidden"]):not([readonly]):not([disabled]):not([type="file"]):not([type="checkbox"]):not([type="radio"]), select:not([disabled]), textarea:not([disabled]), button.btn-save, button.btn-save-customer, button.btn-primary-custom, button.btn-submit, button.btn-add-item, button.btn-add-medicine-dashed';

    const focusables = Array.from(container.querySelectorAll(selector)).filter(el => {
        return el.offsetWidth > 0 && el.offsetHeight > 0 && window.getComputedStyle(el).visibility !== 'hidden';
    });

    const currentIndex = focusables.indexOf(target);
    if (currentIndex > -1) {
        if (currentIndex < focusables.length - 1) {
            e.preventDefault();
            e.stopPropagation();

            const nextEl = focusables[currentIndex + 1];
            nextEl.focus();

            if (typeof nextEl.select === 'function' && nextEl.tagName === 'INPUT' && nextEl.type !== 'date' && nextEl.type !== 'datetime-local') {
                try {
                    nextEl.select();
                } catch (ex) { }
            }
        } else {
            // Reached last element -> click the Add Medicine / Save / Submit button if available
            const saveBtn = container.querySelector('.btn-add-medicine-dashed, .btn-save, .btn-save-customer, .btn-primary-custom, .btn-submit, .btn-add-item');
            if (saveBtn && typeof saveBtn.click === 'function' && saveBtn !== target) {
                e.preventDefault();
                e.stopPropagation();
                saveBtn.click();
            }
        }
    }
});

window.focusFirstModalInput = function (containerSelector) {
    setTimeout(function () {
        const container = document.querySelector(containerSelector || '.casesheet-modal, .modal-form-card, .modal-dialog-custom, .glass-modal');
        if (!container) return;
        const first = container.querySelector('select:not([disabled]), input:not([type="hidden"]):not([readonly]):not([disabled]):not([type="file"]):not([type="checkbox"]), textarea:not([disabled])');
        if (first) {
            first.focus();
            if (typeof first.select === 'function' && first.tagName === 'INPUT') {
                try { first.select(); } catch (ex) { }
            }
        }
    }, 150);
};

window.focusLastPrescriptionDrugInput = function (containerSelector) {
    setTimeout(function () {
        const container = document.querySelector(containerSelector || '.prescription-tab-form, .casesheet-modal');
        if (!container) return;
        const drugInputs = Array.from(container.querySelectorAll('.col-drug input.form-control-input, td.col-drug input, input[placeholder*="Paracetamol"]'));
        if (drugInputs.length > 0) {
            const lastInput = drugInputs[drugInputs.length - 1];
            lastInput.focus();
            if (typeof lastInput.select === 'function') {
                try { lastInput.select(); } catch (ex) { }
            }
        }
    }, 150);
};

window.focusLastDiagnosisInput = function (containerSelector) {
    setTimeout(function () {
        const container = document.querySelector(containerSelector || '.diagnosis-tab-form, .casesheet-modal');
        if (!container) return;
        const diagInputs = Array.from(container.querySelectorAll('.col-diag-code input, td.col-diag-code input, input[placeholder*="Search ICD"]'));
        if (diagInputs.length > 0) {
            const lastInput = diagInputs[diagInputs.length - 1];
            lastInput.focus();
            if (typeof lastInput.select === 'function') {
                try { lastInput.select(); } catch (ex) { }
            }
        }
    }, 150);
};


