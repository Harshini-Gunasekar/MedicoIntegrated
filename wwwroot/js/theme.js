// Medico Portal - Theme Manager (Dark Mode / Bright Mode)
window.medicoTheme = {
    getTheme: function () {
        try {
            return localStorage.getItem('medico_theme') || 'light';
        } catch (e) {
            return 'light';
        }
    },
    setTheme: function (theme) {
        if (theme !== 'dark' && theme !== 'light') theme = 'light';
        try {
            localStorage.setItem('medico_theme', theme);
        } catch (e) {}

        document.documentElement.setAttribute('data-theme', theme);
        if (document.body) {
            if (theme === 'dark') {
                document.body.classList.add('dark-mode');
            } else {
                document.body.classList.remove('dark-mode');
            }
        }
        window.dispatchEvent(new CustomEvent('medicoThemeChanged', { detail: theme }));
        return theme;
    },
    toggleTheme: function () {
        var current = this.getTheme();
        var newTheme = (current === 'dark') ? 'light' : 'dark';
        return this.setTheme(newTheme);
    },
    init: function () {
        var saved = this.getTheme();
        this.setTheme(saved);
        return saved;
    }
};

// Immediate initialization to prevent any theme flash on initial load
(function () {
    try {
        var saved = localStorage.getItem('medico_theme') || 'light';
        document.documentElement.setAttribute('data-theme', saved);
        var applyClass = function () {
            if (document.body) {
                if (saved === 'dark') document.body.classList.add('dark-mode');
                else document.body.classList.remove('dark-mode');
            }
        };
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', applyClass);
        } else {
            applyClass();
        }
    } catch (e) {}
})();
