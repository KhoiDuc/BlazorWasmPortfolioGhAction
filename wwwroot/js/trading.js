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
    },
    _closeHandlers: {},
    registerCloseOnOutside: function (id, menuSelector, toggleSelector, dotNetRef) {
        this.unregisterCloseOnOutside(id);
        var menu = document.querySelector(menuSelector);
        var toggle = document.querySelector(toggleSelector);
        if (!menu || !toggle) return;
        var onPointer = function (e) {
            if (menu.contains(e.target) || toggle.contains(e.target)) return;
            window.tradingAuth.unregisterCloseOnOutside(id);
            dotNetRef.invokeMethodAsync('CloseMenuFromJs');
        };
        var onKey = function (e) {
            if (e.key !== 'Escape') return;
            window.tradingAuth.unregisterCloseOnOutside(id);
            dotNetRef.invokeMethodAsync('CloseMenuFromJs');
        };
        document.addEventListener('pointerdown', onPointer, true);
        document.addEventListener('keydown', onKey, true);
        this._closeHandlers[id] = function () {
            document.removeEventListener('pointerdown', onPointer, true);
            document.removeEventListener('keydown', onKey, true);
        };
    },
    unregisterCloseOnOutside: function (id) {
        var cleanup = this._closeHandlers[id];
        if (cleanup) {
            cleanup();
            delete this._closeHandlers[id];
        }
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
    },
    lines: function (canvasId, labels, datasets, yMin, yMax) {
        var canvas = document.getElementById(canvasId);
        if (!canvas || !window.Chart) return;
        if (canvas._chart) canvas._chart.destroy();
        var mapped = (datasets || []).map(function (d) {
            return {
                label: d.label || '',
                data: d.data || [],
                borderColor: d.color || '#3b82f6',
                backgroundColor: 'transparent',
                borderWidth: 2,
                pointRadius: 0,
                tension: 0.25
            };
        });
        canvas._chart = new Chart(canvas.getContext('2d'), {
            type: 'line',
            data: { labels: labels || [], datasets: mapped },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: { legend: { position: 'bottom' } },
                scales: {
                    x: { ticks: { maxTicksLimit: 8 } },
                    y: {
                        min: typeof yMin === 'number' ? yMin : undefined,
                        max: typeof yMax === 'number' ? yMax : undefined
                    }
                }
            }
        });
    },
    destroy: function (canvasId) {
        var canvas = document.getElementById(canvasId);
        if (canvas && canvas._chart) {
            canvas._chart.destroy();
            canvas._chart = null;
        }
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

window.tradingSignals = {
    _dotNet: null,
    _escHandler: null,
    initEsc: function (dotNetRef) {
        this._dotNet = dotNetRef;
        this._escHandler = function () {
            if (!document.fullscreenElement) {
                dotNetRef.invokeMethodAsync('OnEscPressed');
            }
        };
        document.addEventListener('fullscreenchange', this._escHandler);
    },
    disposeEsc: function (dotNetRef) {
        if (this._escHandler) {
            document.removeEventListener('fullscreenchange', this._escHandler);
            this._escHandler = null;
        }
        if (dotNetRef) dotNetRef.dispose();
        this._dotNet = null;
    },
    requestFs: function (elRef) {
        var el = elRef;
        if (el && el.requestFullscreen) el.requestFullscreen();
    },
    exitFs: function () {
        if (document.fullscreenElement && document.exitFullscreen) document.exitFullscreen();
    }
};
