(function(){ "use strict";

// ══════════════════════════════════════════════════════════════════════
// i18n
// ══════════════════════════════════════════════════════════════════════

var i18n = {
    es: {
        uploadTitle: "Arrastra tu archivo de fuente aquí",
        uploadSub: "o haz clic para seleccionar",
        selectFile: "Seleccionar Archivo",
        searchPlaceholder: "Buscar por nombre, hex, código o categoría…",
        searchPlaceholderShort: "Buscar nombre, hex, categoría…",
        searchEmpty: "Escribe para buscar",
        searchNoResults: "Sin resultados",
        searchHintCat: "categoría: Mayúsculas",
        sortCategory: "Categoría",
        details: "Detalles",
        information: "Información",
        name: "Nombre",
        creator: "Creador",
        date: "Fecha de creación",
        license: "Disponibilidad de uso",
        settings: "Configuración",
        preferences: "Preferencias",
        language: "Idioma",
        themeDesc: "Apariencia",
        cacheDesc: "Datos",
        clearCacheSub: "Elimina preferencias",
        clearBtn: "Limpiar",
        preloadSub: "Precarga recursos",
        prefDesc: "Personaliza",
        sortOrder: "Orden",
        defaultColor: "Color",
        analyzing: "Analizando Fuente",
        copied: "Copiado",
        copy: "Copiar",
        close: "Cerrar",
        unsupported: "Formato no soportado",
        cacheCleaned: "Cache limpiado",
        preloadOn: "Preload activado",
        preloadOff: "Preload desactivado",
        sortSaved: "Orden guardado",
        noCreator: "No especificado",
        noDate: "No disponible",
        noLicense: "No especificada",
        allChars: "Todos los caracteres",
        nothingToCopy: "Nada que copiar",
        linkCopied: "Enlace copiado",
        langChanged: "Idioma cambiado",
        error: "Error",
        about: "Acerca de",
        aboutApp: "Visor de mapas de caracteres",
        aboutBody: "FontView es un visor de mapas de caracteres y texto. Permite ver todos los caracteres de una fuente (.ttf .otf .woff .woff2). Soporta Unicode, Hex, Emojis y otros símbolos.",
        createdBy: "Creado por",
        licenseLbl: "Licencia",
        tpPlaceholder: "Escribe texto para previsualizar…",
        tpEmpty: "Vista previa",
        voiceListening: "Escuchando…",
        voiceNotSupported: "Voz no soportada",
        voiceError: "Error de voz",
        saveAs: "Guardar como…",
        savedOk: "Guardado",
        bulkSelect: "Selección múltiple",
        exitBulk: "Salir de selección",
        selectAll: "Seleccionar todo",
        deselectAll: "Deseleccionar todo",
        selected: "seleccionados",
        downloadZip: "Descargar .zip",
        copyAll: "Copiar todo",
        allCopied: "Todo copiado",
        showPreview: "Vista previa detallada",
        fontOverview: "Vista general",
        charPreview: "Carácter",
        unicode: "Unicode",
        category: "Categoría",
        glyphName: "Nombre del glifo",
        decimal: "Decimal",
        htmlEntity: "Entidad HTML",
        utf8: "UTF-8",
        docked: "Acoplado",
        undocked: "Desacoplado"
    },
    en: {
        uploadTitle: "Drag your font file here",
        uploadSub: "or click to select",
        selectFile: "Select File",
        searchPlaceholder: "Search by name, hex, code or category…",
        searchPlaceholderShort: "Search name, hex, category…",
        searchEmpty: "Type to search",
        searchNoResults: "No results",
        searchHintCat: "category: Uppercase",
        sortCategory: "Category",
        details: "Details",
        information: "Information",
        name: "Name",
        creator: "Creator",
        date: "Creation date",
        license: "Usage availability",
        settings: "Settings",
        preferences: "Preferences",
        language: "Language",
        themeDesc: "Appearance",
        cacheDesc: "Data",
        clearCacheSub: "Remove preferences",
        clearBtn: "Clear",
        preloadSub: "Preload resources",
        prefDesc: "Customize",
        sortOrder: "Order",
        defaultColor: "Color",
        analyzing: "Analyzing Font",
        copied: "Copied",
        copy: "Copy",
        close: "Close",
        unsupported: "Unsupported format",
        cacheCleaned: "Cache cleared",
        preloadOn: "Preload enabled",
        preloadOff: "Preload disabled",
        sortSaved: "Sort saved",
        noCreator: "Not specified",
        noDate: "Not available",
        noLicense: "Not specified",
        allChars: "All characters",
        nothingToCopy: "Nothing to copy",
        linkCopied: "Link copied",
        langChanged: "Language changed",
        error: "Error",
        about: "About",
        aboutApp: "Character Map viewer",
        aboutBody: "FontView is a Character and Text Map viewer. It allows users to view all specific characters of a font through .ttf .otf .woff and .woff2 files. It also supports Unicode, Hex, Emojis, and other symbols.",
        createdBy: "Created by",
        licenseLbl: "License",
        tpPlaceholder: "Type text to preview…",
        tpEmpty: "Preview",
        voiceListening: "Listening…",
        voiceNotSupported: "Voice not supported",
        voiceError: "Voice error",
        saveAs: "Save as…",
        savedOk: "Saved",
        bulkSelect: "Bulk Select",
        exitBulk: "Exit Selection",
        selectAll: "Select All",
        deselectAll: "Deselect All",
        selected: "selected",
        downloadZip: "Download .zip",
        copyAll: "Copy All",
        allCopied: "All copied",
        showPreview: "Detailed Preview",
        fontOverview: "Font Overview",
        charPreview: "Character",
        unicode: "Unicode",
        category: "Category",
        glyphName: "Glyph Name",
        decimal: "Decimal",
        htmlEntity: "HTML Entity",
        utf8: "UTF-8",
        docked: "Docked",
        undocked: "Undocked"
    }
};

var lang = localStorage.getItem("fv-lang") || "es";
function t(k) { return (i18n[lang] && i18n[lang][k]) || i18n.es[k] || k; }
function applyI18n() {
    document.documentElement.lang = lang;
    document.querySelectorAll("[data-i18n]").forEach(function(el) {
        el.textContent = t(el.dataset.i18n);
    });
    document.querySelectorAll("[data-i18n-placeholder]").forEach(function(el) {
        el.placeholder = t(el.dataset.i18nPlaceholder);
    });
    var d = $("tpDisplay");
    if (d) d.setAttribute("data-placeholder", t("tpEmpty"));
}

// ══════════════════════════════════════════════════════════════════════
// State variables
// ══════════════════════════════════════════════════════════════════════

var fontFamily, fontN = 0, allChars = [], sects = {};
var uColor = false, sIdx = -1, scrollLock = false;
var meta = {}, currentSort = "default", ctxMenu = null;
var uploadPopupOpen = false, snavObserver = null;
var voiceRecognition = null, isListening = false, activeVoiceBtn = null;
var parsedFont = null;
var bulkMode = false, selectedChars = new Set();
var longPressTimer = null, longPressFired = false, lpStartX = 0, lpStartY = 0;
var sidebarOpen = false, isDocked = false, sidebarWidth = 380;

var prefs = {
    sort: localStorage.getItem("fv-pref-sort") || "default",
    color: localStorage.getItem("fv-pref-color") || null,
    size: localStorage.getItem("fv-pref-size") || "36",
    preload: localStorage.getItem("fv-pref-preload") === "true"
};

// ══════════════════════════════════════════════════════════════════════
// Utility functions
// ══════════════════════════════════════════════════════════════════════

function esc(s) {
    return s.replace(/[&<>"']/g, function(c) {
        return {"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"}[c];
    });
}

function isDarkFn() {
    return document.documentElement.getAttribute("data-theme") === "dark";
}

function $(id) { return document.getElementById(id); }

function copyText(tx) {
    try { navigator.clipboard.writeText(tx); } catch(e) {
        var a = document.createElement("textarea");
        a.value = tx;
        a.style.cssText = "position:fixed;opacity:0";
        document.body.appendChild(a);
        a.select();
        document.execCommand("copy");
        document.body.removeChild(a);
    }
}

function toast(m, typ) {
    var tc = $("toastC");
    if (!tc) return;
    var el = document.createElement("div");
    el.className = "toast";
    var isS = typ === "error" || typ === "success";
    var ic = typ === "error" ? "error" : typ === "success" ? "check_circle" : "add_circle";
    var ex = (!isS && typ && fontFamily)
        ? '<span class="tc" style="font-family:\'' + fontFamily + '\'">' + esc(typ) + '</span>'
        : '';
    el.innerHTML = '<span class="mi">' + ic + '</span><span>' + m + '</span>' + ex;
    tc.appendChild(el);
    setTimeout(function() {
        el.classList.add("out");
        setTimeout(function() { el.remove(); }, 300);
    }, 2500);
}

// ══════════════════════════════════════════════════════════════════════
// Theme
// ══════════════════════════════════════════════════════════════════════

function setTheme(dark, anim) {
    if (anim) document.documentElement.classList.add("theme-shift");
    document.documentElement.setAttribute("data-theme", dark ? "dark" : "light");
    localStorage.setItem("fv-t", dark ? "dark" : "light");
    var ti = $("themeIcon");
    if (ti) ti.textContent = dark ? "light_mode" : "dark_mode";
    if (!uColor) {
        var c = dark ? "#ffffff" : "#000000";
        var cp = $("colorPicker"), cl = $("colorLbl");
        if (cp) cp.value = c;
        if (cl) cl.textContent = c.toUpperCase();
        updateColors(c);
    }
    document.querySelectorAll(".theme-card").forEach(function(tc) {
        tc.classList.toggle("active", tc.dataset.themeVal === (dark ? "dark" : "light"));
    });
    if (anim) setTimeout(function() {
        document.documentElement.classList.remove("theme-shift");
    }, 600);
}

function updateColors(c) {
    document.querySelectorAll(".char-display").forEach(function(e) {
        e.style.color = c;
    });
    updateTp();
}

function updateTp() {
    var inp = $("tpInput"), d = $("tpDisplay");
    if (!inp || !d) return;
    var v = inp.value;
    if (v) {
        d.textContent = v;
        d.style.fontFamily = "'" + fontFamily + "'";
        d.style.fontSize = $("sizeSlider") ? $("sizeSlider").value + "px" : "36px";
        d.style.color = $("colorPicker") ? $("colorPicker").value : "";
    } else {
        d.textContent = "";
    }
}

// ══════════════════════════════════════════════════════════════════════
// Save utilities (PNG, SVG, BMP)
// ══════════════════════════════════════════════════════════════════════

function renderCharToCanvas(ch, sz) {
    sz = sz || 512;
    var c = document.createElement("canvas");
    c.width = sz; c.height = sz;
    var x = c.getContext("2d");
    x.fillStyle = isDarkFn() ? "#1a1a1a" : "#ffffff";
    x.fillRect(0, 0, sz, sz);
    x.fillStyle = $("colorPicker") ? $("colorPicker").value : (isDarkFn() ? "#ffffff" : "#000000");
    x.font = Math.round(sz * 0.625) + "px '" + fontFamily + "'";
    x.textAlign = "center";
    x.textBaseline = "middle";
    x.fillText(ch, sz / 2, sz / 2);
    return c;
}

function canvasToBlob(c) {
    return new Promise(function(resolve) {
        c.toBlob(function(b) { resolve(b); }, "image/png");
    });
}

function generateSVG(ch) {
    var col = $("colorPicker") ? $("colorPicker").value : (isDarkFn() ? "#fff" : "#000");
    var bg = isDarkFn() ? "#1a1a1a" : "#ffffff";
    var sz = 512, fs = 320, svg = null;

    if (parsedFont) {
        try {
            var adv = parsedFont.getAdvanceWidth(ch, fs);
            var asc = parsedFont.ascender;
            var desc = parsedFont.descender;
            var upm = parsedFont.unitsPerEm;
            var sc = fs / upm;
            var tH = (asc - desc) * sc;
            var x2 = (sz - adv) / 2;
            var y2 = (sz - tH) / 2 + asc * sc;
            var p = parsedFont.getPath(ch, x2, y2, fs);
            p.fill = col;
            p.stroke = null;
            svg = '<?xml version="1.0"?>\n<svg xmlns="http://www.w3.org/2000/svg" width="' + sz + '" height="' + sz + '" viewBox="0 0 ' + sz + ' ' + sz + '">\n<rect width="' + sz + '" height="' + sz + '" fill="' + bg + '"/>\n' + p.toSVG() + '\n</svg>';
        } catch(e) { svg = null; }
    }

    if (!svg) {
        svg = '<?xml version="1.0"?>\n<svg xmlns="http://www.w3.org/2000/svg" width="' + sz + '" height="' + sz + '" viewBox="0 0 ' + sz + ' ' + sz + '">\n<rect width="' + sz + '" height="' + sz + '" fill="' + bg + '"/>\n<text x="256" y="256" font-size="' + fs + '" fill="' + col + '" text-anchor="middle" dominant-baseline="central">' + esc(ch) + '</text>\n</svg>';
    }

    return svg;
}

function generateBMP(ch) {
    var c = renderCharToCanvas(ch);
    var ctx = c.getContext("2d");
    var img = ctx.getImageData(0, 0, c.width, c.height);
    var w = img.width, h = img.height;
    var rowB = w * 3;
    var rowS = Math.ceil(rowB / 4) * 4;
    var pad = rowS - rowB;
    var pxS = rowS * h;
    var fS = 54 + pxS;
    var buf = new ArrayBuffer(fS);
    var v = new DataView(buf);

    v.setUint8(0, 0x42); v.setUint8(1, 0x4D);
    v.setUint32(2, fS, true); v.setUint32(10, 54, true);
    v.setUint32(14, 40, true); v.setInt32(18, w, true);
    v.setInt32(22, -h, true); v.setUint16(26, 1, true);
    v.setUint16(28, 24, true); v.setUint32(34, pxS, true);
    v.setUint32(38, 2835, true); v.setUint32(42, 2835, true);

    var off = 54, d = img.data;
    for (var y = 0; y < h; y++) {
        for (var x = 0; x < w; x++) {
            var i = (y * w + x) * 4;
            v.setUint8(off++, d[i + 2]);
            v.setUint8(off++, d[i + 1]);
            v.setUint8(off++, d[i]);
        }
        for (var p2 = 0; p2 < pad; p2++) v.setUint8(off++, 0);
    }

    return buf;
}

function downloadBlob(blob, name) {
    var link = document.createElement("a");
    link.download = name;
    link.href = URL.createObjectURL(blob);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    setTimeout(function() { URL.revokeObjectURL(link.href); }, 200);
}

function savePNG(ch) {
    var hex = ch.codePointAt(0).toString(16).toUpperCase();
    canvasToBlob(renderCharToCanvas(ch)).then(function(b) {
        downloadBlob(b, "U+" + hex + ".png");
        toast(t("savedOk") + " PNG", "success");
    });
}

function saveSVG(ch) {
    var hex = ch.codePointAt(0).toString(16).toUpperCase();
    downloadBlob(
        new Blob([generateSVG(ch)], { type: "image/svg+xml" }),
        "U+" + hex + ".svg"
    );
    toast(t("savedOk") + " SVG", "success");
}

function saveBMP(ch) {
    var hex = ch.codePointAt(0).toString(16).toUpperCase();
    downloadBlob(
        new Blob([generateBMP(ch)], { type: "image/bmp" }),
        "U+" + hex + ".bmp"
    );
    toast(t("savedOk") + " BMP", "success");
}

// ══════════════════════════════════════════════════════════════════════
// Bulk ZIP download
// ══════════════════════════════════════════════════════════════════════

function downloadBulkZip() {
    var chars = Array.from(selectedChars);
    if (!chars.length) { toast(t("nothingToCopy"), "error"); return; }
    if (typeof JSZip === "undefined") { toast(t("error"), "error"); return; }

    var fmt = $("bulkFormat") ? $("bulkFormat").value : "png";
    var lov = $("loadOv");
    if (lov) lov.style.display = "flex";

    var zip = new JSZip();
    var folder = zip.folder((meta.name || "chars") + "_" + fmt.toUpperCase());
    var promises = [];

    chars.forEach(function(ch) {
        var cp = ch.codePointAt(0);
        var hx = cp.toString(16).toUpperCase();
        while (hx.length < 4) hx = "0" + hx;
        var nm = "U+" + hx;

        if (fmt === "png") {
            promises.push(canvasToBlob(renderCharToCanvas(ch)).then(function(b) {
                folder.file(nm + ".png", b);
            }));
        } else if (fmt === "svg") {
            folder.file(nm + ".svg", generateSVG(ch));
        } else {
            folder.file(nm + ".bmp", generateBMP(ch));
        }
    });

    Promise.all(promises).then(function() {
        return zip.generateAsync({ type: "blob" });
    }).then(function(content) {
        downloadBlob(content, (meta.name || "chars") + "_" + chars.length + ".zip");
        toast(t("savedOk") + " ZIP", "success");
        if (lov) lov.style.display = "none";
    }).catch(function() {
        toast(t("error"), "error");
        if (lov) lov.style.display = "none";
    });
}

// ══════════════════════════════════════════════════════════════════════
// Bulk mode
// ══════════════════════════════════════════════════════════════════════

function enterBulkMode() {
    bulkMode = true;
    selectedChars.clear();
    document.querySelectorAll(".char-card").forEach(function(c) {
        c.classList.add("selectable");
        c.classList.remove("selected");
    });
    $("bulkBar").style.display = "flex";
    $("bulkBar").classList.remove("hiding");
    var g = $("goUpBtn");
    if (g) g.classList.add("bulk-shift");
    updateBulkCount();
}

function exitBulkMode() {
    var bar = $("bulkBar");
    bar.classList.add("hiding");
    setTimeout(function() {
        bar.style.display = "none";
        bar.classList.remove("hiding");
    }, 250);
    bulkMode = false;
    selectedChars.clear();
    document.querySelectorAll(".char-card").forEach(function(c) {
        c.classList.remove("selectable", "selected");
    });
    var g = $("goUpBtn");
    if (g) g.classList.remove("bulk-shift");
}

function toggleCardSelection(card, ch) {
    if (selectedChars.has(ch)) {
        selectedChars.delete(ch);
        card.classList.remove("selected");
    } else {
        selectedChars.add(ch);
        card.classList.add("selected");
    }
    updateBulkCount();
}

function updateBulkCount() {
    var n = selectedChars.size;
    var el = $("bulkCount");
    var dl = $("bulkDownload");
    if (el) el.innerHTML = '<span>' + n + '</span> ' + t("selected");
    if (dl) dl.disabled = n === 0;
}

function selectAllChars() {
    selectedChars.clear();
    allChars.forEach(function(c) { selectedChars.add(c.c); });
    document.querySelectorAll(".char-card.selectable").forEach(function(c) {
        c.classList.add("selected");
    });
    updateBulkCount();
}

function deselectAllChars() {
    selectedChars.clear();
    document.querySelectorAll(".char-card.selectable").forEach(function(c) {
        c.classList.remove("selected");
    });
    updateBulkCount();
}

function copyAllChars() {
    if (!allChars.length) { toast(t("nothingToCopy"), "error"); return; }
    copyText(allChars.map(function(c) { return c.c; }).join(""));
    toast(t("allCopied") + " (" + allChars.length + ")", "success");
}

// ══════════════════════════════════════════════════════════════════════
// Dockable Preview Sidebar
// ══════════════════════════════════════════════════════════════════════

function findCharData(ch) {
    for (var i = 0; i < allChars.length; i++) {
        if (allChars[i].c === ch) return allChars[i];
    }
    return null;
}

function applyDockLayout() {
    var sb = $("previewSidebar");
    var ov = $("psOverlay");
    var w = sidebarWidth;

    if (isDocked && sidebarOpen) {
        ov.classList.remove("show");
        sb.classList.add("docked");
        document.body.classList.add("sidebar-docked");
        document.querySelector(".main").style.marginRight = w + "px";
        var hdr = $("appHeader");
        if (hdr) hdr.style.paddingRight = w + "px";
    } else {
        sb.classList.remove("docked");
        document.body.classList.remove("sidebar-docked");
        document.querySelector(".main").style.marginRight = "";
        var hdr2 = $("appHeader");
        if (hdr2) hdr2.style.paddingRight = "";
        if (sidebarOpen) ov.classList.add("show");
        else ov.classList.remove("show");
    }

    var dk = $("psDock");
    if (dk) {
        dk.classList.toggle("active", isDocked);
        dk.querySelector(".mi").textContent = isDocked ? "dock_to_left" : "dock_to_right";
    }
}

function openSidebar(mode, charData) {
    var body = $("psBody");
    var title = $("psTitle");
    var sb = $("previewSidebar");
    body.innerHTML = "";
    var col = $("colorPicker") ? $("colorPicker").value : "";
    sb.style.width = sidebarWidth + "px";

    // AUTO-DOCK: cuando se abre desde la página de caracteres
    if (!isDocked && allChars.length) {
        isDocked = true;
        localStorage.setItem("fv-dock", "1");
    }

    if (mode === "char" && charData) {
        title.textContent = t("charPreview");
        var cp = charData.u;
        var hex = charData.hex;
        var dec = cp;
        var htmlEnt = "&#" + cp + ";";

        var ub = [];
        if (cp < 0x80) { ub.push(cp); }
        else if (cp < 0x800) { ub.push(0xC0 | (cp >> 6)); ub.push(0x80 | (cp & 0x3F)); }
        else if (cp < 0x10000) { ub.push(0xE0 | (cp >> 12)); ub.push(0x80 | ((cp >> 6) & 0x3F)); ub.push(0x80 | (cp & 0x3F)); }
        else { ub.push(0xF0 | (cp >> 18)); ub.push(0x80 | ((cp >> 12) & 0x3F)); ub.push(0x80 | ((cp >> 6) & 0x3F)); ub.push(0x80 | (cp & 0x3F)); }
        var u8 = ub.map(function(b) { return "0x" + b.toString(16).toUpperCase(); }).join(" ");

        body.innerHTML = '<div class="ps-preview-box"><div class="ps-char" style="font-family:\'' + fontFamily + '\';color:' + col + '">' + esc(charData.c) + '</div></div>' +
            '<div class="ps-info"><div class="ps-name">' + esc(charData.n || charData.c) + '</div>' +
            '<div class="ps-meta"><span>U+' + hex + '</span><span class="ps-meta-tag">' + esc(charData.cat) + '</span></div></div>' +
            '<div class="ps-detail-grid">' +
            '<div class="ps-detail-row"><span class="mi">tag</span><div><div class="ps-dr-label">' + t("unicode") + '</div><div class="ps-dr-value">U+' + hex + '</div></div></div>' +
            '<div class="ps-detail-row"><span class="mi">pin</span><div><div class="ps-dr-label">' + t("decimal") + '</div><div class="ps-dr-value">' + dec + '</div></div></div>' +
            '<div class="ps-detail-row"><span class="mi">code</span><div><div class="ps-dr-label">' + t("htmlEntity") + '</div><div class="ps-dr-value">' + esc(htmlEnt) + '</div></div></div>' +
            '<div class="ps-detail-row"><span class="mi">memory</span><div><div class="ps-dr-label">' + t("utf8") + '</div><div class="ps-dr-value">' + u8 + '</div></div></div>' +
            '<div class="ps-detail-row"><span class="mi">label</span><div><div class="ps-dr-label">' + t("glyphName") + '</div><div class="ps-dr-value' + (charData.n ? '' : ' muted') + '">' + esc(charData.n || t("noCreator")) + '</div></div></div>' +
            '<div class="ps-detail-row"><span class="mi">category</span><div><div class="ps-dr-label">' + t("category") + '</div><div class="ps-dr-value">' + esc(charData.cat) + '</div></div></div>' +
            '</div>' +
            '<div class="ps-divider"></div>' +
            '<div class="ps-actions">' +
            '<div class="ps-action" id="psAcopy"><span class="mi">content_copy</span><span class="ps-action-label">' + t("copy") + '</span></div>' +
            '<div class="ps-action" id="psAshare"><span class="mi">share</span><span class="ps-action-label">Share</span></div>' +
            '<div class="ps-action" id="psAsave"><span class="mi">save_alt</span><span class="ps-action-label">' + t("saveAs") + '</span><span class="mi" style="font-size:16px;color:var(--text-tertiary)">expand_more</span></div>' +
            '<div class="ps-save-formats" id="psAfmts">' +
            '<div class="ps-save-fmt" data-fmt="png"><span class="mi">image</span>PNG<span class="ps-ext">.png</span></div>' +
            '<div class="ps-save-fmt" data-fmt="svg"><span class="mi">code</span>SVG<span class="ps-ext">.svg</span></div>' +
            '<div class="ps-save-fmt" data-fmt="bmp"><span class="mi">image</span>BMP<span class="ps-ext">.bmp</span></div>' +
            '</div>' +
            '<div class="ps-action" id="psAbulk"><span class="mi">select_all</span><span class="ps-action-label">' + t("bulkSelect") + '</span></div>' +
            '</div>';

        body.querySelector("#psAcopy").onclick = function() { copyText(charData.c); toast(t("copied"), charData.c); };
        body.querySelector("#psAshare").onclick = function() {
            if (navigator.share) navigator.share({ title: "U+" + hex, text: charData.c });
            else { copyText(charData.c); toast(t("copied"), "success"); }
        };
        body.querySelector("#psAsave").onclick = function() {
            body.querySelector("#psAfmts").classList.toggle("show");
        };
        body.querySelectorAll(".ps-save-fmt").forEach(function(btn) {
            btn.onclick = function() {
                var f = btn.dataset.fmt;
                if (f === "png") savePNG(charData.c);
                else if (f === "svg") saveSVG(charData.c);
                else saveBMP(charData.c);
                body.querySelector("#psAfmts").classList.remove("show");
            };
        });
        body.querySelector("#psAbulk").onclick = function() { closeSidebar(); enterBulkMode(); };

    } else if (mode === "bulk") {
        title.textContent = t("bulkSelect") + " (" + selectedChars.size + ")";
        var bChars = Array.from(selectedChars);
        var previewCh = bChars[0] || "";
        var previewCD = findCharData(previewCh);

        body.innerHTML = '<div class="ps-preview-box"><div class="ps-char" id="psBulkChar" style="font-family:\'' + fontFamily + '\';color:' + col + '">' + esc(previewCh) + '</div></div>' +
            '<div class="ps-info"><div class="ps-name" id="psBulkName">' + (previewCD ? esc(previewCD.n || previewCD.c) : "") + '</div>' +
            '<div class="ps-meta"><span>' + bChars.length + ' ' + t("selected") + '</span></div></div>' +
            '<div class="ps-bulk-list" id="psBulkList"></div>' +
            '<div class="ps-divider"></div>' +
            '<div class="ps-actions">' +
            '<div class="ps-action" id="psBcopy"><span class="mi">content_copy</span><span class="ps-action-label">' + t("copy") + ' (' + bChars.length + ')</span></div>' +
            '<div class="ps-action" id="psBshare"><span class="mi">share</span><span class="ps-action-label">Share</span></div>' +
            '<div class="ps-action" id="psBdl"><span class="mi">download</span><span class="ps-action-label">' + t("downloadZip") + '</span></div>' +
            '<div class="ps-action" id="psBexit"><span class="mi">deselect</span><span class="ps-action-label">' + t("exitBulk") + '</span></div>' +
            '</div>';

        var list = body.querySelector("#psBulkList");
        bChars.forEach(function(ch, idx) {
            var chip = document.createElement("div");
            chip.className = "ps-bulk-chip" + (idx === 0 ? " active" : "");
            chip.style.fontFamily = "'" + fontFamily + "'";
            chip.textContent = ch;
            chip.onclick = function() {
                list.querySelectorAll(".ps-bulk-chip").forEach(function(c2) { c2.classList.remove("active"); });
                chip.classList.add("active");
                var cd = findCharData(ch);
                body.querySelector("#psBulkChar").textContent = ch;
                body.querySelector("#psBulkName").textContent = cd ? (cd.n || cd.c) : "";
            };
            list.appendChild(chip);
        });

        body.querySelector("#psBcopy").onclick = function() {
            copyText(bChars.join(""));
            toast(t("copied") + " (" + bChars.length + ")", "success");
        };
        body.querySelector("#psBshare").onclick = function() {
            if (navigator.share) navigator.share({ title: "Characters", text: bChars.join("") });
            else { copyText(bChars.join("")); toast(t("copied"), "success"); }
        };
        body.querySelector("#psBdl").onclick = downloadBulkZip;
        body.querySelector("#psBexit").onclick = function() { closeSidebar(); exitBulkMode(); };

    } else {
        title.textContent = t("fontOverview");

        body.innerHTML = '<div class="ps-preview-box"><div class="ps-sample" style="font-family:\'' + fontFamily + '\';color:' + col + '">AaBbCcDd 0123</div></div>' +
            '<div class="ps-info"><div class="ps-name">' + esc(meta.name || "Font") + '</div>' +
            '<div class="ps-meta"><span>' + allChars.length + ' ' + t("allChars").toLowerCase() + '</span>' +
            (meta.sub ? '<span class="ps-meta-tag">' + esc(meta.sub) + '</span>' : '') +
            '</div></div>' +
            '<div class="ps-detail-grid">' +
            '<div class="ps-detail-row"><span class="mi">person</span><div><div class="ps-dr-label">' + t("creator") + '</div><div class="ps-dr-value' + (meta.creator ? '' : ' muted') + '">' + esc(meta.creator || t("noCreator")) + '</div></div></div>' +
            '<div class="ps-detail-row"><span class="mi">calendar_today</span><div><div class="ps-dr-label">' + t("date") + '</div><div class="ps-dr-value' + (meta.date ? '' : ' muted') + '">' + esc(meta.date || t("noDate")) + '</div></div></div>' +
            '<div class="ps-detail-row"><span class="mi">verified</span><div><div class="ps-dr-label">' + t("license") + '</div><div class="ps-dr-value' + (meta.license ? '' : ' muted') + '">' + esc(meta.license || t("noLicense")) + '</div></div></div>' +
            '</div>' +
            '<div class="ps-divider"></div>' +
            '<div class="ps-actions">' +
            '<div class="ps-action" id="psFcopy"><span class="mi">copy_all</span><span class="ps-action-label">' + t("copyAll") + '</span></div>' +
            '<div class="ps-action" id="psFshare"><span class="mi">share</span><span class="ps-action-label">Share</span></div>' +
            '<div class="ps-action" id="psFbulk"><span class="mi">select_all</span><span class="ps-action-label">' + t("bulkSelect") + '</span></div>' +
            '</div>';

        body.querySelector("#psFcopy").onclick = copyAllChars;
        body.querySelector("#psFshare").onclick = function() {
            if (navigator.share) navigator.share({ title: "FontView", url: location.href });
            else { copyText(location.href); toast(t("linkCopied"), "success"); }
        };
        body.querySelector("#psFbulk").onclick = function() { closeSidebar(); enterBulkMode(); };
    }

    sb.classList.add("open");
    sidebarOpen = true;
    applyDockLayout();
}

function closeSidebar() {
    $("previewSidebar").classList.remove("open");
    $("psOverlay").classList.remove("show");
    sidebarOpen = false;
    document.body.classList.remove("sidebar-docked");
    document.querySelector(".main").style.marginRight = "";
    var hdr = $("appHeader");
    if (hdr) hdr.style.paddingRight = "";
}

function toggleDock() {
    isDocked = !isDocked;
    localStorage.setItem("fv-dock", isDocked ? "1" : "0");
    applyDockLayout();
    toast(isDocked ? t("docked") : t("undocked"), "success");
}

// ══════════════════════════════════════════════════════════════════════
// Resize handle
// ══════════════════════════════════════════════════════════════════════

function initResize() {
    var handle = $("psResize");
    var sb = $("previewSidebar");
    var ind = $("psWidthInd");
    var dragging = false, startX = 0, startW = 0;

    function onDown(e) {
        e.preventDefault();
        dragging = true;
        handle.classList.add("active");
        sb.classList.add("no-transition");
        startX = e.clientX || e.touches[0].clientX;
        startW = sb.offsetWidth;
        ind.classList.add("show");
        ind.textContent = startW + "px";
        document.addEventListener("mousemove", onMove);
        document.addEventListener("mouseup", onUp);
        document.addEventListener("touchmove", onMove, { passive: false });
        document.addEventListener("touchend", onUp);
    }

    function onMove(e) {
        if (!dragging) return;
        if (e.cancelable) e.preventDefault();
        var cx = e.clientX || (e.touches && e.touches[0] ? e.touches[0].clientX : 0);
        var diff = startX - cx;
        var nw = Math.max(280, Math.min(startW + diff, window.innerWidth * 0.8));
        sidebarWidth = nw;
        sb.style.width = nw + "px";
        ind.textContent = Math.round(nw) + "px";
        if (isDocked) applyDockLayout();
    }

    function onUp() {
        dragging = false;
        handle.classList.remove("active");
        sb.classList.remove("no-transition");
        ind.classList.remove("show");
        localStorage.setItem("fv-sw", String(Math.round(sidebarWidth)));
        document.removeEventListener("mousemove", onMove);
        document.removeEventListener("mouseup", onUp);
        document.removeEventListener("touchmove", onMove);
        document.removeEventListener("touchend", onUp);
    }

    handle.addEventListener("mousedown", onDown);
    handle.addEventListener("touchstart", onDown, { passive: false });
}

// ══════════════════════════════════════════════════════════════════════
// Unicode blocks
// ══════════════════════════════════════════════════════════════════════

function getCat(u) {
    if (u < 0x20 || u === 0x20 || u === 0xa0 || u === 0x200b || u === 0xfeff) return null;

    var blocks = [
        [0x0021, 0x007e, "Basic Latin"], [0x00a1, 0x00ff, "Latin-1 Supplement"],
        [0x0100, 0x024f, "Latin Extended"], [0x0250, 0x02af, "IPA Extensions"],
        [0x02b0, 0x02ff, "Spacing Modifiers"], [0x0300, 0x036f, "Combining Diacriticals"],
        [0x0370, 0x03ff, "Greek & Coptic"], [0x0400, 0x04ff, "Cyrillic"],
        [0x0500, 0x052f, "Cyrillic Supplement"], [0x0530, 0x058f, "Armenian"],
        [0x0590, 0x05ff, "Hebrew"], [0x0600, 0x06ff, "Arabic"],
        [0x0700, 0x074f, "Syriac"], [0x0780, 0x07bf, "Thaana"],
        [0x0900, 0x097f, "Devanagari"], [0x0980, 0x09ff, "Bengali"],
        [0x0a00, 0x0a7f, "Gurmukhi"], [0x0a80, 0x0aff, "Gujarati"],
        [0x0b00, 0x0b7f, "Oriya"], [0x0b80, 0x0bff, "Tamil"],
        [0x0c00, 0x0c7f, "Telugu"], [0x0c80, 0x0cff, "Kannada"],
        [0x0d00, 0x0d7f, "Malayalam"], [0x0d80, 0x0dff, "Sinhala"],
        [0x0e00, 0x0e7f, "Thai"], [0x0e80, 0x0eff, "Lao"],
        [0x0f00, 0x0fff, "Tibetan"], [0x1000, 0x109f, "Myanmar"],
        [0x10a0, 0x10ff, "Georgian"], [0x1100, 0x11ff, "Hangul Jamo"],
        [0x1200, 0x137f, "Ethiopic"], [0x13a0, 0x13ff, "Cherokee"],
        [0x1400, 0x167f, "Canadian Aboriginal"], [0x1680, 0x169f, "Ogham"],
        [0x16a0, 0x16ff, "Runic"], [0x1780, 0x17ff, "Khmer"],
        [0x1800, 0x18af, "Mongolian"], [0x1e00, 0x1eff, "Latin Extended Additional"],
        [0x1f00, 0x1fff, "Greek Extended"], [0x2000, 0x206f, "General Punctuation"],
        [0x2070, 0x209f, "Super & Subscripts"], [0x20a0, 0x20cf, "Currency Symbols"],
        [0x2100, 0x214f, "Letterlike Symbols"], [0x2150, 0x218f, "Number Forms"],
        [0x2190, 0x21ff, "Arrows"], [0x2200, 0x22ff, "Math Operators"],
        [0x2300, 0x23ff, "Technical"], [0x2400, 0x243f, "Control Pictures"],
        [0x2460, 0x24ff, "Enclosed Alphanumerics"], [0x2500, 0x257f, "Box Drawing"],
        [0x2580, 0x259f, "Block Elements"], [0x25a0, 0x25ff, "Geometric Shapes"],
        [0x2600, 0x26ff, "Misc Symbols"], [0x2700, 0x27bf, "Dingbats"],
        [0x2800, 0x28ff, "Braille"], [0x2b00, 0x2bff, "Misc Symbols & Arrows"],
        [0x2e80, 0x2eff, "CJK Radicals"], [0x3000, 0x303f, "CJK Symbols"],
        [0x3040, 0x309f, "Hiragana"], [0x30a0, 0x30ff, "Katakana"],
        [0x3100, 0x312f, "Bopomofo"], [0x3130, 0x318f, "Hangul Compat Jamo"],
        [0x3200, 0x32ff, "Enclosed CJK"], [0x3300, 0x33ff, "CJK Compatibility"],
        [0x3400, 0x4dbf, "CJK Ext A"], [0x4e00, 0x9fff, "CJK Ideographs"],
        [0xa000, 0xa48f, "Yi Syllables"], [0xac00, 0xd7af, "Hangul Syllables"],
        [0xe000, 0xf8ff, "Private Use"], [0xf900, 0xfaff, "CJK Compat Ideographs"],
        [0xfb00, 0xfb06, "Alphabetic Presentation"], [0xfb50, 0xfdff, "Arabic Presentation A"],
        [0xfe00, 0xfe0f, "Variation Selectors"], [0xfe30, 0xfe4f, "CJK Compat Forms"],
        [0xfe70, 0xfeff, "Arabic Presentation B"], [0xff00, 0xffef, "Halfwidth & Fullwidth"],
        [0x1d400, 0x1d7ff, "Math Alphanumeric"], [0x1f300, 0x1f5ff, "Misc Symbols & Pictographs"],
        [0x1f600, 0x1f64f, "Emoticons"], [0x1f680, 0x1f6ff, "Transport & Map"],
        [0x1f900, 0x1f9ff, "Supplemental Symbols"], [0x1fa70, 0x1faff, "Symbols Extended-A"]
    ];

    for (var i = 0; i < blocks.length; i++) {
        if (u >= blocks[i][0] && u <= blocks[i][1]) return blocks[i][2];
    }
    if (u >= 0x10000) return "Supplementary";
    return "Other";
}

// ══════════════════════════════════════════════════════════════════════
// Sorting
// ══════════════════════════════════════════════════════════════════════

function sortCats() {
    return Object.keys(sects).sort(function(a, b) {
        return Object.keys(sects).indexOf(a) - Object.keys(sects).indexOf(b);
    });
}

function isFlatSort() {
    return ["az", "za", "unicode", "unicode-desc"].indexOf(currentSort) !== -1;
}

function getSortedChars() {
    var c = allChars.slice();
    var l = lang === "en" ? "en" : "es";
    if (currentSort === "az") c.sort(function(a, b) { return a.c.localeCompare(b.c, l); });
    else if (currentSort === "za") c.sort(function(a, b) { return b.c.localeCompare(a.c, l); });
    else if (currentSort === "unicode") c.sort(function(a, b) { return a.u - b.u; });
    else if (currentSort === "unicode-desc") c.sort(function(a, b) { return b.u - a.u; });
    return c;
}

// ══════════════════════════════════════════════════════════════════════
// Render
// ══════════════════════════════════════════════════════════════════════

function render() {
    var sec = $("sections");
    if (!sec) return;
    sec.innerHTML = "";
    if (isFlatSort()) { renderFlat(); return; }
    var cc = $("colorPicker").value;
    var cs = $("sizeSlider").value + "px";

    sortCats().forEach(function(cat) {
        var s = document.createElement("div");
        s.className = "char-section";
        s.id = "s-" + cat.replace(/[^a-zA-Z0-9]/g, "-");
        s.dataset.cat = cat;
        s.innerHTML = '<div class="section-header"><span class="mi">expand_more</span><h3>' + esc(cat) + '</h3><span class="section-count">' + sects[cat].length + '</span></div><div class="grid-wrap"><div class="grid-inner"><div class="char-grid"></div></div></div>';
        s.querySelector(".section-header").onclick = function() { s.classList.toggle("collapsed"); };
        var g = s.querySelector(".char-grid");
        var f = document.createDocumentFragment();
        sects[cat].forEach(function(ch) { f.appendChild(mkCard(ch, cc, cs)); });
        g.appendChild(f);
        sec.appendChild(s);
    });
}

function renderFlat() {
    var chars = getSortedChars();
    var sec = $("sections");
    var s = document.createElement("div");
    s.className = "char-section";
    s.innerHTML = '<div class="section-header"><span class="mi">expand_more</span><h3>' + t("allChars") + '</h3><span class="section-count">' + chars.length + '</span></div><div class="grid-wrap"><div class="grid-inner"><div class="char-grid"></div></div></div>';
    s.querySelector(".section-header").onclick = function() { s.classList.toggle("collapsed"); };
    var g = s.querySelector(".char-grid");
    var cc = $("colorPicker").value;
    var cs = $("sizeSlider").value + "px";
    var f = document.createDocumentFragment();
    chars.forEach(function(ch) { f.appendChild(mkCard(ch, cc, cs)); });
    g.appendChild(f);
    sec.appendChild(s);
}

function mkCard(ch, cc, cs) {
    var card = document.createElement("div");
    card.className = "char-card" + (bulkMode ? " selectable" : "");
    card.dataset.char = ch.c;
    card.innerHTML = '<span class="char-display" style="font-family:\'' + fontFamily + '\';color:' + cc + ';font-size:' + cs + '">' + esc(ch.c) + '</span><span class="char-code">U+' + ch.hex + '</span><div class="cov"><span class="mi" style="font-size:12px;color:inherit">check</span>OK</div>';

    if (bulkMode && selectedChars.has(ch.c)) card.classList.add("selected");

    card.onclick = function() {
        if (longPressFired) { longPressFired = false; return; }
        if (bulkMode) { toggleCardSelection(card, ch.c); return; }
        copyText(ch.c);
        card.classList.add("copied");
        toast(t("copied"), ch.c);
        setTimeout(function() { card.classList.remove("copied"); }, 600);
    };

    return card;
}

// ══════════════════════════════════════════════════════════════════════
// Navigation arrows
// ══════════════════════════════════════════════════════════════════════

function setupNavArrows(scrollEl, arrowL, arrowR, fadeL, fadeR) {
    if (!scrollEl) return;

    function update() {
        var cL = scrollEl.scrollLeft > 6;
        var cR = scrollEl.scrollLeft < scrollEl.scrollWidth - scrollEl.clientWidth - 6;
        if (arrowL) arrowL.classList.toggle("show", cL);
        if (arrowR) arrowR.classList.toggle("show", cR);
        if (fadeL) fadeL.classList.toggle("show", cL);
        if (fadeR) fadeR.classList.toggle("show", cR);
    }

    scrollEl.addEventListener("scroll", update, { passive: true });
    window.addEventListener("resize", update, { passive: true });
    if (arrowL) arrowL.onclick = function() { scrollEl.scrollBy({ left: -160, behavior: "smooth" }); };
    if (arrowR) arrowR.onclick = function() { scrollEl.scrollBy({ left: 160, behavior: "smooth" }); };
    setTimeout(update, 100);
    return update;
}

var updateInlineArrows = null;
var updateHeaderArrows = null;

function buildNav() {
    var sn = $("snav");
    var hs = $("headerSnav");
    var hsw = $("headerSnavWrap");
    if (!sn || !hs) return;
    sn.innerHTML = "";
    hs.innerHTML = "";
    if (hsw) hsw.classList.remove("visible");

    if (isFlatSort()) {
        var p = document.createElement("button");
        p.className = "npill active";
        p.textContent = t("allChars") + " (" + allChars.length + ")";
        sn.appendChild(p);
        if (updateInlineArrows) setTimeout(updateInlineArrows, 50);
        return;
    }

    sortCats().forEach(function(cat) {
        function mk() {
            var pill = document.createElement("button");
            pill.className = "npill";
            pill.textContent = cat;
            pill.onclick = function() {
                scrollLock = true;
                document.querySelectorAll(".npill").forEach(function(x) {
                    x.classList.toggle("active", x.textContent === cat);
                });
                goSec(cat);
            };
            return pill;
        }
        sn.appendChild(mk());
        hs.appendChild(mk());
    });

    if (!updateInlineArrows) {
        updateInlineArrows = setupNavArrows($("snav"), $("sNavL"), $("sNavR"), $("sFadeL"), $("sFadeR"));
    } else {
        setTimeout(updateInlineArrows, 50);
    }
    if (!updateHeaderArrows) {
        updateHeaderArrows = setupNavArrows($("headerSnav"), $("hsNavL"), $("hsNavR"), $("hsFadeL"), $("hsFadeR"));
    } else {
        setTimeout(updateHeaderArrows, 50);
    }
}

function goSec(cat) {
    var el = document.getElementById("s-" + cat.replace(/[^a-zA-Z0-9]/g, "-"));
    if (!el) return;
    var hdr = $("appHeader");
    var hh = hdr ? hdr.offsetHeight : 60;
    window.scrollTo({ top: el.getBoundingClientRect().top + window.pageYOffset - hh - 10, behavior: "smooth" });
    clearTimeout(goSec._t);
    goSec._t = setTimeout(function() { scrollLock = false; }, 700);
}

function setupStickyNav() {
    if (snavObserver) snavObserver.disconnect();
    var hsw = $("headerSnavWrap");
    var sw = $("snavWrap");
    var hdr = $("appHeader");
    var cnt = $("content");

    if (!hsw || !sw || !hdr || !allChars.length || isFlatSort()) {
        if (hsw) hsw.classList.remove("visible");
        return;
    }

    snavObserver = new IntersectionObserver(function(entries) {
        entries.forEach(function(e) {
            var show = !e.isIntersecting && cnt && cnt.style.display !== "none";
            hsw.classList.toggle("visible", show);
            if (show && updateHeaderArrows) setTimeout(updateHeaderArrows, 50);
        });
    }, { rootMargin: "-" + hdr.offsetHeight + "px 0px 0px 0px", threshold: 0 });
    snavObserver.observe(sw);
}

// ══════════════════════════════════════════════════════════════════════
// Voice search
// ══════════════════════════════════════════════════════════════════════

function stopVoice() {
    if (isListening && voiceRecognition) voiceRecognition.stop();
}

function toggleVoice(btn) {
    btn = btn || $("searchVoiceBtn");
    var inp = $("searchIn");
    if (!btn || !inp) return;

    if (isListening && voiceRecognition) { voiceRecognition.stop(); return; }

    var SR = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!SR) { toast(t("voiceNotSupported"), "error"); return; }

    voiceRecognition = new SR();
    voiceRecognition.continuous = false;
    voiceRecognition.interimResults = true;
    voiceRecognition.maxAlternatives = 1;
    voiceRecognition.lang = lang === "en" ? "en-US" : "es-ES";

    activeVoiceBtn = btn;
    isListening = true;
    btn.classList.add("listening");
    toast(t("voiceListening"), "success");

    voiceRecognition.onresult = function(ev) {
        var tr = "";
        for (var i = ev.resultIndex; i < ev.results.length; i++) {
            tr += ev.results[i][0].transcript;
        }
        inp.value = tr;
        sIdx = -1;
        renderSR(tr);
    };

    voiceRecognition.onerror = function(ev) {
        isListening = false;
        if (activeVoiceBtn) activeVoiceBtn.classList.remove("listening");
        if (ev.error !== "aborted" && ev.error !== "no-speech") {
            toast(t("voiceError"), "error");
        }
    };

    voiceRecognition.onend = function() {
        isListening = false;
        if (activeVoiceBtn) activeVoiceBtn.classList.remove("listening");
    };

    try { voiceRecognition.start(); } catch(e) {
        isListening = false;
        btn.classList.remove("listening");
        toast(t("voiceError"), "error");
    }
}

// ══════════════════════════════════════════════════════════════════════
// Search
// ══════════════════════════════════════════════════════════════════════

function renderSR(q) {
    var sr = $("searchRes");
    if (!sr) return;
    q = (q || "").trim();
    if (!q) {
        sr.innerHTML = '<div class="search-empty"><span class="mi">font_download</span><p>' + t("searchEmpty") + '</p></div>';
        return;
    }

    var ql = q.toLowerCase();
    var hq = q.replace(/^(0x|u\+|U\+|#)/, "").toLowerCase();
    var isH = /^[0-9a-f]{1,6}$/i.test(hq) && hq.length >= 2;

    var r = allChars.filter(function(c) {
        return c.c.toLowerCase().indexOf(ql) !== -1 ||
            c.n.toLowerCase().indexOf(ql) !== -1 ||
            c.cat.toLowerCase().indexOf(ql) !== -1 ||
            ("u+" + c.hex.toLowerCase()).indexOf(ql) !== -1 ||
            (isH && c.hex.toLowerCase().indexOf(hq) !== -1);
    }).slice(0, 120);

    if (!r.length) {
        sr.innerHTML = '<div class="search-empty"><span class="mi">search_off</span><p>' + t("searchNoResults") + '</p></div>';
        return;
    }

    var groups = {}, co = [];
    r.forEach(function(c) {
        if (!groups[c.cat]) { groups[c.cat] = []; co.push(c.cat); }
        groups[c.cat].push(c);
    });

    var cc = $("colorPicker") ? $("colorPicker").value : "#000";
    var html = "";
    co.forEach(function(cat) {
        html += '<div class="sr-group-header"><span class="sr-group-title">' + esc(cat) + '</span><span class="sr-group-count">' + groups[cat].length + '</span><div class="sr-group-line"></div></div>';
        groups[cat].forEach(function(c) {
            html += '<div class="sr-item" data-c="' + esc(c.c) + '">' +
                '<div class="sr-char" style="font-family:\'' + fontFamily + '\';color:' + cc + '">' + esc(c.c) + '</div>' +
                '<div class="sr-info"><div class="sr-name">' + esc(c.n || c.c) + '</div>' +
                '<div class="sr-code">U+' + c.hex + '</div></div>' +
                '<span class="sr-cat">' + esc(c.cat) + '</span></div>';
        });
    });

    sr.innerHTML = html;
    sr.querySelectorAll(".sr-item").forEach(function(it) {
        it.onclick = function() {
            copyText(it.dataset.c);
            toast(t("copied"), it.dataset.c);
        };
    });
}

function closeModal(id) {
    var m = $(id);
    if (!m) return;
    m.classList.add("closing");
    setTimeout(function() {
        m.style.display = "none";
        m.classList.remove("closing");
    }, 200);
}

// ══════════════════════════════════════════════════════════════════════
// Upload popup analyzing state
// ══════════════════════════════════════════════════════════════════════

function setUploadPopupAnalyzing(on, fileName) {
    var drop = $("uploadPopupDrop");
    var btn = $("uploadPopupSelect");
    if (!drop) return;

    if (on) {
        drop._origHTML = drop.innerHTML;
        drop.innerHTML =
            '<div class="upload-popup-icon upi-spin">' +
                '<span class="mi">sync</span>' +
            '</div>' +
            '<div class="upload-popup-analyzing-wrap">' +
                '<div class="upload-popup-title">' + t("analyzing") + '</div>' +
                '<div class="upload-popup-sub upload-popup-filename">' + esc(fileName || "") + '</div>' +
            '</div>' +
            '<div class="upload-popup-progress">' +
                '<div class="upload-popup-progress-bar"></div>' +
            '</div>';
        drop.classList.add("analyzing");
        if (btn) btn.style.display = "none";
    } else {
        if (drop._origHTML) {
            drop.innerHTML = drop._origHTML;
            drop._origHTML = null;
        }
        drop.classList.remove("analyzing");
        if (btn) btn.style.display = "";
        uploadPopupOpen = false;
        var popup = $("uploadPopup");
        if (popup) popup.style.display = "none";
    }
}

// ══════════════════════════════════════════════════════════════════════
// Load font
// ══════════════════════════════════════════════════════════════════════

function loadFont(file) {
    var ext = "." + file.name.split(".").pop().toLowerCase();
    if ([".ttf", ".otf", ".woff", ".woff2"].indexOf(ext) === -1) {
        toast(t("unsupported"), "error");
        return;
    }

    if (bulkMode) exitBulkMode();
    if (sidebarOpen) closeSidebar();

    var lov = $("loadOv");
    if (lov) lov.style.display = "flex";

    // Mostrar estado analizando en el popup
    setUploadPopupAnalyzing(true, file.name);

    file.arrayBuffer().then(function(ab) {
        fontN++;
        fontFamily = "fv-font-" + fontN;
        var font;

        try { font = opentype.parse(ab); } catch(err) {
            console.error(err);
            toast(t("error"), "error");
            if (lov) lov.style.display = "none";
            setUploadPopupAnalyzing(false);
            return;
        }

        parsedFont = font;
        var ff = new FontFace(fontFamily, ab.slice(0));

        return ff.load().then(function() {
            document.fonts.add(ff);
            allChars = [];
            sects = {};

            for (var i = 0; i < font.glyphs.length; i++) {
                var g = font.glyphs.get(i);
                if (g.unicode == null) continue;
                var cat = getCat(g.unicode);
                if (!cat) continue;
                var hex = g.unicode.toString(16).toUpperCase();
                while (hex.length < 4) hex = "0" + hex;
                var obj = { c: String.fromCodePoint(g.unicode), u: g.unicode, n: g.name || "", cat: cat, hex: hex };
                allChars.push(obj);
                if (!sects[cat]) sects[cat] = [];
                sects[cat].push(obj);
            }

            Object.keys(sects).forEach(function(k) {
                sects[k].sort(function(a, b) { return a.u - b.u; });
            });

            var nm = font.names;
            var gn = function(v) {
                if (!v) return null;
                if (typeof v === "string") return v;
                return v.en || Object.values(v)[0] || null;
            };

            var fn = gn(nm.fontFamily) || gn(nm.fullName) || file.name;
            var sf = gn(nm.fontSubfamily) || "";
            var cd = null;

            try {
                if (font.tables && font.tables.head && font.tables.head.created) {
                    var ep = new Date(1904, 0, 1);
                    var d = new Date(ep.getTime() + font.tables.head.created * 1000);
                    if (d.getFullYear() > 1970) {
                        cd = d.toLocaleDateString(lang === "en" ? "en-US" : "es-ES", { year: "numeric", month: "long", day: "numeric" });
                    }
                }
            } catch(e) {}

            meta = {
                name: fn, sub: sf,
                creator: gn(nm.designer) || gn(nm.manufacturer) || null,
                date: cd,
                license: gn(nm.license) || gn(nm.licenseURL) || null,
                size: Math.round(file.size / 1024)
            };

            var fne = $("fontName"), fme = $("fontMeta");
            if (fne) fne.textContent = fn;
            if (fme) fme.textContent = (sf || "Regular") + " · " + allChars.length + " car. · " + meta.size + " KB";

            currentSort = prefs.sort;
            document.querySelectorAll(".sort-pill").forEach(function(p) {
                p.classList.toggle("active", p.dataset.sort === currentSort);
            });

            var cp = $("colorPicker"), cl = $("colorLbl"), ss = $("sizeSlider"), sl = $("sizeLbl");
            if (prefs.color && cp) { uColor = true; cp.value = prefs.color; if (cl) cl.textContent = prefs.color.toUpperCase(); }
            if (prefs.size && ss) { ss.value = prefs.size; if (sl) sl.textContent = prefs.size + "px"; }

            render();
            buildNav();

            var uz = $("uploadZone"), cnt = $("content"), tb = $("toolbar"), upb = $("uploadPopupBtn");
            if (uz) uz.style.display = "none";
            if (cnt) { cnt.style.display = "block"; cnt.style.animation = "fadeUp .5s cubic-bezier(.16,1,.3,1)"; }
            if (tb) { tb.classList.add("visible"); tb.style.animation = "fadeUp .3s cubic-bezier(.16,1,.3,1)"; }
            if (upb) upb.style.display = "flex";

            var tpd = $("tpDisplay");
            if (tpd) { tpd.style.fontFamily = "'" + fontFamily + "'"; tpd.setAttribute("data-placeholder", t("tpEmpty")); }
            var tpi = $("tpInput");
            if (tpi) tpi.value = "";
            updateTp();
            setTimeout(setupStickyNav, 200);
            if (lov) lov.style.display = "none";
            setUploadPopupAnalyzing(false);
        });
    });
}

// ══════════════════════════════════════════════════════════════════════
// Context menu
// ══════════════════════════════════════════════════════════════════════

function showContextMenu(x, y, card) {
    if (ctxMenu) { ctxMenu.remove(); ctxMenu = null; }

    var ch = card ? card.dataset.char : null;
    var cd = ch ? findCharData(ch) : null;
    var hasFont = fontFamily && allChars.length;
    var items = [];

    items.push({ i: "content_copy", l: t("copy"), a: function() {
        if (ch) { copyText(ch); toast(t("copied"), ch); }
        else {
            var s = window.getSelection();
            var tx = s && s.toString() ? s.toString() : "";
            if (tx) { copyText(tx); toast(t("copied"), "success"); }
            else toast(t("nothingToCopy"), "error");
        }
    }});

    if (ch && hasFont) {
        items.push({ i: "save_alt", l: t("saveAs"), sub: [
            { i: "image", l: "PNG", ext: "PNG", a: function() { savePNG(ch); } },
            { i: "code", l: "SVG", ext: "SVG", a: function() { saveSVG(ch); } },
            { i: "image", l: "BMP", ext: "BMP", a: function() { saveBMP(ch); } }
        ]});
    }

    if (hasFont) { items.push({ i: "copy_all", l: t("copyAll"), a: copyAllChars }); }

    if (hasFont) {
        if (bulkMode && selectedChars.size > 0) {
            items.push({ i: "preview", l: t("showPreview"), a: function() { openSidebar("bulk"); } });
        } else if (ch && cd) {
            items.push({ i: "preview", l: t("showPreview"), a: function() { openSidebar("char", cd); } });
        } else {
            items.push({ i: "preview", l: t("showPreview"), a: function() { openSidebar("font"); } });
        }
    }

    if (hasFont) {
        items.push({ i: bulkMode ? "deselect" : "select_all", l: bulkMode ? t("exitBulk") : t("bulkSelect"), a: function() {
            if (bulkMode) exitBulkMode(); else enterBulkMode();
        }});
    }

    items.push({ i: "share", l: "Share", a: function() {
        if (navigator.share) navigator.share({ title: "FontView", url: location.href });
        else { copyText(location.href); toast(t("linkCopied"), "success"); }
    }});

    ctxMenu = document.createElement("div");
    ctxMenu.className = "ctx-menu";

    items.forEach(function(item, idx) {
        if (idx > 0) {
            var sep = document.createElement("div");
            sep.className = "ctx-sep";
            ctxMenu.appendChild(sep);
        }

        var el = document.createElement("div");
        el.className = "ctx-menu-item" + (item.sub ? " has-sub" : "");

        if (item.sub) {
            el.innerHTML = '<span class="mi">' + item.i + '</span><span>' + item.l + '</span><span class="mi ctx-arrow">chevron_right</span>';
            var submenu = document.createElement("div");
            submenu.className = "ctx-submenu";
            item.sub.forEach(function(si) {
                var se = document.createElement("div");
                se.className = "ctx-menu-item";
                se.innerHTML = '<span class="mi">' + si.i + '</span><span>' + si.l + '</span><span class="ctx-sub-ext">.' + si.ext.toLowerCase() + '</span>';
                se.onclick = function(ev) {
                    ev.stopPropagation();
                    ctxMenu.remove(); ctxMenu = null;
                    si.a();
                };
                submenu.appendChild(se);
            });
            el.appendChild(submenu);
            el.onclick = function(ev) { ev.stopPropagation(); el.classList.toggle("open"); };
        } else {
            el.innerHTML = '<span class="mi">' + item.i + '</span><span>' + item.l + '</span>';
            el.onclick = function() { ctxMenu.remove(); ctxMenu = null; item.a(); };
        }

        ctxMenu.appendChild(el);
    });

    document.body.appendChild(ctxMenu);

    var mw = ctxMenu.offsetWidth;
    var mh = ctxMenu.offsetHeight;
    if (x + mw > innerWidth) x = innerWidth - mw - 8;
    if (y + mh > innerHeight) y = innerHeight - mh - 8;
    ctxMenu.style.left = Math.max(4, x) + "px";
    ctxMenu.style.top = Math.max(4, y) + "px";

    var sub = ctxMenu.querySelector(".ctx-submenu");
    if (sub) {
        requestAnimationFrame(function() {
            if (sub.getBoundingClientRect().right > innerWidth) sub.classList.add("flip");
        });
    }
}

// ══════════════════════════════════════════════════════════════════════
// INIT
// ══════════════════════════════════════════════════════════════════════

function init() {
    var sv = localStorage.getItem("fv-t");
    if (sv === "dark") document.documentElement.setAttribute("data-theme", "dark");
    else if (!sv && matchMedia("(prefers-color-scheme:dark)").matches) {
        document.documentElement.setAttribute("data-theme", "dark");
    }
    setTheme(isDarkFn(), false);
    applyI18n();

    var savedW = localStorage.getItem("fv-sw");
    if (savedW) sidebarWidth = Math.max(280, parseInt(savedW, 10));
    isDocked = localStorage.getItem("fv-dock") === "1";
    initResize();

    addEventListener("scroll", function() {
        var h = $("appHeader");
        if (h) h.classList.toggle("scrolled", scrollY > 3);
        var g = $("goUpBtn");
        if (g) g.classList.toggle("show", scrollY > 300);
    }, { passive: true });

    $("goUpBtn").onclick = function() { window.scrollTo({ top: 0, behavior: "smooth" }); };

    $("themeBtn").onclick = function() { setTheme(!isDarkFn(), true); };

    $("uploadPopupBtn").onclick = function(e) {
        e.stopPropagation();
        uploadPopupOpen = !uploadPopupOpen;
        $("uploadPopup").style.display = uploadPopupOpen ? "block" : "none";
    };
    $("uploadPopupSelect").onclick = function() {
        $("fileInput").click();
        uploadPopupOpen = false;
        $("uploadPopup").style.display = "none";
    };
    $("uploadPopupDrop").onclick = function() {
        $("fileInput").click();
        uploadPopupOpen = false;
        $("uploadPopup").style.display = "none";
    };
    $("uploadPopupDrop").ondragover = function(e) {
        e.preventDefault();
        this.classList.add("drag-over");
    };
    $("uploadPopupDrop").ondragleave = function(e) {
        e.preventDefault();
        this.classList.remove("drag-over");
    };
    $("uploadPopupDrop").ondrop = function(e) {
        e.preventDefault();
        this.classList.remove("drag-over");
        if (e.dataTransfer.files[0]) {
            loadFont(e.dataTransfer.files[0]);
            uploadPopupOpen = false;
            $("uploadPopup").style.display = "none";
        }
    };
    document.addEventListener("click", function(e) {
        if (uploadPopupOpen && !$("uploadPopup").contains(e.target) && e.target !== $("uploadPopupBtn")) {
            uploadPopupOpen = false;
            $("uploadPopup").style.display = "none";
        }
    });

    $("selectBtn").onclick = function(e) { e.stopPropagation(); $("fileInput").click(); };
    $("uploadZone").onclick = function() { $("fileInput").click(); };
    $("uploadZone").ondragover = function(e) { e.preventDefault(); this.classList.add("drag-over"); };
    $("uploadZone").ondragleave = function(e) { e.preventDefault(); this.classList.remove("drag-over"); };
    $("uploadZone").ondrop = function(e) { e.preventDefault(); this.classList.remove("drag-over"); if (e.dataTransfer.files[0]) loadFont(e.dataTransfer.files[0]); };
    $("fileInput").onchange = function() { if (this.files[0]) loadFont(this.files[0]); };

    document.querySelectorAll(".sort-pill").forEach(function(btn) {
        btn.onclick = function() {
            currentSort = btn.dataset.sort;
            document.querySelectorAll(".sort-pill").forEach(function(p) { p.classList.toggle("active", p === btn); });
            render();
            buildNav();
            setupStickyNav();
        };
    });

    $("tpInput").oninput = updateTp;

    $("voiceBtn").onclick = function() {
        if (!allChars.length) return;
        var sov = $("searchOv");
        if (sov && (sov.style.display === "none" || sov.style.display === "")) {
            sov.style.display = "flex";
            sIdx = -1;
            $("searchIn").value = "";
            renderSR("");
        }
        setTimeout(function() { toggleVoice($("searchVoiceBtn")); }, 150);
    };

    $("searchVoiceBtn").onclick = function() { toggleVoice(this); };

    $("fontInfoBar").onclick = function() {
        $("dName").textContent = meta.name || "";
        $("dCreator").textContent = meta.creator || t("noCreator");
        $("dDate").textContent = meta.date || t("noDate");
        $("dLicense").textContent = meta.license || t("noLicense");
        var dp = $("dPreview");
        if (dp) {
            dp.textContent = "AaBbCcDdEeFf 0123456789";
            dp.style.fontFamily = "'" + fontFamily + "'";
            dp.style.color = $("colorPicker") ? $("colorPicker").value : "";
        }
        $("detailOv").style.display = "flex";
    };
    $("detailX").onclick = function() { closeModal("detailOv"); };
    $("detailOv").onclick = function(e) { if (e.target === $("detailOv")) closeModal("detailOv"); };

    $("settingsBtn").onclick = function() {
        $("prefSort").value = prefs.sort;
        var dc = prefs.color || (isDarkFn() ? "#ffffff" : "#000000");
        $("prefColor").value = dc;
        $("prefColorLbl").textContent = dc.toUpperCase();
        $("prefSize").value = prefs.size;
        $("prefSizeLbl").textContent = prefs.size + "px";
        $("preloadToggle").checked = prefs.preload;
        document.querySelectorAll(".theme-card").forEach(function(tc) {
            tc.classList.toggle("active", tc.dataset.themeVal === (isDarkFn() ? "dark" : "light"));
        });
        document.querySelectorAll(".lang-card").forEach(function(lc) {
            lc.classList.toggle("active", lc.dataset.langVal === lang);
        });
        $("settingsOv").style.display = "flex";
    };
    $("settingsX").onclick = function() { closeModal("settingsOv"); };
    $("settingsOv").onclick = function(e) { if (e.target === $("settingsOv")) closeModal("settingsOv"); };

    document.querySelectorAll(".settings-tab").forEach(function(tab) {
        tab.onclick = function() {
            document.querySelectorAll(".settings-tab").forEach(function(t2) { t2.classList.remove("active"); });
            document.querySelectorAll(".settings-panel").forEach(function(p) { p.classList.remove("active"); });
            tab.classList.add("active");
            var p = document.querySelector('.settings-panel[data-panel="' + tab.dataset.tab + '"]');
            if (p) p.classList.add("active");
        };
    });

    document.querySelectorAll(".theme-card").forEach(function(tc) {
        tc.onclick = function() { setTheme(tc.dataset.themeVal === "dark", true); };
    });

    document.querySelectorAll(".lang-card").forEach(function(lc) {
        lc.onclick = function() {
            lang = lc.dataset.langVal;
            localStorage.setItem("fv-lang", lang);
            document.querySelectorAll(".lang-card").forEach(function(c) {
                c.classList.toggle("active", c.dataset.langVal === lang);
            });
            applyI18n();
            toast(t("langChanged"), "success");
        };
    });

    $("clearCacheBtn").onclick = function() {
        localStorage.clear();
        prefs = { sort: "default", color: null, size: "36", preload: false };
        toast(t("cacheCleaned"), "success");
    };

    $("preloadToggle").onchange = function() {
        prefs.preload = this.checked;
        localStorage.setItem("fv-pref-preload", String(prefs.preload));
        toast(prefs.preload ? t("preloadOn") : t("preloadOff"), "success");
    };

    $("prefSort").onchange = function() {
        prefs.sort = this.value;
        localStorage.setItem("fv-pref-sort", prefs.sort);
        toast(t("sortSaved"), "success");
    };

    $("prefColor").oninput = function() {
        prefs.color = this.value;
        $("prefColorLbl").textContent = this.value.toUpperCase();
        localStorage.setItem("fv-pref-color", prefs.color);
    };

    $("prefSize").oninput = function() {
        prefs.size = this.value;
        $("prefSizeLbl").textContent = this.value + "px";
        localStorage.setItem("fv-pref-size", prefs.size);
    };

    $("searchBtn").onclick = function() {
        if (!allChars.length) return;
        $("searchOv").style.display = "flex";
        sIdx = -1;
        $("searchIn").value = "";
        renderSR("");
        requestAnimationFrame(function() { $("searchIn").focus(); });
    };
    $("searchEsc").onclick = function() { closeModal("searchOv"); stopVoice(); };
    $("searchOv").onclick = function(e) { if (e.target === $("searchOv")) { closeModal("searchOv"); stopVoice(); } };
    $("searchIn").oninput = function() { sIdx = -1; renderSR(this.value); };
    $("searchIn").onkeydown = function(e) {
        var items = $("searchRes").querySelectorAll(".sr-item");
        if (e.key === "ArrowDown") {
            e.preventDefault();
            sIdx = Math.min(sIdx + 1, items.length - 1);
            items.forEach(function(it, i) { it.classList.toggle("hl", i === sIdx); });
            if (sIdx >= 0 && items[sIdx]) items[sIdx].scrollIntoView({ block: "nearest" });
        }
        if (e.key === "ArrowUp") {
            e.preventDefault();
            sIdx = Math.max(sIdx - 1, 0);
            items.forEach(function(it, i) { it.classList.toggle("hl", i === sIdx); });
            if (sIdx >= 0 && items[sIdx]) items[sIdx].scrollIntoView({ block: "nearest" });
        }
        if (e.key === "Enter" && items.length) {
            e.preventDefault();
            items[sIdx >= 0 ? sIdx : 0].click();
        } else if (e.key === "Escape") {
            closeModal("searchOv");
            stopVoice();
        }
    };

    $("colorPicker").oninput = function() {
        uColor = true;
        $("colorLbl").textContent = this.value.toUpperCase();
        updateColors(this.value);
    };

    $("sizeSlider").oninput = function() {
        var s = this.value + "px";
        $("sizeLbl").textContent = s;
        document.querySelectorAll(".char-display").forEach(function(e) { e.style.fontSize = s; });
        updateTp();
    };

    $("bulkSelectAll").onclick = function() {
        if (selectedChars.size === allChars.length) deselectAllChars();
        else selectAllChars();
    };
    $("bulkDownload").onclick = downloadBulkZip;
    $("bulkCancel").onclick = exitBulkMode;

    $("psClose").onclick = closeSidebar;
    $("psOverlay").onclick = function() { if (!isDocked) closeSidebar(); };
    $("psDock").onclick = toggleDock;

    document.onkeydown = function(e) {
        if ((e.ctrlKey || e.metaKey) && e.key === "k") { e.preventDefault(); $("searchBtn").click(); }
        if (e.key === "/" && document.activeElement.tagName !== "INPUT") { e.preventDefault(); $("searchBtn").click(); }
        if (e.key === "Escape") {
            if (sidebarOpen && !isDocked) { closeSidebar(); return; }
            if (bulkMode) { exitBulkMode(); return; }
            if (uploadPopupOpen) { uploadPopupOpen = false; $("uploadPopup").style.display = "none"; }
            if ($("searchOv").style.display !== "none") { closeModal("searchOv"); stopVoice(); }
            else if ($("settingsOv").style.display !== "none") closeModal("settingsOv");
            else if ($("detailOv").style.display !== "none") closeModal("detailOv");
        }
    };

    var spyT;
    addEventListener("scroll", function() {
        clearTimeout(spyT);
        spyT = setTimeout(function() {
            if (scrollLock || isFlatSort()) return;
            var hdr = $("appHeader");
            var hh = (hdr ? hdr.offsetHeight : 60) + 16;
            var ac = null;
            document.querySelectorAll(".char-section").forEach(function(s) {
                var r = s.getBoundingClientRect();
                if (r.top <= hh && r.bottom > hh) ac = s.dataset.cat;
            });
            if (ac) {
                document.querySelectorAll(".npill").forEach(function(p) {
                    p.classList.toggle("active", p.textContent === ac);
                });
            }
        }, 80);
    }, { passive: true });

    document.addEventListener("contextmenu", function(e) {
        e.preventDefault();
        showContextMenu(e.clientX, e.clientY, e.target.closest(".char-card"));
    });

    document.addEventListener("touchstart", function(e) {
        var card = e.target.closest(".char-card");
        if (!card && !e.target.closest(".main")) return;
        longPressFired = false;
        var touch = e.touches[0];
        lpStartX = touch.clientX;
        lpStartY = touch.clientY;
        longPressTimer = setTimeout(function() {
            longPressFired = true;
            if (navigator.vibrate) navigator.vibrate(25);
            showContextMenu(lpStartX, lpStartY, card);
        }, 500);
    }, { passive: true });

    document.addEventListener("touchmove", function(e) {
        if (longPressTimer && e.touches[0]) {
            var dx = e.touches[0].clientX - lpStartX;
            var dy = e.touches[0].clientY - lpStartY;
            if (Math.abs(dx) > 10 || Math.abs(dy) > 10) {
                clearTimeout(longPressTimer);
                longPressTimer = null;
            }
        }
    }, { passive: true });

    document.addEventListener("touchend", function() {
        if (longPressTimer) { clearTimeout(longPressTimer); longPressTimer = null; }
    }, { passive: true });

    document.addEventListener("click", function(e) {
        if (ctxMenu && !ctxMenu.contains(e.target)) { ctxMenu.remove(); ctxMenu = null; }
    });
    document.addEventListener("scroll", function() {
        if (ctxMenu) { ctxMenu.remove(); ctxMenu = null; }
    }, { passive: true });
}

// ══════════════════════════════════════════════════════════════════════
// Bootstrap
// ══════════════════════════════════════════════════════════════════════

if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", init);
} else {
    init();
}

})();
