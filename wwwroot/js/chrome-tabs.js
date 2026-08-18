// Medico Portal - Chrome Tab Strip Persistence & Scroll Manager
window.medicoTabs = {
    saveTabs: function (tabsJson, activeUrl) {
        try {
            localStorage.setItem('medico_chrome_tabs', tabsJson);
            localStorage.setItem('medico_chrome_active_url', activeUrl || '');
        } catch (e) {
            console.error('Error saving tabs to localStorage:', e);
        }
    },
    loadTabs: function () {
        try {
            var tabsJson = localStorage.getItem('medico_chrome_tabs');
            var activeUrl = localStorage.getItem('medico_chrome_active_url') || '';
            if (!tabsJson) return null;
            return JSON.stringify({
                tabs: JSON.parse(tabsJson),
                activeUrl: activeUrl
            });
        } catch (e) {
            console.error('Error loading tabs from localStorage:', e);
            return null;
        }
    },
    clearTabs: function () {
        try {
            localStorage.removeItem('medico_chrome_tabs');
            localStorage.removeItem('medico_chrome_active_url');
        } catch (e) {}
    },
    scrollStrip: function (direction) {
        var el = document.getElementById('chromeTabStrip');
        if (!el) return;
        var scrollAmount = direction === 'left' ? -220 : 220;
        el.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
};
