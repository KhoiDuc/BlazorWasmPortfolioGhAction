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
