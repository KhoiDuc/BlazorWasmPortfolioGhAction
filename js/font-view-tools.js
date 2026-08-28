window.fontViewTools = {
    _otLoaded: false,
    _fontFaceId: 0,

    ensureOpenType: function () {
        if (this._otLoaded) return Promise.resolve();
        var self = this;
        return scriptLoader.load('https://cdn.jsdelivr.net/npm/opentype.js@1.3.4/dist/opentype.min.js')
            .then(function () { self._otLoaded = true; });
    },

    loadFont: async function (base64) {
        await this.ensureOpenType();
        var binary = atob(base64);
        var bytes = new Uint8Array(binary.length);
        for (var i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);

        var font = opentype.parse(bytes.buffer);
        if (!font) return null;

        var glyphs = [];
        for (var key in font.glyphs) {
            if (!Object.prototype.hasOwnProperty.call(font.glyphs, key)) continue;
            var g = font.glyphs[key];
            if (g.unicode === undefined || g.unicode === null) continue;
            var cp = typeof g.unicode === 'number' ? g.unicode : (Array.isArray(g.unicode) ? g.unicode[0] : 0);
            if (!cp) continue;
            glyphs.push({
                unicode: cp,
                hex: cp.toString(16).toUpperCase().padStart(4, '0'),
                name: g.name || ('U+' + cp.toString(16).toUpperCase())
            });
        }

        glyphs.sort(function (a, b) { return a.unicode - b.unicode; });

        return {
            family: (font.names && font.names.fullName && font.names.fullName.en) ? font.names.fullName.en[0] : 'Unknown',
            subfamily: (font.names && font.names.fontSubfamily && font.names.fontSubfamily.en) ? font.names.fontSubfamily.en[0] : '',
            numGlyphs: font.glyphs.length,
            glyphs: glyphs
        };
    },

    registerFontFace: async function (base64, family) {
        await this.ensureOpenType();
        var id = 'fv-' + (++this._fontFaceId);
        var bin = atob(base64);
        var bytes = new Uint8Array(bin.length);
        for (var i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
        var blob = new Blob([bytes]);
        var url = URL.createObjectURL(blob);
        var ff = new FontFace(id, "url('" + url + "')");
        await ff.load();
        document.fonts.add(ff);
        return id;
    }
};

window.fontViewScrollTo = function (elementId) {
    var el = document.getElementById(elementId);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
};