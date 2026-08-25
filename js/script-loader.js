window.scriptLoader = {
    _loaded: {},

    load: function (url) {
        if (this._loaded[url]) {
            return this._loaded[url];
        }

        this._loaded[url] = new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = url;
            script.onload = resolve;
            script.onerror = reject;
            document.head.appendChild(script);
        });

        return this._loaded[url];
    },

    loadChartJs: function () {
        return this.load('https://cdn.jsdelivr.net/npm/chart.js');
    },

    loadPhysicsLibs: function () {
        var self = this;
        return self.load('https://cdnjs.cloudflare.com/ajax/libs/p5.js/1.4.0/p5.js')
            .then(function () { return self.load('https://cdnjs.cloudflare.com/ajax/libs/matter-js/0.17.1/matter.min.js'); })
            .then(function () { return self.load('https://cdn.jsdelivr.net/npm/planck@latest/dist/planck.min.js'); });
    },

    loadInteractJs: function () {
        return this.load('https://cdn.jsdelivr.net/npm/interactjs/dist/interact.min.js');
    }
};
