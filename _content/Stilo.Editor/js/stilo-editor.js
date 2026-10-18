// Stilo Editor — WYSIWYG HTML editor engine

const registry = new Map();

// ─── Public API ──────────────────────────────────────────────────────────────

export function initialize(editorId, dotNetRef, options) {
    // Guard against double-initialization
    if (registry.has(editorId)) destroy(editorId);

    const el = document.getElementById(editorId);
    if (!el) return;

    const controller = new AbortController();
    const { signal } = controller;

    const selectionHandler = () => {
        if (document.activeElement === el) _notifyState(editorId);
    };

    const instance = {
        el,
        dotNetRef,
        options,
        savedRange: null,
        lastContent: '',
        inputTimer: null,
        controller,
        selectionHandler
    };

    registry.set(editorId, instance);

    el.contentEditable = 'true';
    el.spellcheck = true;

    if (options.minHeight) el.style.minHeight = options.minHeight + 'px';
    if (options.maxHeight) el.style.maxHeight = options.maxHeight + 'px';
    if (options.readOnly) el.contentEditable = 'false';

    instance.lastContent = el.innerHTML;

    // All element listeners use AbortController signal for clean removal
    el.addEventListener('input', () => {
        const inst = registry.get(editorId);
        if (!inst) return;
        clearTimeout(inst.inputTimer);
        inst.inputTimer = setTimeout(() => _onInput(editorId), 200);
    }, { signal });

    el.addEventListener('keydown', (e) => _onKeydown(e, editorId), { signal });

    // Save range after mouseup so we capture the full selection (not just mousedown start)
    el.addEventListener('mouseup', () => {
        const inst = registry.get(editorId);
        if (!inst) return;
        const sel = window.getSelection();
        if (sel && sel.rangeCount > 0) inst.savedRange = sel.getRangeAt(0).cloneRange();
        _notifyState(editorId);
    }, { signal });

    // Save range on keyup too (arrow keys, Shift+arrow selections)
    el.addEventListener('keyup', () => {
        const inst = registry.get(editorId);
        if (!inst) return;
        const sel = window.getSelection();
        if (sel && sel.rangeCount > 0) inst.savedRange = sel.getRangeAt(0).cloneRange();
        _notifyState(editorId);
    }, { signal });

    // Save range when editor loses focus (handles color picker, select, and all other cases)
    el.addEventListener('blur', () => {
        const inst = registry.get(editorId);
        if (!inst) return;
        const sel = window.getSelection();
        if (sel && sel.rangeCount > 0) inst.savedRange = sel.getRangeAt(0).cloneRange();
    }, { signal });

    el.addEventListener('focus', () => _notifyState(editorId), { signal });
    el.addEventListener('paste', (e) => _onPaste(e), { signal });

    // Detect clicks on images and tables to show the object toolbar
    el.addEventListener('click', (e) => {
        const inst = registry.get(editorId);
        if (!inst) return;
        const img   = e.target.closest('img');
        const table = e.target.closest('table');
        if (img && inst.el.contains(img)) {
            inst.lastClickedImg   = img;
            inst.lastClickedTable = null;
            _showImageOverlay(img, editorId);
            const r = img.getBoundingClientRect();
            inst.dotNetRef.invokeMethodAsync('OnObjectClick', 'img',
                r.left, r.top, r.width, r.height,
                JSON.stringify({
                    src: img.getAttribute('src') || '',
                    alt: img.alt || '',
                    width: img.width || img.naturalWidth || 0,
                    height: img.height || img.naturalHeight || 0
                }));
            return;
        }
        if (table && inst.el.contains(table)) {
            inst.lastClickedTable = table;
            inst.lastClickedImg   = null;
            _hideImageOverlay();
            const r = table.getBoundingClientRect();
            const rows = table.rows.length;
            const cols = table.rows[0]?.cells.length || 0;
            inst.dotNetRef.invokeMethodAsync('OnObjectClick', 'table',
                r.left, r.top, r.width, r.height,
                JSON.stringify({ rows, cols }));
            return;
        }
        if (inst.lastClickedImg || inst.lastClickedTable) {
            inst.lastClickedImg   = null;
            inst.lastClickedTable = null;
            _hideImageOverlay();
            inst.dotNetRef.invokeMethodAsync('CloseObjectToolbar');
        }
    }, { signal });

    // Table column resize: hover near border → col-resize cursor; drag → adjust widths
    let _colDrag = null;

    el.addEventListener('mousemove', (e) => {
        if (_colDrag) return;
        const cell = e.target.closest('td, th');
        if (cell && el.contains(cell)) {
            const r = cell.getBoundingClientRect();
            if ((e.clientX >= r.right - 5 && e.clientX <= r.right + 5) ||
                (e.clientX >= r.left - 5  && e.clientX <= r.left + 5 && cell.cellIndex > 0)) {
                el.style.cursor = 'col-resize'; return;
            }
        }
        if (el.style.cursor === 'col-resize') el.style.cursor = '';
    }, { signal });

    el.addEventListener('mousedown', (e) => {
        if (_colDrag) return;
        const cell = e.target.closest('td, th');
        if (!cell || !el.contains(cell)) return;
        const r        = cell.getBoundingClientRect();
        const nearRight = e.clientX >= r.right - 5 && e.clientX <= r.right + 5;
        const nearLeft  = e.clientX >= r.left  - 5 && e.clientX <= r.left  + 5 && cell.cellIndex > 0;
        if (!nearRight && !nearLeft) return;
        e.preventDefault();

        const table  = cell.closest('table');
        if (!table) return;
        const colIdx = nearRight ? cell.cellIndex : cell.cellIndex - 1;
        const zoom   = _getEditorZoom(editorId);

        // Freeze all widths so the table doesn't reflow
        table.style.tableLayout = 'fixed';
        for (const row of table.rows)
            for (const c of row.cells)
                c.style.width = (c.getBoundingClientRect().width / zoom) + 'px';

        const startW     = (table.rows[0]?.cells[colIdx]?.getBoundingClientRect().width   || 0) / zoom;
        const startNextW = (table.rows[0]?.cells[colIdx+1]?.getBoundingClientRect().width || 0) / zoom;
        _colDrag = { table, colIdx, startX: e.clientX, startW, startNextW, zoom };

        const onMove = (ev) => {
            if (!_colDrag) return;
            const dx = (ev.clientX - _colDrag.startX) / _colDrag.zoom;
            const nw  = Math.max(20, _colDrag.startW + dx);
            const nw2 = Math.max(20, _colDrag.startNextW - dx);
            for (const row of _colDrag.table.rows) {
                if (row.cells[_colDrag.colIdx])   row.cells[_colDrag.colIdx].style.width   = nw  + 'px';
                if (row.cells[_colDrag.colIdx+1]) row.cells[_colDrag.colIdx+1].style.width = nw2 + 'px';
            }
        };
        const onUp = () => {
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
            el.style.cursor = '';
            _colDrag = null;
            _onInput(editorId);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    }, { signal });

    // Prevent toolbar buttons from stealing focus so selection is never lost
    const container = el.closest('.se-editor-container');
    if (container) {
        container.addEventListener('mousedown', (e) => {
            if (e.target.closest('button')) e.preventDefault();
        }, { signal });
    }

    document.addEventListener('selectionchange', selectionHandler);
}

export function destroy(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    inst.controller?.abort();
    document.removeEventListener('selectionchange', inst.selectionHandler);
    clearTimeout(inst.inputTimer);
    registry.delete(editorId);
    if (_imgOverlay?.editorId === editorId) _hideImageOverlay();
}

// ─── Image resize overlay ─────────────────────────────────────────────────────

let _imgOverlay = null;

function _getEditorZoom(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return 1;
    const ow = inst.el.offsetWidth;
    return ow ? inst.el.getBoundingClientRect().width / ow : 1;
}

function _showImageOverlay(img, editorId) {
    _hideImageOverlay();
    const overlay = document.createElement('div');
    overlay.className = 'se-img-overlay';
    for (const d of ['nw','n','ne','e','se','s','sw','w']) {
        const h = document.createElement('div');
        h.className = `se-img-handle se-img-handle-${d}`;
        h.dataset.dir = d;
        overlay.appendChild(h);
    }
    document.body.appendChild(overlay);
    _imgOverlay = { overlay, img, editorId };
    _positionImageOverlay();

    overlay.addEventListener('mousedown', (e) => {
        const handle = e.target.closest('.se-img-handle');
        if (!handle) return;
        e.preventDefault();
        e.stopPropagation();

        const dir    = handle.dataset.dir;
        const zoom   = _getEditorZoom(editorId);
        const r0     = img.getBoundingClientRect();
        const sx     = e.clientX, sy = e.clientY;
        const sw     = r0.width  / zoom;
        const sh     = r0.height / zoom;
        const aspect = sw / sh;

        const onMove = (ev) => {
            if (!_imgOverlay) return;
            const dx = (ev.clientX - sx) / zoom;
            const dy = (ev.clientY - sy) / zoom;
            const hasE = dir.includes('e'), hasW = dir.includes('w');
            const hasN = dir.includes('n'), hasS = dir.includes('s');
            let nw = sw, nh = sh;
            if (hasE) nw = Math.max(20, sw + dx);
            if (hasW) nw = Math.max(20, sw - dx);
            if (hasS) nh = Math.max(20, sh + dy);
            if (hasN) nh = Math.max(20, sh - dy);
            if (dir.length === 2) nh = nw / aspect; // corner: lock aspect ratio
            img.style.width  = nw + 'px';
            img.style.height = (hasN || hasS || dir.length === 2) ? nh + 'px' : '';
            _positionImageOverlay();
        };
        const onUp = () => {
            document.removeEventListener('mousemove', onMove);
            document.removeEventListener('mouseup', onUp);
            if (_imgOverlay) _onInput(_imgOverlay.editorId);
        };
        document.addEventListener('mousemove', onMove);
        document.addEventListener('mouseup', onUp);
    });
}

function _hideImageOverlay() {
    if (_imgOverlay) { _imgOverlay.overlay.remove(); _imgOverlay = null; }
}

function _positionImageOverlay() {
    if (!_imgOverlay) return;
    const r = _imgOverlay.img.getBoundingClientRect();
    const s = _imgOverlay.overlay.style;
    s.left = r.left + 'px'; s.top  = r.top  + 'px';
    s.width = r.width + 'px'; s.height = r.height + 'px';
}

// Update overlay on scroll/resize (global, cheap if no overlay active)
window.addEventListener('scroll',  _positionImageOverlay, true);
window.addEventListener('resize',  _positionImageOverlay);

export function execCommand(editorId, command, value) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    try { document.execCommand('styleWithCSS', false, true); } catch (_) {}
    document.execCommand(command, false, value ?? null);
    _onInput(editorId);
    _notifyState(editorId);
}

