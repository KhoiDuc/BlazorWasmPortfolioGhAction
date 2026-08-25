window.tradingAuth = {
    getItem: function (key) {
        try { return localStorage.getItem(key); } catch { return null; }
    },
    setItem: function (key, value) {
        try { localStorage.setItem(key, value); } catch { }
    },
    removeItem: function (key) {
        try { localStorage.removeItem(key); } catch { }
    }
};

window.tradingViewInterop = {
    _widgets: {},
    ensureScript: function () {
        return new Promise(function (resolve) {
            if (window.TradingView) { resolve(); return; }
            var s = document.createElement('script');
            s.src = 'https://s3.tradingview.com/tv.js';
            s.onload = resolve;
            document.body.appendChild(s);
        });
    },
    render: async function (elementId, symbol, height) {
        await window.tradingViewInterop.ensureScript();
        var el = document.getElementById(elementId);
        if (!el) return;
        el.innerHTML = '';
        var sym = symbol || 'BINANCE:BTCUSDT';
        if (!sym.includes(':') && sym.toUpperCase().endsWith('USDT')) {
            sym = 'BINANCE:' + sym;
        }
        new window.TradingView.widget({
            container_id: elementId,
            width: '100%',
            height: height || 380,
            symbol: sym,
            interval: '1D',
            timezone: 'Asia/Bangkok',
            theme: 'light',
            style: '1',
            locale: 'en',
            enable_publishing: false
        });
    },
    destroy: function (elementId) {
        var el = document.getElementById(elementId);
        if (el) el.innerHTML = '';
    }
};

window.tradingSpeech = {
    speak: function (text) {
        if (!window.speechSynthesis) return;
        var u = new SpeechSynthesisUtterance(text);
        u.lang = 'vi-VN';
        window.speechSynthesis.speak(u);
    },
    stop: function () {
        if (window.speechSynthesis) window.speechSynthesis.cancel();
    }
};
