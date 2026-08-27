window.barcodeTools = {
    downloadText: function (filename, content, mimeType) {
        var blob = new Blob([content], { type: mimeType || 'text/plain;charset=utf-8' });
        var url = URL.createObjectURL(blob);
        var a = document.createElement('a');
        a.href = url;
        a.download = filename || 'download.txt';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    },

    downloadSvgAsPng: function (filename, svgText, scale) {
        scale = scale || 2;
        return new Promise(function (resolve, reject) {
            try {
                var blob = new Blob([svgText], { type: 'image/svg+xml;charset=utf-8' });
                var url = URL.createObjectURL(blob);
                var img = new Image();
                img.onload = function () {
                    try {
                        var canvas = document.createElement('canvas');
                        canvas.width = Math.max(1, Math.round(img.width * scale));
                        canvas.height = Math.max(1, Math.round(img.height * scale));
                        var ctx = canvas.getContext('2d');
                        ctx.fillStyle = '#ffffff';
                        ctx.fillRect(0, 0, canvas.width, canvas.height);
                        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);
                        URL.revokeObjectURL(url);
                        canvas.toBlob(function (pngBlob) {
                            if (!pngBlob) {
                                reject(new Error('PNG encode failed'));
                                return;
                            }
                            var pngUrl = URL.createObjectURL(pngBlob);
                            var a = document.createElement('a');
                            a.href = pngUrl;
                            a.download = filename || 'barcode.png';
                            document.body.appendChild(a);
                            a.click();
                            document.body.removeChild(a);
                            URL.revokeObjectURL(pngUrl);
                            resolve(true);
                        }, 'image/png');
                    } catch (e) {
                        URL.revokeObjectURL(url);
                        reject(e);
                    }
                };
                img.onerror = function () {
                    URL.revokeObjectURL(url);
                    reject(new Error('Failed to render SVG'));
                };
                img.src = url;
            } catch (e) {
                reject(e);
            }
        });
    },

    /**
     * Decode image bytes to RGBA pixels with quiet-zone padding for ZXing.
     * Returns { width, height, rgba: number[] } or null.
     */
    imageToRgba: async function (bytes, contentType) {
        var mime = contentType || 'application/octet-stream';
        var u8 = bytes instanceof Uint8Array ? bytes : new Uint8Array(bytes);

        // Detect SVG
        var head = '';
        try {
            head = new TextDecoder().decode(u8.slice(0, Math.min(120, u8.length))).trimStart();
        } catch (_) { /* binary */ }
        var isSvg = mime.indexOf('svg') >= 0
            || head.startsWith('<?xml')
            || head.toLowerCase().startsWith('<svg');

        var bitmap;
        if (isSvg) {
            var svgBlob = new Blob([u8], { type: 'image/svg+xml' });
            var svgUrl = URL.createObjectURL(svgBlob);
            try {
                bitmap = await createImageBitmap(await (await fetch(svgUrl)).blob());
            } finally {
                URL.revokeObjectURL(svgUrl);
            }
        } else {
            var blob = new Blob([u8], { type: mime.startsWith('image/') ? mime : 'image/png' });
            bitmap = await createImageBitmap(blob);
        }

        var pad = Math.max(20, Math.round(Math.min(bitmap.width, bitmap.height) * 0.1));
        var width = bitmap.width + pad * 2;
        var height = bitmap.height + pad * 2;
        var canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;
        var ctx = canvas.getContext('2d', { willReadFrequently: true });
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, width, height);
        ctx.drawImage(bitmap, pad, pad);
        if (bitmap.close) bitmap.close();

        var imageData = ctx.getImageData(0, 0, width, height);
        var u8 = imageData.data;
        var chunk = 0x8000;
        var binary = '';
        for (var i = 0; i < u8.length; i += chunk) {
            binary += String.fromCharCode.apply(null, u8.subarray(i, Math.min(i + chunk, u8.length)));
        }
        return {
            width: width,
            height: height,
            rgbaBase64: btoa(binary)
        };
    }
};