export function getContent(editorId) {
    const inst = registry.get(editorId);
    return inst ? _stripTypoSpans(inst.el.innerHTML) : '';
}

export function getContentRaw(editorId) {
    const inst = registry.get(editorId);
    return inst ? inst.el.innerHTML : '';
}

function _stripTypoSpans(html) {
    return html.replace(/<span[^>]*class="se-spell-typo[^"]*"[^>]*>(.*?)<\/span>/gi, '$1');
}

export function setContent(editorId, html) {
    const inst = registry.get(editorId);
    if (!inst) return;
    inst.el.innerHTML = html ?? '';
    inst.lastContent = inst.el.innerHTML;
}

export function insertHtml(editorId, html) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    document.execCommand('insertHTML', false, html);
    _onInput(editorId);
}

export function insertTable(editorId, rows, cols, withHeader, styleClass) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);

    const cls = styleClass || 'se-table-word';
    let html = `<table class="${cls}">`;
    if (withHeader) {
        html += '<thead><tr>';
        for (let c = 0; c < cols; c++) html += `<th>Intestazione ${c + 1}</th>`;
        html += '</tr></thead>';
    }
    html += '<tbody>';
    for (let r = 0; r < rows; r++) {
        html += '<tr>';
        for (let c = 0; c < cols; c++) html += '<td><br></td>';
        html += '</tr>';
    }
    html += '</tbody></table><p><br></p>';

    document.execCommand('insertHTML', false, html);
    _onInput(editorId);
}

