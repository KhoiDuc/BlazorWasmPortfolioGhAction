window.tinyMceLoader = {
    _loaded: false,
    _loading: null,

    ensureLoaded: function () {
        if (window.tinymce) {
            this._loaded = true;
            return Promise.resolve();
        }

        if (this._loading) {
            return this._loading;
        }

        this._loading = new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = 'https://cdn.jsdelivr.net/npm/tinymce@8.8.2/tinymce.min.js';
            script.onload = function () {
                window.tinyMceLoader._loaded = true;
                resolve();
            };
            script.onerror = reject;
            document.head.appendChild(script);
        });

        return this._loading;
    }
};
