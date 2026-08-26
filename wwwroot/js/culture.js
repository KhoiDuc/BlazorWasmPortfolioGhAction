(function () {
    const STORAGE_KEY = 'app-culture';

    function normalize(value) {
        if (!value) return 'vi';
        var v = String(value).trim().toLowerCase();
        if (v === 'en' || v === 'en-us' || v === 'en-gb') return 'en';
        if (v === 'vn' || v === 'vi' || v === 'vi-vn') return 'vi';
        return 'vi';
    }

    function getCulture() {
        try {
            return normalize(localStorage.getItem(STORAGE_KEY));
        } catch (e) {
            return 'vi';
        }
    }

    function setDocumentLang(lang) {
        document.documentElement.setAttribute('lang', normalize(lang));
    }

    function apply(culture) {
        var c = normalize(culture);
        try {
            localStorage.setItem(STORAGE_KEY, c);
        } catch (e) { }
        setDocumentLang(c);
        window.__currentCulture = c;
        window.dispatchEvent(new CustomEvent('culture-changed', { detail: c }));
    }

    window.cultureManager = {
        init: function () {
            apply(getCulture());
        },
        get: function () {
            return getCulture();
        },
        set: function (culture) {
            apply(culture);
        },
        setDocumentLang: setDocumentLang,
        toggle: function () {
            apply(getCulture() === 'en' ? 'vi' : 'en');
        }
    };

    cultureManager.init();
})();