export function insertLink(editorId, url, text, target) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);

    const sel = window.getSelection();
    const hasSelection = sel && !sel.isCollapsed;

    if (hasSelection) {
        document.execCommand('createLink', false, url);
        const anchors = inst.el.querySelectorAll(`a[href="${url}"]`);
        anchors.forEach(a => {
            if (target) a.target = target;
        });
    } else {
        const targetAttr = target ? ` target="${target}"` : '';
        document.execCommand('insertHTML', false, `<a href="${_escapeAttr(url)}"${targetAttr}>${_escapeHtml(text || url)}</a>`);
    }
    _onInput(editorId);
}

export function insertImage(editorId, src, alt, width) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    const widthAttr = width ? ` width="${width}"` : '';
    document.execCommand('insertHTML', false, `<img src="${_escapeAttr(src)}" alt="${_escapeAttr(alt ?? '')}"${widthAttr} class="se-image">`);
    _onInput(editorId);
}

export function applyFontName(editorId, fontFamily) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    _wrapSelectionWith(inst, 'span', `font-family:${fontFamily}`);
    _onInput(editorId);
    _notifyState(editorId);
}

export function applyFontSize(editorId, sizePx) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    _wrapSelectionWith(inst, 'span', `font-size:${sizePx}px`);
    _onInput(editorId);
    _notifyState(editorId);
}

export function applyForeColor(editorId, color) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    try { document.execCommand('styleWithCSS', false, true); } catch (_) {}
    document.execCommand('foreColor', false, color);
    _onInput(editorId);
}

export function applyBackColor(editorId, color) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    try { document.execCommand('styleWithCSS', false, true); } catch (_) {}
    document.execCommand('hiliteColor', false, color);
    _onInput(editorId);
}

// Uses native browser undo for per-character granularity
export function undo(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    document.execCommand('undo');
    _onInput(editorId);
    _notifyState(editorId);
}

export function redo(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    document.execCommand('redo');
    _onInput(editorId);
    _notifyState(editorId);
}

export function saveRange(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    const sel = window.getSelection();
    if (sel && sel.rangeCount > 0) {
        inst.savedRange = sel.getRangeAt(0).cloneRange();
    }
}

export function focus(editorId) {
    const inst = registry.get(editorId);
    if (inst) {
        inst.el.focus();
        _restoreRange(inst);
    }
}

export function getSelectionText(editorId) {
    const sel = window.getSelection();
    return sel ? sel.toString() : '';
}

export function getElementBottom(elId) {
    const el = document.getElementById(elId);
    if (!el) return [0, 0, 0, 0];
    const r = el.getBoundingClientRect();
    // Larghezza/altezza della finestra: chi chiama le usa per non far uscire il flyout dai bordi
    // (successo con l'accessibilita', sempre nell'angolo destro del nastro: il flyout partiva da
    // r.left senza mai controllare quanto restava a destra, e usciva dalla finestra).
    return [r.bottom, r.left, window.innerWidth, window.innerHeight];
}

export function scrollRibbon(ribbonId, amount) {
    const el = document.getElementById(ribbonId);
    if (!el) return [0, 0, 0];
    const wrap = el.parentElement;
    if (!wrap) return [0, 0, 0];

    const maxLeft = wrap.scrollWidth - wrap.clientWidth;
    let next = wrap.scrollLeft + amount;
    next = Math.max(0, Math.min(maxLeft, next));

    wrap.scrollLeft = next;

    return [next > 0 ? 1 : 0, next < maxLeft ? 1 : 0, Math.round(-next)];
}

export function getRibbonScrollState(ribbonId) {
    const el = document.getElementById(ribbonId);
    if (!el) return [0, 0];
    const wrap = el.parentElement;
    if (!wrap) return [0, 0];

    const maxLeft = wrap.scrollWidth - wrap.clientWidth;
    if (maxLeft <= 1) return [0, 0];

    return [wrap.scrollLeft > 0 ? 1 : 0, wrap.scrollLeft < maxLeft ? 1 : 0];
}

