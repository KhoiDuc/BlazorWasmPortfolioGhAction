window.tradingAuth = {
    getItem: function (key) {
        try { return localStorage.getItem(key); } catch { return null; }
    },
    setItem: function (key, value) {
        try { localStorage.setItem(key, value); } catch { }
    },
    removeItem: function (key) {
        try { localStorage.removeItem(key); } catch { }
    },
    confirm: function (message) {
        return confirm(message);
    },
    _savedFocus: null,
    trapFocus: function (dialogSelector) {
        this._savedFocus = document.activeElement;
        var dialog = document.querySelector(dialogSelector);
        if (!dialog) return;
        var focusable = dialog.querySelectorAll('input, button, select, textarea, a[href], [tabindex]:not([tabindex="-1"])');
        if (focusable.length > 0) focusable[0].focus();
        this._handler = function (e) {
            if (e.key !== 'Tab') return;
            var f = Array.from(dialog.querySelectorAll('input, button, select, textarea, a[href], [tabindex]:not([tabindex="-1"])'));
            if (f.length === 0) return;
            var first = f[0], last = f[f.length - 1];
            if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
            else if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
        };
        dialog.addEventListener('keydown', this._handler);
    },
    releaseFocus: function () {
        if (this._handler) {
            var dialog = document.querySelector('.broker-qs-overlay');
            if (dialog) dialog.removeEventListener('keydown', this._handler);
            this._handler = null;
        }
        if (this._savedFocus) { try { this._savedFocus.focus(); } catch { } this._savedFocus = null; }
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

window.tradingCharts = {
    bar: function (canvasId, labels, values, label) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !window.Chart) return;
        if (canvas._chart) canvas._chart.destroy();
        canvas._chart = new Chart(canvas.getContext('2d'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{ label: label || 'Value', data: values, backgroundColor: '#3b82f6' }]
            },
            options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } }
        });
    },
    doughnut: function (canvasId, labels, values) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !window.Chart) return;
        if (canvas._chart) canvas._chart.destroy();
        var colors = ['#3b82f6', '#10b981', '#f59e0b', '#8b5cf6', '#ef4444', '#06b6d4'];
        canvas._chart = new Chart(canvas.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{ data: values, backgroundColor: labels.map(function (_, i) { return colors[i % colors.length]; }) }]
            },
            options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
        });
    }
};

window.tradingExport = {
    csv: function (filename, content) {
        var blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = filename || 'export.csv';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};
