(function () {
    const STORAGE_KEY = 'portfolio-theme';

    function getTheme() {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (saved === 'light' || saved === 'dark') return saved;
        return 'light';
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(STORAGE_KEY, theme);
        window.__currentTheme = theme;
        window.dispatchEvent(new CustomEvent('theme-changed', { detail: theme }));
    }

    window.themeManager = {
        init: function () {
            applyTheme(getTheme());
        },
        toggle: function () {
            const current = getTheme();
            applyTheme(current === 'light' ? 'dark' : 'light');
        },
        get: function () {
            return getTheme();
        },
        set: function (theme) {
            if (theme === 'light' || theme === 'dark') applyTheme(theme);
        }
    };

    themeManager.init();
})();