export function applyRibbonTransform(ribbonId, x) {
    const el = document.getElementById(ribbonId);
    if (!el) return;
    const wrap = el.parentElement;
    if (!wrap) return;
    const maxLeft = Math.max(0, wrap.scrollWidth - wrap.clientWidth);
    wrap.scrollLeft = Math.max(0, Math.min(maxLeft, -x));
}

export function setReadOnly(editorId, readOnly) {
    const inst = registry.get(editorId);
    if (!inst) return;
    inst.el.contentEditable = readOnly ? 'false' : 'true';
}

export async function pasteFromClipboard(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _restoreFocus(inst);
    try {
        const items = await navigator.clipboard.read();
        for (const item of items) {
            if (item.types.includes('text/html')) {
                const blob = await item.getType('text/html');
                const html = await blob.text();
                const clean = _cleanPastedHtml(html);
                document.execCommand('insertHTML', false, clean);
                _onInput(editorId);
                return;
            }
            if (item.types.includes('text/plain')) {
                const blob = await item.getType('text/plain');
                const text = await blob.text();
                document.execCommand('insertText', false, text);
                _onInput(editorId);
                return;
            }
        }
    } catch (_) {
        try { document.execCommand('paste'); } catch (__) {}
    }
}

// ─── Internal helpers ─────────────────────────────────────────────────────────

function _onInput(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    const current = inst.el.innerHTML;
    if (current !== inst.lastContent) {
        inst.lastContent = current;
        inst.dotNetRef.invokeMethodAsync('OnContentChanged', current);
    }
}

function _onKeydown(e, editorId) {
    // Let browser handle Ctrl+Z/Y natively for per-character undo granularity.
    // Toolbar buttons call our undo()/redo() which also use execCommand('undo/redo').
    // No override needed here.
}

function _onPaste(e) {
    e.preventDefault();
    const cd = e.clipboardData;
    if (!cd) return;

    if (cd.types.includes('text/html')) {
        const html = cd.getData('text/html');
        const clean = _cleanPastedHtml(html);
        document.execCommand('insertHTML', false, clean);
    } else {
        const text = cd.getData('text/plain');
        document.execCommand('insertText', false, text);
    }
}

function _cleanPastedHtml(html) {
    const tmp = document.createElement('div');
    tmp.innerHTML = html;

    ['script', 'style', 'meta', 'link', 'object', 'embed'].forEach(tag => {
        tmp.querySelectorAll(tag).forEach(el => el.remove());
    });

    tmp.querySelectorAll('[class]').forEach(el => el.removeAttribute('class'));

    tmp.querySelectorAll('*').forEach(el => {
        [...el.attributes].filter(a => a.name.startsWith('data-')).forEach(a => el.removeAttribute(a.name));
    });

    return tmp.innerHTML;
}

function _toHexColor(color) {
    if (!color) return '#000000';
    if (/^#[0-9a-f]{6}$/i.test(color)) return color;
    const m = color.match(/^rgb\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)$/i);
    if (m) {
        return '#' + [1,2,3].map(i => parseInt(m[i]).toString(16).padStart(2, '0')).join('');
    }
    return '#000000';
}

function _notifyState(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;

    try {
        const state = {
            bold: document.queryCommandState('bold'),
            italic: document.queryCommandState('italic'),
            underline: document.queryCommandState('underline'),
            strikethrough: document.queryCommandState('strikeThrough'),
            subscript: document.queryCommandState('subscript'),
            superscript: document.queryCommandState('superscript'),
            alignment: _getAlignment(),
            orderedList: document.queryCommandState('insertOrderedList'),
            unorderedList: document.queryCommandState('insertUnorderedList'),
            fontName: _getCurrentFontName(),
            fontSize: _getCurrentFontSize(),
            foreColor: _toHexColor(_safeQueryValue('foreColor')),
            backColor: _toHexColor(_safeQueryValue('backColor')),
            formatBlock: _safeQueryValue('formatBlock').replace(/^<|>$/g, '').toLowerCase() || 'p',
            canUndo: document.queryCommandEnabled('undo'),
            canRedo: document.queryCommandEnabled('redo')
        };

        inst.dotNetRef.invokeMethodAsync('OnStateChanged', state);
    } catch (_) { }
}

function _getAlignment() {
    if (document.queryCommandState('justifyLeft')) return 'left';
    if (document.queryCommandState('justifyCenter')) return 'center';
    if (document.queryCommandState('justifyRight')) return 'right';
    if (document.queryCommandState('justifyFull')) return 'justify';
    return 'left';
}

function _safeQueryValue(cmd) {
    try { return document.queryCommandValue(cmd) || ''; } catch (_) { return ''; }
}

