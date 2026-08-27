// Mermaid.js Diagram Rendering helpers for DiagramViewer
let currentTheme = 'default';

window.renderMermaid = async function (code, config) {
    try {
        if (!window.mermaid) {
            throw new Error('Mermaid library is not loaded');
        }

        const configObj = typeof config === 'string' ? JSON.parse(config) : (config || {});

        if (configObj.theme !== currentTheme || !window.__mermaidInitialized) {
            mermaid.initialize({
                startOnLoad: false,
                securityLevel: 'loose',
                ...configObj
            });
            currentTheme = configObj.theme || 'default';
            window.__mermaidInitialized = true;
        }

        const id = 'mermaid-' + Date.now() + '-' + Math.floor(Math.random() * 10000);
        const { svg } = await mermaid.render(id, code);
        return svg;
    } catch (error) {
        console.error('Mermaid rendering error:', error);
        throw new Error('Syntax error in diagram code: ' + (error.message || error));
    }
};

window.downloadMermaidSvg = function (filename, containerSelector) {
    try {
        const root = document.querySelector(containerSelector || '#mermaid-diagram');
        const svgElement = root && root.querySelector('svg');
        if (!svgElement) {
            throw new Error('No diagram to export');
        }

        const svgData = new XMLSerializer().serializeToString(svgElement);
        const blob = new Blob([svgData], { type: 'image/svg+xml' });
        const url = URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;
        link.download = filename || 'diagram.svg';
        link.click();
        URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Export error:', error);
        throw error;
    }
};

window.downloadMermaidPng = function (filename, scale, containerSelector) {
    try {
        const root = document.querySelector(containerSelector || '#mermaid-diagram');
        const svgElement = root && root.querySelector('svg');
        if (!svgElement) {
            throw new Error('No diagram to export');
        }

        scale = scale || 2;
        const bbox = svgElement.getBBox();
        const width = bbox.width || svgElement.width.baseVal.value || 800;
        const height = bbox.height || svgElement.height.baseVal.value || 600;

        const canvas = document.createElement('canvas');
        canvas.width = width * scale;
        canvas.height = height * scale;
        const ctx = canvas.getContext('2d');
        ctx.scale(scale, scale);
        ctx.fillStyle = 'white';
        ctx.fillRect(0, 0, width, height);

        const svgData = new XMLSerializer().serializeToString(svgElement);
        const img = new Image();
        const svgBlob = new Blob([svgData], { type: 'image/svg+xml;charset=utf-8' });
        const url = URL.createObjectURL(svgBlob);

        img.onload = function () {
            ctx.drawImage(img, 0, 0, width, height);
            URL.revokeObjectURL(url);
            canvas.toBlob(function (blob) {
                const pngUrl = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = pngUrl;
                link.download = filename || 'diagram.png';
                link.click();
                URL.revokeObjectURL(pngUrl);
            }, 'image/png');
        };

        img.src = url;
    } catch (error) {
        console.error('PNG export error:', error);
        throw error;
    }
};

window.downloadTextFile = function (content, filename, mimeType) {
    try {
        const blob = new Blob([content], { type: mimeType || 'text/plain;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename || 'download.txt';
        link.click();
        URL.revokeObjectURL(url);
    } catch (error) {
        console.error('Download error:', error);
        throw error;
    }
};

window.reinitializeMermaid = function (config) {
    const configObj = typeof config === 'string' ? JSON.parse(config) : (config || {});
    mermaid.initialize(configObj);
    currentTheme = configObj.theme || 'default';
    window.__mermaidInitialized = true;
};
