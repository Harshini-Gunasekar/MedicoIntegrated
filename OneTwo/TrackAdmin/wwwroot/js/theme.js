window.ThemeManager = {
    toggleTheme: function () {
        const isDark = document.documentElement.classList.toggle('dark-theme');
        localStorage.setItem('theme', isDark ? 'dark' : 'light');
        return isDark;
    },
    getTheme: function () {
        return document.documentElement.classList.contains('dark-theme') ? 'dark' : 'light';
    }
};