function _getCurrentFontName() {
    const sel = window.getSelection();
    if (!sel || !sel.rangeCount) return '';
    let node = sel.getRangeAt(0).startContainer;
    if (node.nodeType === Node.TEXT_NODE) node = node.parentElement;
    // Walk up checking EXPLICIT inline style only (computedStyle always returns a value)
    while (node && node.nodeType === Node.ELEMENT_NODE && node !== document.body) {
        const ff = node.style && node.style.fontFamily;
        if (ff) return ff.replace(/^["']|["']$/g, '').split(',')[0].trim();
        node = node.parentElement;
    }
    return '';
}

function _getCurrentFontSize() {
    const sel = window.getSelection();
    if (!sel || !sel.rangeCount) return '';
    let node = sel.getRangeAt(0).startContainer;
    if (node.nodeType === Node.TEXT_NODE) node = node.parentElement;
    while (node && node.nodeType === Node.ELEMENT_NODE && node !== document.body) {
        const sz = node.style && node.style.fontSize;
        if (sz) return Math.round(parseFloat(sz)) + '';
        node = node.parentElement;
    }
    return '';
}

function _wrapSelectionWith(inst, tag, style) {
    const sel = window.getSelection();
    if (!sel || sel.isCollapsed) return;
    const range = sel.getRangeAt(0);
    const span = document.createElement(tag);
    span.style.cssText = style;
    try {
        range.surroundContents(span);
    } catch (_) {
        // surroundContents fails when selection spans multiple elements.
        // extractContents preserves inner HTML structure.
        const contents = range.extractContents();
        span.appendChild(contents);
        range.insertNode(span);
    }
}

function _restoreFocus(inst) {
    if (document.activeElement !== inst.el) {
        inst.el.focus();
        _restoreRange(inst);
    }
}

function _restoreRange(inst) {
    if (inst.savedRange) {
        const sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(inst.savedRange);
    }
}

function _escapeHtml(str) {
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
}

function _escapeAttr(str) {
    return str.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

// ─── Spell Check ───────────────────────────────────────────────────────────────

let spellCheckTimers = {};

export function enableSpellCheck(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    inst.spellCheckEnabled = true;
    // Don't trigger spell check here — C# caller handles it via OnSpellCheckRequest
}

export function disableSpellCheck(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    inst.spellCheckEnabled = false;
    clearTimeout(spellCheckTimers[editorId]);
    _saveCursor(inst);
    _clearTypos(inst);
    inst.el.normalize();
    _restoreCursor(inst);
}

export function runSpellCheck(editorId) {
    _triggerSpellCheck(editorId, true);
}

const CARET_MARKER = '\u0008\u007F\u0001';

function _saveCursor(inst) {
    const sel = window.getSelection();
    if (!sel || !sel.rangeCount || document.activeElement !== inst.el) return false;

    const range = sel.getRangeAt(0);
    // Only save cursor for collapsed ranges — never delete content
    if (!range.collapsed) return false;
    range.insertNode(document.createTextNode(CARET_MARKER));
    return true;
}

function _restoreCursor(inst) {
    const walker = document.createTreeWalker(inst.el, NodeFilter.SHOW_TEXT, null, false);
    while (walker.nextNode()) {
        const node = walker.currentNode;
        const idx = node.textContent.indexOf(CARET_MARKER);
        if (idx >= 0) {
            node.textContent = node.textContent.replace(CARET_MARKER, '');
            const sel = window.getSelection();
            if (sel) {
                const range = document.createRange();
                const offset = Math.min(idx, node.textContent.length);
                range.setStart(node, offset);
                range.collapse(true);
                sel.removeAllRanges();
                sel.addRange(range);
            }
            return;
        }
    }
}

export function markTypos(editorId, typoMap) {
    const inst = registry.get(editorId);
    if (!inst) return;

    _saveCursor(inst);
    _clearTypos(inst);
    inst.el.normalize();
    if (typoMap && Object.keys(typoMap).length > 0) {
        _markTyposChunked(inst, typoMap);
    } else {
        _restoreCursor(inst);
    }
}

function _markTyposChunked(inst, typoMap) {
    const treeWalker = document.createTreeWalker(inst.el, NodeFilter.SHOW_TEXT, null, false);
    const textNodes = [];
    // Limit to 1000 text nodes to avoid freezing on huge documents
    const MAX_NODES = 1000;
    while (treeWalker.nextNode() && textNodes.length < MAX_NODES) {
        const node = treeWalker.currentNode;
        if (node.parentNode && node.parentNode.classList &&
            (node.parentNode.classList.contains('se-spell-typo') || node.parentNode.classList.contains('se-spell-typo-disabled'))) {
            continue;
        }
        textNodes.push(node);
    }

    if (textNodes.length === 0) { _restoreCursor(inst); return; }

    const CHUNK_SIZE = 100;
    let index = 0;

    function processChunk() {
        const end = Math.min(index + CHUNK_SIZE, textNodes.length);
        for (; index < end; index++) {
            _markTyposInTextNode(textNodes[index], typoMap);
        }
        if (index < textNodes.length) {
            requestAnimationFrame(processChunk);
        } else {
            _restoreCursor(inst);
        }
    }

    // Process first chunk synchronously so typo spans appear immediately
    const firstEnd = Math.min(CHUNK_SIZE, textNodes.length);
    for (; index < firstEnd; index++) {
        _markTyposInTextNode(textNodes[index], typoMap);
    }
    if (index < textNodes.length) {
        requestAnimationFrame(processChunk);
    } else {
        _restoreCursor(inst);
    }
}

export function clearTypos(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    _clearTypos(inst);
}

export function replaceWord(editorId, oldWord, newWord) {
    const inst = registry.get(editorId);
    if (!inst) return;

    const typoSpans = inst.el.querySelectorAll('span.se-spell-typo');
    for (const span of typoSpans) {
        const text = span.textContent;
        if (text === oldWord) {
            const txtNode = document.createTextNode(newWord);
            span.parentNode.replaceChild(txtNode, span);
        }
    }
    // Re-naturalize and update cache without triggering extra events
    inst.lastContent = inst.el.innerHTML;
}

export function getWordAtPoint(editorId, x, y) {
    const inst = registry.get(editorId);
    if (!inst) return null;

    const origDisplay = inst.el.style.display;
    inst.el.style.display = 'none';
    const el = document.elementFromPoint(x, y);
    inst.el.style.display = origDisplay;
    if (!el) return null;

    if (el.classList && el.classList.contains('se-spell-typo')) {
        return { word: el.textContent, rect: el.getBoundingClientRect() };
    }
    return null;
}

function _triggerSpellCheck(editorId, immediate) {
    const inst = registry.get(editorId);
    if (!inst || !inst.spellCheckEnabled) return;

    clearTimeout(spellCheckTimers[editorId]);
    spellCheckTimers[editorId] = setTimeout(() => {
        const text = _getPlainText(inst.el);
        if (text.length > 0) {
            inst.dotNetRef.invokeMethodAsync('OnSpellCheckRequest', text);
        }
    }, immediate ? 50 : 250);
}

function _getPlainText(root) {
    let text = '';
    const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null, false);
    while (walker.nextNode()) {
        const node = walker.currentNode;
        if (node.parentNode && node.parentNode.classList &&
            (node.parentNode.classList.contains('se-spell-typo') || node.parentNode.classList.contains('se-spell-typo-disabled'))) {
            continue;
        }
        text += node.textContent + ' ';
    }
    return text;
}

function _clearTypos(inst) {
    const container = inst.el;
    while (true) {
        const spans = container.querySelectorAll('span.se-spell-typo, span.se-spell-typo-disabled');
        if (spans.length === 0) break;
        for (const span of spans) {
            const parent = span.parentNode;
            if (parent) {
                const text = document.createTextNode(span.textContent);
                parent.replaceChild(text, span);
                parent.normalize();
            }
        }
    }
}

function _markTyposInElement(root, typoMap) {
    if (!typoMap || Object.keys(typoMap).length === 0) return;

    const treeWalker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT, null, false);
    const textNodes = [];
    while (treeWalker.nextNode()) {
        const node = treeWalker.currentNode;
        if (node.parentNode && node.parentNode.classList &&
            (node.parentNode.classList.contains('se-spell-typo') || node.parentNode.classList.contains('se-spell-typo-disabled'))) {
            continue;
        }
        textNodes.push(node);
    }

    for (const textNode of textNodes) {
        _markTyposInTextNode(textNode, typoMap);
    }
}

function _markTyposInTextNode(textNode, typoMap) {
    const text = textNode.textContent;
    if (!text) return;

    const regex = /[\p{L}'\u2019]+/gu;
    let match;
    const fragments = [];
    let lastIndex = 0;

    while ((match = regex.exec(text)) !== null) {
        const word = match[0].replace(/\u2019/g, "'");

        if (lastIndex < match.index) {
            fragments.push(document.createTextNode(text.slice(lastIndex, match.index)));
        }

        if (typoMap[word] || typoMap[word.toLowerCase()]) {
            const span = document.createElement('span');
            span.className = 'se-spell-typo';
            span.setAttribute('data-word', word);
            span.textContent = match[0];
            fragments.push(span);
        } else {
            fragments.push(document.createTextNode(match[0]));
        }

        lastIndex = match.index + match[0].length;
    }

    if (lastIndex < text.length) {
        fragments.push(document.createTextNode(text.slice(lastIndex)));
    }

    if (fragments.length > 1 || lastIndex < text.length) {
        const parent = textNode.parentNode;
        const refNode = textNode.nextSibling;
        const fragment = document.createDocumentFragment();
        for (const frag of fragments) {
            fragment.appendChild(frag);
        }
        parent.insertBefore(fragment, refNode);
        parent.removeChild(textNode);
    }
}

// ─── Ruler & grid & object toolbar ───────────────────────────────────────────

export function initHRuler(wrapperId, pageWidthMm, dotNetRef) {
    const wrap = document.getElementById(wrapperId);
    if (!wrap) return;

    if (wrap._hrulerCleanup) wrap._hrulerCleanup();

    let dragging = null;

    const onMouseDown = (e) => {
        const handle = e.target.closest('.se-rh-handle');
        if (!handle) return;
        e.preventDefault();
        dragging = handle.getAttribute('data-side');
    };

    const onMouseUp = (e) => {
        if (!dragging) return;
        const svg = wrap.querySelector('svg');
        if (svg) {
            const r = svg.getBoundingClientRect();
            const mm = Math.max(0, (e.clientX - r.left) / r.width * pageWidthMm);
            dotNetRef.invokeMethodAsync('OnHRulerDrag', dragging, mm);
        }
        dragging = null;
    };

    const onMouseLeave = () => { dragging = null; };

    wrap.addEventListener('mousedown', onMouseDown);
    document.addEventListener('mouseup', onMouseUp);

    wrap._hrulerCleanup = () => {
        wrap.removeEventListener('mousedown', onMouseDown);
        document.removeEventListener('mouseup', onMouseUp);
    };
}

export function initVRuler(stripId, pageHeightMm, dotNetRef) {
    const strip = document.getElementById(stripId);
    if (!strip) return;

    if (strip._vrulerCleanup) strip._vrulerCleanup();

    let dragging = null;
    let startY = 0;
    const marginTopMm = parseFloat(getComputedStyle(strip).getPropertyValue('--se-margin-top')) || 25;
    const marginBottomMm = parseFloat(getComputedStyle(strip).getPropertyValue('--se-margin-bottom')) || 25;

    const onMouseDown = (e) => {
        const r = strip.getBoundingClientRect();
        const relY = e.clientY - r.top;
        const zoom = parseFloat(strip.style.zoom || '100') / 100;
        const mmPerPx = pageHeightMm / (r.height / zoom);
        const clickMm = relY / zoom * mmPerPx;
        const topBoundary = marginTopMm;
        const bottomBoundary = pageHeightMm - marginBottomMm;
        if (Math.abs(clickMm - topBoundary) < 4) { dragging = 'top'; e.preventDefault(); }
        else if (Math.abs(clickMm - bottomBoundary) < 4) { dragging = 'bottom'; e.preventDefault(); }
    };

    const onMouseMove = (e) => {
        if (!dragging) return;
        const r = strip.getBoundingClientRect();
        const zoom = parseFloat(strip.style.zoom || '100') / 100;
        const mmPerPx = pageHeightMm / (r.height / zoom);
        const mm = (e.clientY - r.top) / zoom * mmPerPx;
        strip.style.cursor = 'ns-resize';
        dotNetRef.invokeMethodAsync('OnVRulerDrag', dragging, mm);
    };

    const onMouseUp = () => { dragging = null; };

    strip.addEventListener('mousedown', onMouseDown);
    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);

    strip._vrulerCleanup = () => {
        strip.removeEventListener('mousedown', onMouseDown);
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
    };
}

export function initTableGrid(gridId, labelId, dotNetRef) {
    const label = document.getElementById(labelId);
    const old = document.getElementById(gridId);
    if (!old || !label) return;

    // Clone to cleanly remove previous event listeners
    const grid = old.cloneNode(true);
    old.parentNode.replaceChild(grid, old);

    grid.addEventListener('mouseover', (e) => {
        const cell = e.target.closest('.se-tg-cell');
        if (!cell) return;
        const r = +cell.dataset.r;
        const c = +cell.dataset.c;
        label.textContent = `${r + 1} × ${c + 1}`;
        for (const cc of grid.querySelectorAll('.se-tg-cell'))
            cc.classList.toggle('se-tg-active', +cc.dataset.r <= r && +cc.dataset.c <= c);
    });

    grid.addEventListener('mouseleave', () => {
        label.textContent = 'Seleziona griglia';
        for (const cc of grid.querySelectorAll('.se-tg-cell'))
            cc.classList.remove('se-tg-active');
    });

    grid.addEventListener('click', (e) => {
        const cell = e.target.closest('.se-tg-cell');
        if (!cell) return;
        dotNetRef.invokeMethodAsync('OnTableGridSelect', +cell.dataset.r + 1, +cell.dataset.c + 1);
    });
}

export function setImageStyle(editorId, pos) {
    const inst = registry.get(editorId);
    if (!inst || !inst.lastClickedImg) return;
    const img = inst.lastClickedImg;
    img.style.float  = '';
    img.style.display = '';
    img.style.margin  = '';
    if (pos === 'left')   { img.style.float = 'left';  img.style.margin = '0 12px 6px 0'; }
    if (pos === 'right')  { img.style.float = 'right'; img.style.margin = '0 0 6px 12px'; }
    if (pos === 'inline') { img.style.display = 'inline'; }
    _onInput(editorId);
}

export function deleteSelectedObject(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    const el = inst.lastClickedImg || inst.lastClickedTable;
    if (el && inst.el.contains(el)) el.remove();
    inst.lastClickedImg   = null;
    inst.lastClickedTable = null;
    _onInput(editorId);
}

export function execTableCommand(editorId, command) {
    const inst = registry.get(editorId);
    if (!inst || !inst.lastClickedTable) return;
    const tbl = inst.lastClickedTable;
    if (!inst.el.contains(tbl)) return;

    const sel = window.getSelection();
    let targetCell = null;
    if (sel && sel.rangeCount > 0) {
        let node = sel.getRangeAt(0).commonAncestorContainer;
        while (node && node !== tbl) {
            if (node.nodeName === 'TD' || node.nodeName === 'TH') { targetCell = node; break; }
            node = node.parentNode;
        }
    }
    if (!targetCell) targetCell = tbl.rows[0]?.cells[0];
    if (!targetCell) return;

    const row = targetCell.parentNode;
    const rowIndex = row.rowIndex;
    const colIndex = targetCell.cellIndex;
    const colCount = tbl.rows[0]?.cells.length || 1;

    if (command === 'insertRowAbove') {
        const newRow = tbl.insertRow(rowIndex);
        for (let i = 0; i < colCount; i++) newRow.insertCell(i).innerHTML = '<br>';
    } else if (command === 'insertRowBelow') {
        const newRow = tbl.insertRow(rowIndex + 1);
        for (let i = 0; i < colCount; i++) newRow.insertCell(i).innerHTML = '<br>';
    } else if (command === 'insertColLeft') {
        for (const r of tbl.rows) r.insertCell(colIndex).innerHTML = '<br>';
    } else if (command === 'insertColRight') {
        for (const r of tbl.rows) r.insertCell(colIndex + 1).innerHTML = '<br>';
    } else if (command === 'deleteRow') {
        if (tbl.rows.length > 1) tbl.deleteRow(rowIndex);
    } else if (command === 'deleteCol') {
        if (colCount > 1) for (const r of tbl.rows) if (r.cells[colIndex]) r.deleteCell(colIndex);
    } else if (command === 'mergeCells') {
        const sel = window.getSelection();
        if (sel && sel.rangeCount > 0) {
            let startCell = null, endCell = null;
            const range = sel.getRangeAt(0);
            let node = range.startContainer;
            while (node && node !== tbl) {
                if (node.nodeName === 'TD' || node.nodeName === 'TH') { startCell = node; break; }
                node = node.parentNode;
            }
            node = range.endContainer;
            while (node && node !== tbl) {
                if (node.nodeName === 'TD' || node.nodeName === 'TH') { endCell = node; break; }
                node = node.parentNode;
            }
            if (startCell && endCell && startCell !== endCell) {
                const startRow = startCell.parentNode.rowIndex;
                const endRow = endCell.parentNode.rowIndex;
                const startCol = startCell.cellIndex;
                const endCol = endCell.cellIndex;
                const r1 = Math.min(startRow, endRow), r2 = Math.max(startRow, endRow);
                const c1 = Math.min(startCol, endCol), c2 = Math.max(startCol, endCol);
                let mergedHtml = '';
                for (let r = r1; r <= r2; r++) {
                    const row = tbl.rows[r];
                    for (let c = c1; c <= c2; c++) {
                        const cell = row.cells[c];
                        if (cell) mergedHtml += cell.innerHTML;
                    }
                }
                // Remove merged cells (right-to-left, bottom-to-top)
                for (let r = r2; r >= r1; r--) {
                    const row = tbl.rows[r];
                    for (let c = c2; c >= c1; c--) {
                        if (r === r1 && c === c1) continue; // keep anchor
                        const cell = row.cells[c1];
                        if (cell) row.deleteCell(c1);
                    }
                }
                const anchor = tbl.rows[r1].cells[c1];
                anchor.rowSpan = (r2 - r1 + 1);
                anchor.colSpan = (c2 - c1 + 1);
                anchor.innerHTML = mergedHtml;
            }
        }
    } else if (command === 'splitCell') {
        if (targetCell.colSpan > 1 || targetCell.rowSpan > 1) {
            const cs = targetCell.colSpan || 1;
            const rs = targetCell.rowSpan || 1;
            targetCell.colSpan = 1;
            targetCell.rowSpan = 1;
            if (cs > 1) {
                for (let c = 1; c < cs; c++) {
                    const newCell = row.insertCell(colIndex + c);
                    newCell.innerHTML = '<br>';
                }
            }
            if (rs > 1) {
                for (let r = 1; r < rs; r++) {
                    const newRow = tbl.rows[rowIndex + r];
                    if (newRow) {
                        const newCell = newRow.insertCell(colIndex);
                        newCell.innerHTML = '<br>';
                    }
                }
            }
        }
    } else if (command.startsWith('cellBg:')) {
        const color = command.split(':')[1];
        targetCell.style.backgroundColor = color;
    } else if (command === 'borderAll') {
        tbl.style.borderCollapse = 'collapse';
        for (const cell of tbl.querySelectorAll('td, th')) {
            cell.style.border = '1px solid #333';
        }
    } else if (command === 'borderNone') {
        for (const cell of tbl.querySelectorAll('td, th')) {
            cell.style.border = 'none';
        }
    } else if (command === 'borderOuter') {
        tbl.style.border = '1px solid #333';
        for (const cell of tbl.querySelectorAll('td, th')) {
            cell.style.border = 'none';
        }
    } else if (command === 'tableAlignLeft') {
        tbl.style.marginLeft = '0';
        tbl.style.marginRight = 'auto';
    } else if (command === 'tableAlignCenter') {
        tbl.style.marginLeft = 'auto';
        tbl.style.marginRight = 'auto';
    } else if (command === 'tableAlignRight') {
        tbl.style.marginLeft = 'auto';
        tbl.style.marginRight = '0';
    }

    _onInput(editorId);
}

// Hook into input to trigger spell check (only on actual content changes)
const _origOnInput = _onInput;
_onInput = function(editorId) {
    const inst = registry.get(editorId);
    if (!inst) return;
    const current = inst.el.innerHTML;
    if (current !== inst.lastContent) {
        inst.lastContent = current;
        inst.dotNetRef.invokeMethodAsync('OnContentChanged', _stripTypoSpans(current));
        _triggerSpellCheck(editorId, false);
    }
};

// Left-click handler for typo spans and popup dismiss
document.addEventListener('click', function(e) {
    // Typo span click → show suggestions
    const span = e.target.closest('.se-spell-typo');
    if (span) {
        for (const [editorId, inst] of registry) {
            if (inst.el && inst.el.contains(span)) {
                const word = span.getAttribute('data-word') || span.textContent;
                const rect = span.getBoundingClientRect();
                inst.dotNetRef.invokeMethodAsync('OnTypoClick', word, rect.left, rect.bottom + 4);
                return;
            }
        }
    }

    // Close suggestion popup when clicking outside (not on typo, not on popup itself)
    for (const [editorId, inst] of registry) {
        const editorRoot = inst.el && inst.el.closest('.stilo-editor');
        if (!editorRoot) continue;
        const popup = editorRoot.querySelector('.se-spell-popup');
        if (popup && !popup.contains(e.target)) {
            // Never close if clicking on a typo
            if (e.target.closest('.se-spell-typo')) continue;
            inst.dotNetRef.invokeMethodAsync('CloseSuggestionPopup');
        }
    }
});

export function downloadFile(filename, mimeType, base64) {
    const a = document.createElement('a');
    a.href = 'data:' + mimeType + ';base64,' + base64;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

export function printHtml(html) {
    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write('<!DOCTYPE html><html><head><meta charset="utf-8"><title>Stampa</title><style>body{font-family:sans-serif;margin:20mm}@media print{body{margin:0}}</style></head><body>' + html + '</body></html>');
    win.document.close();
    win.focus();
    win.print();
}

// Expose registry for plugin modules
window.__stiloGetInst = function(editorId) { return registry.get(editorId); };
