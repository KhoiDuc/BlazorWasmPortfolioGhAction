window.localFontTools = {
    isSupported: function () {
        return typeof window.queryLocalFonts === 'function';
    },

    getPermission: async function () {
        try {
            if (navigator.permissions && navigator.permissions.query) {
                var status = await navigator.permissions.query({ name: 'local-fonts' });
                return status.state; // 'granted' | 'denied' | 'prompt'
            }
        } catch (_) { /* permission not supported */ }
        return 'prompt';
    },

    query: async function () {
        var fonts = await window.queryLocalFonts();
        return fonts.map(function (f) {
            return {
                postscriptName: f.postscriptName || '',
                fullName: f.fullName || '',
                family: f.family || '',
                style: f.style || ''
            };
        });
    },

    getFontFile: async function (postscriptName) {
        var fonts = await window.queryLocalFonts({ postscriptNames: [postscriptName] });
        if (!fonts || fonts.length === 0) return null;

        var blob = await fonts[0].blob();
        var arrayBuffer = await blob.arrayBuffer();
        var u8 = new Uint8Array(arrayBuffer);
        var binary = '';
        var chunk = 0x8000;
        for (var i = 0; i < u8.length; i += chunk) {
            binary += String.fromCharCode.apply(null, u8.subarray(i, Math.min(i + chunk, u8.length)));
        }
        return { base64: btoa(binary) };
    }
};