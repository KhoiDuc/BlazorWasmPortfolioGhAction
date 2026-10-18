// Stilo Editor — Plugin extra functions (separate from core stilo-editor.js)

// ─── Fullscreen ──────────────────────────────────────────────────────────────

export function toggleFullscreen(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    const editor = el.closest('.stilo-editor');
    if (!editor) return;
    const isFull = editor.classList.toggle('se-fullscreen');
    document.body.classList.toggle('se-fullscreen-body', isFull);
    // Trigger resize to let editor recalc
    window.dispatchEvent(new Event('resize'));
    // Set up Esc key listener once
    if (isFull && !editor._fsListener) {
        editor._fsListener = (e) => {
            if (e.key === 'Escape' && editor.classList.contains('se-fullscreen')) {
                editor.classList.remove('se-fullscreen');
                document.body.classList.remove('se-fullscreen-body');
                window.dispatchEvent(new Event('resize'));
            }
        };
        document.addEventListener('keydown', editor._fsListener);
    }
    return isFull;
}

// ─── Word Count ──────────────────────────────────────────────────────────────

export function getWordCount(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return { words: 0, chars: 0, charsNoSpace: 0 };
    const text = el.innerText || '';
    const words = text.trim() ? text.trim().split(/\s+/).length : 0;
    return {
        words,
        chars: text.length,
        charsNoSpace: text.replace(/\s/g, '').length
    };
}

// ─── Dark Mode ───────────────────────────────────────────────────────────────

export function toggleDarkMode(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    const editor = el.closest('.stilo-editor');
    if (!editor) return;
    const isDark = editor.classList.toggle('se-dark');
    return isDark;
}

// ─── HTML Support (element allowlist sanitizer) ──────────────────────────────

export function sanitizeHtml(html, allowedTags) {
    if (!allowedTags || allowedTags.length === 0) return html;
    const tagSet = new Set(allowedTags.map(t => t.toLowerCase()));
    // Always allow text-level elements needed for basic editing
    const always = new Set(['#text', 'br', 'p', 'div', 'span', 'strong', 'b', 'em', 'i', 'u', 's', 'sub', 'sup',
        'a', 'ul', 'ol', 'li', 'table', 'thead', 'tbody', 'tfoot', 'tr', 'td', 'th', 'caption',
        'colgroup', 'col', 'blockquote', 'pre', 'code', 'h1', 'h2', 'h3', 'h4', 'h5', 'h6', 'hr',
        'img', 'figure', 'figcaption', 'dl', 'dt', 'dd', 'input']);
    const parser = new DOMParser();
    const doc = parser.parseFromString(`<body>${html}</body>`, 'text/html');
    const body = doc.body;

    function clean(node) {
        if (node.nodeType === Node.ELEMENT_NODE) {
            const tag = node.tagName.toLowerCase();
            if (!tagSet.has(tag) && !always.has(tag)) {
                // Replace disallowed element with its children
                const parent = node.parentNode;
                while (node.firstChild) parent.insertBefore(node.firstChild, node);
                parent.removeChild(node);
                return;
            }
            // Walk children in reverse to handle removals safely
            Array.from(node.childNodes).reverse().forEach(clean);
        }
    }

    Array.from(body.childNodes).reverse().forEach(clean);
    return body.innerHTML;
}

// ─── Paste from Office ─────────────────────────────────────────────────────

export function initPasteCleaner(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;

    el.addEventListener('paste', () => {
        requestAnimationFrame(() => {
            let html = el.innerHTML;
            const cleaned = _cleanOfficePaste(html);
            if (cleaned !== html) html = cleaned;
            // Apply HTML Support sanitization if configured
            const allowedTags = el.getAttribute('data-allowed-tags');
            if (allowedTags) {
                html = sanitizeHtml(html, JSON.parse(allowedTags));
            }
            if (html !== el.innerHTML) el.innerHTML = html;
        });
    });
}

function _cleanOfficePaste(html) {
    // Remove Word/Office XML cruft
    let c = html
        .replace(/<style[^>]*>[\s\S]*?<\/style>/gi, '')
        .replace(/<xml>[\s\S]*?<\/xml>/gi, '')
        .replace(/<!--\[if[^>]*>[\s\S]*?<!\[endif\]-->/gi, '')
        .replace(/<!--[\s\S]*?-->/g, '')
        .replace(/<o:p>[^<]*<\/o:p>/gi, '')
        .replace(/<o:[^>]+>[^<]*<\/o:>/gi, '')
        .replace(/<\/?span[^>]*>/gi, '')
        .replace(/<font[^>]*>/gi, '')
        .replace(/<\/font>/gi, '')
        .replace(/<v:[^>]+>[^<]*<\/v:>/gi, '')
        .replace(/<w:[^>]+>[^<]*<\/w:>/gi, '');

    // Remove mso-* and other Office inline styles
    c = c.replace(/\smso-[^=]+="[^"]*"/gi, '');
    c = c.replace(/\sstyle="[^"]*mso[^"]*"/gi, '');
    c = c.replace(/\sclass="[^"]*"/gi, '');

    // Google Docs specific: remove data-* and id targeting
    c = c.replace(/\sdata-[^=]+="[^"]*"/gi, '');
    c = c.replace(/\sid="docs-[^"]*"/gi, '');

    // Normalize list markers (Word uses custom paragraph styles for lists)
    // Convert Word list paragraphs to proper list items
    c = c.replace(/<p[^>]*style="[^"]*mso-list:[^"]*"[^>]*>\s*<!--\[if!supportLists\]-->[^<]*<span[^>]*>[^<]*<\/span><!--\[endif\]-->\s*(.*?)<\/p>/gi, '<li>$1</li>');

    // Clean empty tags
    c = c.replace(/<p>\s*<\/p>/gi, '<p><br></p>');

    // Remove excessive line breaks inside paragraphs
    c = c.replace(/<p><br\s*\/?><\/p>/gi, '<p><br></p>');

    return c;
}

// ─── Set sanitized content (called from C#) ────────────────────────────────

export function setSanitizedContent(editorId, html, allowedTags) {
    const el = document.getElementById(editorId);
    if (!el) return;
    if (allowedTags && allowedTags.length > 0) {
        html = sanitizeHtml(html, allowedTags);
    }
    el.innerHTML = html;
    const inst = window.__stiloGetInst(editorId);
    if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', html);
}

// ─── Autoformat (Markdown-style shortcuts) ────────────────────────────────────

export function initAutoformat(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;

    el.addEventListener('keydown', (e) => _onAutoformatKey(el, e));
    el.addEventListener('input', () => {
        clearTimeout(el._autoformatTimer);
        el._autoformatTimer = setTimeout(() => _processAutoformat(el), 400);
    });
}

function _onAutoformatKey(el, e) {
    if (e.key === 'Enter') {
        const sel = window.getSelection();
        if (!sel || sel.rangeCount === 0) return;
        const node = sel.getAnchorNode();
        if (!node || node.nodeType !== Node.TEXT_NODE) return;
        const text = node.textContent;
        const pre = text.substring(0, sel.getAnchorOffset()).trim();

        // --- → hr
        if (/^-{3,}$/.test(pre)) {
            e.preventDefault();
            const hr = document.createElement('hr');
            const p = node.parentElement?.closest('p, h1, h2, h3, h4, h5, h6, div, li');
            if (p) {
                p.parentNode.replaceChild(hr, p);
                hr.insertAdjacentElement('afterend', document.createElement('p'));
                _setCursor(hr.nextElementSibling, 0);
            }
            return;
        }

        // List continuation: after a list item with empty text → close list
        const listItem = node.parentElement?.closest('li');
        if (listItem && node.textContent.trim() === '') {
            e.preventDefault();
            const list = listItem.parentElement;
            const p = document.createElement('p');
            p.innerHTML = '<br>';
            list.parentNode.insertBefore(p, list.nextSibling);
            listItem.remove();
            if (!list.querySelector('li')) list.remove();
            _setCursor(p, 0);
            return;
        }
    }

    if (e.key === ' ') {
        const sel = window.getSelection();
        if (!sel || sel.rangeCount === 0) return;
        const node = sel.getAnchorNode();
        if (!node || node.nodeType !== Node.TEXT_NODE) return;
        const text = node.textContent;
        const pos = sel.getAnchorOffset();
        const lineStart = text.lastIndexOf('\n', pos - 1) + 1;
        const line = text.substring(lineStart, pos).trimEnd();

        // - [ ] or - [x] → task list
        const taskMatch = line.match(/^-\s\[([ x])\]\s(.+)/);
        if (taskMatch) {
            e.preventDefault();
            const p = node.parentElement?.closest('p, h1, h2, h3, h4, h5, h6, div, li');
            if (p) {
                const li = document.createElement('li');
                li.className = 'se-task-item';
                const cb = document.createElement('input');
                cb.type = 'checkbox';
                cb.checked = taskMatch[1] === 'x';
                cb.className = 'se-task-checkbox';
                const span = document.createElement('span');
                span.textContent = taskMatch[2];
                li.append(cb, span);
                const ul = document.createElement('ul');
                ul.className = 'se-task-list';
                ul.appendChild(li);
                p.parentNode.replaceChild(ul, p);
                _setCursor(span, span.textContent.length);
            }
            return;
        }

        // - or * → unordered list
        const ulMatch = line.match(/^[-*]\s(.+)/);
        if (ulMatch && !taskMatch) {
            e.preventDefault();
            const p = node.parentElement?.closest('p, h1, h2, h3, h4, h5, h6, div, li');
            if (p) {
                const li = document.createElement('li');
                li.textContent = ulMatch[1];
                const ul = document.createElement('ul');
                ul.appendChild(li);
                p.parentNode.replaceChild(ul, p);
                _setCursor(li, ulMatch[1].length);
            }
            return;
        }

        // 1. → ordered list
        const olMatch = line.match(/^(\d+)[.)]\s(.+)/);
        if (olMatch) {
            e.preventDefault();
            const p = node.parentElement?.closest('p, h1, h2, h3, h4, h5, h6, div, li');
            if (p) {
                const li = document.createElement('li');
                li.textContent = olMatch[2];
                const ol = document.createElement('ol');
                ol.appendChild(li);
                p.parentNode.replaceChild(ol, p);
                _setCursor(li, olMatch[2].length);
            }
            return;
        }

        // > → blockquote
        const bqMatch = line.match(/^>\s(.+)/);
        if (bqMatch) {
            e.preventDefault();
            const p = node.parentElement?.closest('p, h1, h2, h3, h4, h5, h6, div, li');
            if (p) {
                const bq = document.createElement('blockquote');
                const bp = document.createElement('p');
                bp.textContent = bqMatch[1];
                bq.appendChild(bp);
                p.parentNode.replaceChild(bq, p);
                _setCursor(bp, bqMatch[1].length);
            }
            return;
        }

        // # → heading
        const headingMatch = line.match(/^(#{1,6})\s(.+)/);
        if (headingMatch) {
            e.preventDefault();
            const level = headingMatch[1].length;
            const content = headingMatch[2];
            const p = node.parentElement?.closest('p, h1, h2, h3, h4, h5, h6, div, li');
            if (p) {
                const h = document.createElement('h' + level);
                h.textContent = content;
                p.parentNode.replaceChild(h, p);
                _setCursor(h, content.length);
            }
            return;
        }
    }
}

function _processAutoformat(el) {
    // Inline patterns: **bold**, *italic*, `code`, ~~strikethrough~~
    const walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT);
    const textNodes = [];
    while (walker.nextNode()) textNodes.push(walker.currentNode);

    for (const node of textNodes) {
        if (!node.textContent) continue;
        const text = node.textContent;

        // ~~strikethrough~~
        const sMatch = text.match(/~~(.+?)~~/);
        if (sMatch) {
            _replaceInlineNode(node, sMatch, (txt) => {
                const s = document.createElement('s');
                s.textContent = txt;
                return s;
            });
            el.normalize();
            return;
        }

        // `code`
        const cMatch = text.match(/`([^`]+)`/);
        if (cMatch) {
            _replaceInlineNode(node, cMatch, (txt) => {
                const c = document.createElement('code');
                c.textContent = txt;
                return c;
            });
            el.normalize();
            return;
        }

        // **bold**
        const bMatch = text.match(/\*\*(.+?)\*\*/);
        if (bMatch) {
            _replaceInlineNode(node, bMatch, (txt) => {
                const b = document.createElement('strong');
                b.textContent = txt;
                return b;
            });
            el.normalize();
            return;
        }

        // *italic*
        const iMatch = text.match(/(?<!\*)\*(?!\*)(.+?)(?<!\*)\*(?!\*)/);
        if (iMatch) {
            _replaceInlineNode(node, iMatch, (txt) => {
                const i = document.createElement('em');
                i.textContent = txt;
                return i;
            });
            el.normalize();
            return;
        }
    }
}

function _replaceInlineNode(textNode, match, createEl) {
    const text = textNode.textContent;
    const before = text.slice(0, match.index);
    const after = text.slice(match.index + match[0].length);
    const fragment = document.createDocumentFragment();
    if (before) fragment.appendChild(document.createTextNode(before));
    fragment.appendChild(createEl(match[1]));
    if (after) fragment.appendChild(document.createTextNode(after));
    textNode.parentNode.replaceChild(fragment, textNode);
}

function _wrapInline(fullText, match, createEl, editorEl) {
    // Legacy wrapper — used by paste-processed calls
    const html = fullText.slice(0, match.index) +
        createEl(match[1]).outerHTML +
        fullText.slice(match.index + match[0].length);
    editorEl.innerHTML = html;
    editorEl.normalize();
}

function _setCursor(parent, offset) {
    const sel = window.getSelection();
    const range = document.createRange();
    range.setStart(parent.firstChild || parent, offset);
    range.collapse(true);
    sel.removeAllRanges();
    sel.addRange(range);
}

// ─── Table Column Resize ─────────────────────────────────────────────────────

export function initTableResize(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;

    el.addEventListener('mousemove', (e) => {
        const cell = e.target.closest('td, th');
        if (!cell || !el.contains(cell)) return;

        const rect = cell.getBoundingClientRect();
        const edge = e.clientX - rect.left;

        // Show resize cursor near right edge
        if (edge > rect.width - 6 && edge < rect.width + 2) {
            el.style.cursor = 'col-resize';
            el._resizeCell = cell;
            el._resizeStartX = e.clientX;
            el._resizeStartW = rect.width;
        } else {
            el.style.cursor = '';
            el._resizeCell = null;
        }
    });

    el.addEventListener('mousedown', (e) => {
        if (!el._resizeCell) return;
        e.preventDefault();

        const cell = el._resizeCell;
        const table = cell.closest('table');
        if (!table) return;

        function onMouseMove(me) {
            const dx = me.clientX - el._resizeStartX;
            const colIndex = Array.from(cell.parentNode.children).indexOf(cell);
            const rows = table.querySelectorAll('tr');
            rows.forEach(r => {
                const c = r.children[colIndex];
                if (c) c.style.width = (parseFloat(c.getAttribute('data-width') || getComputedStyle(c).width || '100')) + dx + 'px';
            });
        }

        function onMouseUp() {
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);
            el.style.cursor = '';
            el._resizeCell = null;
        }

        // Store initial widths
        const colIndex = Array.from(cell.parentNode.children).indexOf(cell);
        const rows = table.querySelectorAll('tr');
        rows.forEach(r => {
            const c = r.children[colIndex];
            if (c) c.setAttribute('data-width', parseFloat(getComputedStyle(c).width));
        });

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
    });
}

// ─── Code Snippets ───────────────────────────────────────────────────────────

export function insertCodeSnippet(editorId, code, language) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.focus();

    let html = '<pre><code';
    if (language) html += ' class="language-' + _escapeAttr(language) + '"';
    html += '>' + _escapeHtml(code) + '</code></pre><p><br></p>';

    document.execCommand('insertHTML', false, html);
}

function _escapeHtml(str) {
    return str.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function _escapeAttr(str) {
    return str.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

// ─── MathJax / LaTeX ─────────────────────────────────────────────────────────

export function insertLatex(editorId, latex) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.focus();

    const span = document.createElement('span');
    span.className = 'se-math';
    span.contentEditable = 'false';
    span.textContent = latex;

    // Try to render with KaTeX if available
    if (window.katex) {
        try {
            span.innerHTML = window.katex.renderToString(latex, { throwOnError: false });
        } catch (_) {
            span.textContent = latex;
        }
    }

    document.execCommand('insertHTML', false, span.outerHTML + '&nbsp;');
}

// ─── Undo Manager (simple snapshot-based) ────────────────────────────────────

export function initUndoManager(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;

    el._undoStack = [];
    el._redoStack = [];
    el._undoTimer = null;
    el._undoSkipNext = false;

    // Take initial snapshot
    _pushSnapshot(el);

    el.addEventListener('input', () => {
        if (el._undoSkipNext) { el._undoSkipNext = false; return; }
        clearTimeout(el._undoTimer);
        el._undoTimer = setTimeout(() => _pushSnapshot(el), 300);
    });

    // Override Ctrl+Z / Ctrl+Y
    el.addEventListener('keydown', (e) => {
        if (e.ctrlKey && e.key === 'z') {
            e.preventDefault();
            _undoSnapshot(el);
        }
        if (e.ctrlKey && e.key === 'y') {
            e.preventDefault();
            _redoSnapshot(el);
        }
    });
}

function _pushSnapshot(el) {
    const html = el.innerHTML;
    if (el._undoStack.length === 0 || el._undoStack[el._undoStack.length - 1] !== html) {
        el._undoStack.push(html);
        el._redoStack = [];
        // Limit stack
        if (el._undoStack.length > 50) el._undoStack.shift();
    }
}

function _undoSnapshot(el) {
    if (el._undoStack.length <= 1) return;
    const current = el._undoStack.pop();
    el._redoStack.push(current);
    const prev = el._undoStack[el._undoStack.length - 1];
    el._undoSkipNext = true;
    el.innerHTML = prev;
}

function _redoSnapshot(el) {
    if (el._redoStack.length === 0) return;
    const next = el._redoStack.pop();
    el._undoStack.push(next);
    el._undoSkipNext = true;
    el.innerHTML = next;
}

// ─── Existing plugin functions below ─────────────────────────────────────────

// ─── Direction ──────────────────────────────────────────────────────────────

export function setDirection(editorId, dir) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.style.direction = dir;
    el.querySelectorAll('[style*="direction"]').forEach(e => e.style.direction = dir);
}

// ─── Insert helpers ─────────────────────────────────────────────────────────

export function insertHtml(editorId, html) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.focus();
    document.execCommand('insertHTML', false, html);
}

export function insertIframe(editorId, src, width, height, allowFullscreen) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.focus();
    const iframe = document.createElement('iframe');
    iframe.src = src;
    iframe.style.width = (width || 560) + 'px';
    iframe.style.height = (height || 315) + 'px';
    iframe.style.border = 'none';
    iframe.style.maxWidth = '100%';
    if (allowFullscreen) iframe.allowFullscreen = true;
    document.execCommand('insertHTML', false, iframe.outerHTML);
}

// ─── Find & Replace ──────────────────────────────────────────────────────────

function _escapeRegex(str) {
    return str.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}

export function findText(editorId, query, matchCase = false, wholeWord = false) {
    const el = document.getElementById(editorId);
    if (!el || !query) return 0;

    el.querySelectorAll('.se-fr-highlight').forEach(s => {
        const parent = s.parentNode;
        parent.replaceChild(document.createTextNode(s.textContent), s);
        parent.normalize();
    });

    let flags = 'g' + (matchCase ? '' : 'i');
    let pattern;
    if (wholeWord) {
        pattern = new RegExp('\\b' + _escapeRegex(query) + '\\b', flags);
    } else {
        pattern = new RegExp(_escapeRegex(query), flags);
    }

    const walker = document.createTreeWalker(el, NodeFilter.SHOW_TEXT);
    const nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);

    let count = 0;
    for (const node of nodes) {
        const text = node.textContent;
        if (!text) continue;
        let match;
        while ((match = pattern.exec(text)) !== null) {
            const range = document.createRange();
            range.setStart(node, match.index);
            range.setEnd(node, match.index + match[0].length);
            const span = document.createElement('span');
            span.className = 'se-fr-highlight';
            range.surroundContents(span);
            count++;
            break;
        }
    }
    return count;
}

export function replaceText(editorId, query, replacement, matchCase = false, wholeWord = false) {
    const el = document.getElementById(editorId);
    if (!el) return false;
    const highlight = el.querySelector('.se-fr-highlight');
    if (highlight) {
        highlight.replaceWith(document.createTextNode(replacement));
        el.normalize();
    }
    const count = findText(editorId, query, matchCase, wholeWord);
    if (count > 0) {
        const next = el.querySelector('.se-fr-highlight');
        if (next) {
            const range = document.createRange();
            range.selectNodeContents(next);
            const sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
            next.scrollIntoView({ block: 'center' });
        }
    }
    return count > 0;
}

export function replaceAllText(editorId, query, replacement, matchCase = false, wholeWord = false) {
    const el = document.getElementById(editorId);
    if (!el) return 0;

    el.querySelectorAll('.se-fr-highlight').forEach(s => {
        const parent = s.parentNode;
        parent.replaceChild(document.createTextNode(s.textContent), s);
        parent.normalize();
    });

    let flags = 'g' + (matchCase ? '' : 'i');
    let pattern;
    if (wholeWord) {
        pattern = new RegExp('\\b' + _escapeRegex(query) + '\\b', flags);
    } else {
        pattern = new RegExp(_escapeRegex(query), flags);
    }

    const html = el.innerHTML;
    const replaced = html.replace(pattern, replacement);
    el.innerHTML = replaced;
    return (replaced.match(new RegExp(_escapeRegex(replacement), 'g')) || []).length;
}

// ─── Magic Line ─────────────────────────────────────────────────────────────

export function initMagicLine(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;

    const btn = document.createElement('div');
    btn.className = 'se-magic-line';
    btn.innerHTML = '<svg viewBox="0 0 24 24" width="16" height="16"><path d="M19 13h-6v6h-2v-6H5v-2h6V5h2v6h6v2z"/></svg>';
    btn.title = 'Aggiungi contenuto qui';
    btn.style.display = 'none';
    el.parentElement.appendChild(btn);

    let magicTarget = null;
    let hideTimer = null;

    function posBtn(p) {
        const rect = p.getBoundingClientRect();
        const elRect = el.getBoundingClientRect();
        btn.style.display = 'flex';
        btn.style.top = (rect.top - elRect.top - 12) + 'px';
        btn.style.left = Math.max(0, rect.left - elRect.left - 20) + 'px';
    }

    function hideBtn() {
        if (hideTimer) clearTimeout(hideTimer);
        hideTimer = setTimeout(() => {
            btn.style.display = 'none';
            hideTimer = null;
        }, 200);
    }

    function cancelHide() {
        if (hideTimer) {
            clearTimeout(hideTimer);
            hideTimer = null;
        }
    }

    el.addEventListener('mouseover', (e) => {
        const p = e.target.closest('p, h1, h2, h3, h4, h5, h6, div, blockquote, pre, ul, ol, table, figure');
        if (p && p !== el && el.contains(p)) {
            magicTarget = p;
            posBtn(p);
            cancelHide();
        }
    });

    el.addEventListener('mouseleave', hideBtn);

    btn.addEventListener('mouseenter', cancelHide);
    btn.addEventListener('mouseleave', hideBtn);

    btn.addEventListener('click', () => {
        if (magicTarget) {
            const newP = document.createElement('p');
            newP.innerHTML = '<br>';
            magicTarget.parentNode.insertBefore(newP, magicTarget);
            const range = document.createRange();
            range.setStart(newP, 0);
            range.collapse(true);
            const sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
            newP.focus();
            btn.style.display = 'none';
        }
    });
}

// ─── Balloon Toolbar ─────────────────────────────────────────────────────────

export function initBalloonToolbar(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;

    const toolbar = document.createElement('div');
    toolbar.className = 'se-balloon';
    toolbar.innerHTML = [
        '<button data-cmd="bold" title="Grassetto"><svg viewBox="0 0 24 24" width="16" height="16"><path d="M15.6 10.79c.97-.67 1.65-1.77 1.65-2.79 0-2.26-1.75-4-4-4H7v14h7.04c2.09 0 3.71-1.7 3.71-3.79 0-1.52-.86-2.82-2.15-3.42zM10 6.5h3c.83 0 1.5.67 1.5 1.5s-.67 1.5-1.5 1.5h-3v-3zm3.5 9H10v-3h3.5c.83 0 1.5.67 1.5 1.5s-.67 1.5-1.5 1.5z"/></svg></button>',
        '<button data-cmd="italic" title="Corsivo"><svg viewBox="0 0 24 24" width="16" height="16"><path d="M10 4v3h2.21l-3.42 8H6v3h8v-3h-2.21l3.42-8H18V4z"/></svg></button>',
        '<button data-cmd="underline" title="Sottolineato"><svg viewBox="0 0 24 24" width="16" height="16"><path d="M12 17c3.31 0 6-2.69 6-6V3h-2.5v8c0 1.93-1.57 3.5-3.5 3.5S8.5 12.93 8.5 11V3H6v8c0 3.31 2.69 6 6 6zm-7 2v2h14v-2H5z"/></svg></button>',
        '<button data-cmd="createlink" title="Link"><svg viewBox="0 0 24 24" width="16" height="16"><path d="M3.9 12c0-1.71 1.39-3.1 3.1-3.1h4V7H7c-2.76 0-5 2.24-5 5s2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1zM8 13h8v-2H8v2zm9-6h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1s-1.39 3.1-3.1 3.1h-4V17h4c2.76 0 5-2.24 5-5s-2.24-5-5-5z"/></svg></button>'
    ].join('');
    toolbar.style.display = 'none';
    document.body.appendChild(toolbar);

    toolbar.addEventListener('click', (e) => {
        const btn = e.target.closest('button');
        if (!btn) return;
        const cmd = btn.dataset.cmd;
        el.focus();
        if (cmd === 'createlink') {
            const url = prompt('URL:');
            if (url) document.execCommand(cmd, false, url);
        } else {
            document.execCommand(cmd, false, null);
        }
        toolbar.style.display = 'none';
    });

    el.addEventListener('mouseup', () => {
        const sel = window.getSelection();
        if (sel && !sel.isCollapsed && el.contains(sel.anchorNode)) {
            const range = sel.getRangeAt(0);
            const rect = range.getBoundingClientRect();
            toolbar.style.display = 'flex';
            toolbar.style.top = (rect.top - 48) + 'px';
            toolbar.style.left = (rect.left + rect.width / 2 - 80) + 'px';
        }
    });

    document.addEventListener('mousedown', (e) => {
        if (!toolbar.contains(e.target)) toolbar.style.display = 'none';
    });
}

// ─── Context Menu Table ──────────────────────────────────────────────────────

export function initTableContextMenu(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;

    const menu = document.createElement('div');
    menu.className = 'se-table-menu';
    menu.innerHTML = [
        '<button data-action="add-row-before">Inserisci riga sopra</button>',
        '<button data-action="add-row-after">Inserisci riga sotto</button>',
        '<button data-action="add-col-before">Inserisci colonna prima</button>',
        '<button data-action="add-col-after">Inserisci colonna dopo</button>',
        '<hr>',
        '<button data-action="delete-row">Elimina riga</button>',
        '<button data-action="delete-col">Elimina colonna</button>',
        '<button data-action="delete-table">Elimina tabella</button>',
        '<hr>',
        '<button data-action="merge-cells">Unisci celle</button>',
        '<button data-action="split-cell">Dividi cella</button>',
        '<button data-action="cell-props">Proprietà cella</button>'
    ].join('');
    menu.style.display = 'none';
    document.body.appendChild(menu);

    let contextCell = null;

    function cellAction(action, cell) {
        const table = cell.closest('table');
        const tbody = table.querySelector('tbody') || table;
        const rows = Array.from(tbody.querySelectorAll('tr'));
        const cellIndex = Array.from(cell.parentNode.children).indexOf(cell);
        const rowIndex = rows.indexOf(cell.closest('tr'));

        switch (action) {
            case 'add-row-before': {
                const newRow = document.createElement('tr');
                const cols = rows[rowIndex].children.length;
                for (let i = 0; i < cols; i++) {
                    const td = document.createElement('td');
                    td.innerHTML = '<br>';
                    newRow.appendChild(td);
                }
                rows[rowIndex].parentNode.insertBefore(newRow, rows[rowIndex]);
                break;
            }
            case 'add-row-after': {
                const newRow = document.createElement('tr');
                const cols = rows[rowIndex].children.length;
                for (let i = 0; i < cols; i++) {
                    const td = document.createElement('td');
                    td.innerHTML = '<br>';
                    newRow.appendChild(td);
                }
                rows[rowIndex].parentNode.insertBefore(newRow, rows[rowIndex].nextSibling);
                break;
            }
            case 'add-col-before':
                rows.forEach(r => {
                    const td = document.createElement('td');
                    td.innerHTML = '<br>';
                    r.insertBefore(td, r.children[cellIndex]);
                });
                break;
            case 'add-col-after':
                rows.forEach(r => {
                    const td = document.createElement('td');
                    td.innerHTML = '<br>';
                    const ref = r.children[cellIndex + 1] || null;
                    r.insertBefore(td, ref);
                });
                break;
            case 'delete-row':
                rows[rowIndex].remove();
                break;
            case 'delete-col':
                rows.forEach(r => {
                    if (r.children[cellIndex]) r.children[cellIndex].remove();
                });
                break;
            case 'delete-table':
                table.remove();
                break;
            case 'merge-cells': {
                const sel = window.getSelection();
                if (!sel || sel.isCollapsed) break;
                const range = sel.getRangeAt(0);
                const common = range.commonAncestorContainer;
                const cells = common.closest('td, th');
                if (cells) cells.setAttribute('colspan', '2');
                break;
            }
            case 'split-cell': {
                const colspan = cell.getAttribute('colspan');
                if (colspan && parseInt(colspan) > 1) {
                    cell.removeAttribute('colspan');
                    const td = document.createElement('td');
                    td.innerHTML = '<br>';
                    cell.parentNode.insertBefore(td, cell.nextSibling);
                }
                break;
            }
            case 'cell-props':
                cell.style.backgroundColor = prompt('Colore sfondo:', cell.style.backgroundColor || '#ffffff');
                break;
        }
    }

    el.addEventListener('contextmenu', (e) => {
        const cell = e.target.closest('td, th');
        if (cell && el.contains(cell)) {
            e.preventDefault();
            contextCell = cell;
            menu.style.display = 'block';
            menu.style.top = e.clientY + 'px';
            menu.style.left = e.clientX + 'px';
        } else {
            menu.style.display = 'none';
        }
    });

    menu.addEventListener('click', (e) => {
        const btn = e.target.closest('button');
        if (!btn) return;
        if (contextCell) cellAction(btn.dataset.action, contextCell);
        menu.style.display = 'none';
    });

    document.addEventListener('click', (e) => {
        if (!menu.contains(e.target)) menu.style.display = 'none';
    });
}

// ─── Print Preview ───────────────────────────────────────────────────────────

export function printPreview(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    const content = el.innerHTML;
    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write('<!DOCTYPE html><html><head><meta charset="utf-8">');
    win.document.write('<title>Anteprima di stampa</title>');
    win.document.write('<style>body{font-family:sans-serif;margin:20mm}table{border-collapse:collapse;width:100%}td,th{border:1px solid #ccc;padding:6px}</style>');
    win.document.write('</head><body>' + content + '</body></html>');
    win.document.close();
    win.focus();
}

window.__stiloPrintPreview = (html) => {
    const win = window.open('', '_blank');
    if (!win) return;
    win.document.write('<!DOCTYPE html><html><head><meta charset="utf-8">');
    win.document.write('<title>Anteprima di stampa</title>');
    win.document.write('<style>body{font-family:sans-serif;margin:20mm}table{border-collapse:collapse;width:100%}td,th{border:1px solid #ccc;padding:6px}</style>');
    win.document.write('</head><body>' + html + '</body></body></html>');
    win.document.close();
    win.focus();
};

// ─── Globals for FindReplaceDialog (receives editorId from caller) ────────────

window.__stiloFindText = (editorId, query, matchCase) => {
    const result = findText(editorId, query, matchCase, false);
    const el = document.getElementById(editorId);
    if (result > 0 && el) {
        const hl = el.querySelector('.se-fr-highlight');
        if (hl) hl.scrollIntoView({ block: 'center' });
    }
    return result > 0;
};

window.__stiloReplace = (editorId, query, replacement, matchCase) => {
    return replaceText(editorId, query, replacement, matchCase, false);
};

window.__stiloReplaceAll = (editorId, query, replacement, matchCase) => {
    return replaceAllText(editorId, query, replacement, matchCase, false);
};

// ─── Text-to-Speech (TTS) — hover mode ──────────────────────────────────────

export function isSpeechSupported() {
    return 'speechSynthesis' in window;
}

let _ttsUtterance = null;

export function cancelSpeech() {
    const synth = window.speechSynthesis;
    if (!synth) return;
    synth.cancel();
    _ttsUtterance = null;
}

function _ttsSpeakElement(element, lang, rate, pitch) {
    const synth = window.speechSynthesis;
    if (!synth) return;

    // For inputs/textareas: read placeholder
    const placeholder = element.getAttribute('placeholder') || '';
    if (placeholder && (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA' || element.tagName === 'SELECT')) {
        synth.cancel();
        const utterance = new SpeechSynthesisUtterance(placeholder);
        if (lang) utterance.lang = lang.replace('_', '-');
        utterance.rate = rate || 1.0;
        utterance.pitch = pitch || 1.0;
        const langPrefix = (lang || '').substring(0, 2);
        const voice = synth.getVoices().find(v => v.lang && v.lang.startsWith(langPrefix));
        if (voice) utterance.voice = voice;
        _ttsUtterance = utterance;
        synth.speak(utterance);
        return;
    }

    // For buttons and controls: use title or text content
    let text = element.getAttribute('title') || '';
    if (!text) {
        // For editor text: find closest block
        const block = element.closest('p, h1, h2, h3, h4, h5, h6, li, td, th, blockquote, pre, div.se-content');
        text = block ? (block.innerText || block.textContent || '').trim() : '';
    }
    if (!text) {
        text = (element.innerText || element.textContent || '').trim();
    }
    if (!text) return;

    synth.cancel();
    const utterance = new SpeechSynthesisUtterance(text);
    if (lang) utterance.lang = lang.replace('_', '-');
    utterance.rate = rate || 1.0;
    utterance.pitch = pitch || 1.0;
    const langPrefix = (lang || '').substring(0, 2);
    const voice = synth.getVoices().find(v => v.lang && v.lang.startsWith(langPrefix));
    if (voice) utterance.voice = voice;
    _ttsUtterance = utterance;
    synth.speak(utterance);
}

export function initTtsHover(editorId, lang, rate, pitch) {
    const editor = document.getElementById(editorId);
    if (!editor) return;
    const root = editor.closest('.stilo-editor');
    if (!root) return;
    if (root._ttsHoverInit) return;
    root._ttsHoverInit = true;

    let activeEl = null;

    root.addEventListener('mouseover', (e) => {
        if (!root._ttsHoverActive) return;
        const target = e.target;
        if (target === activeEl) return;
        activeEl = target;

        // For UI controls
        const ctrl = target.closest('button, .se-ribbon-tab, .se-a11y-btn, .se-action-btn, .se-scroll-btn, .se-statusbar-btn, .se-btn, .se-tdd-card, .se-tdd-small-card, .se-format-item, .se-fly-tab, select.se-select, .se-group-label, .se-tdd-section');
        if (ctrl) {
            _ttsSpeakElement(ctrl, lang, rate, pitch);
            return;
        }

        // For editor content — speak the closest block
        if (root.contains(target) && editor.contains(target)) {
            _ttsSpeakElement(target, lang, rate, pitch);
        }
    });

    root.addEventListener('mouseout', (e) => {
        if (!root._ttsHoverActive) return;
        const rel = e.relatedTarget;
        if (!rel || !root.contains(rel)) {
            activeEl = null;
            cancelSpeech();
        }
    });
}

export function toggleTtsHover(editorId, lang, rate, pitch) {
    const editor = document.getElementById(editorId);
    if (!editor) return false;
    const root = editor.closest('.stilo-editor');
    if (!root) return false;
    root._ttsHoverActive = !root._ttsHoverActive;
    if (!root._ttsHoverActive) {
        cancelSpeech();
    }
    return root._ttsHoverActive;
}

export function isTtsHoverActive(editorId) {
    const editor = document.getElementById(editorId);
    if (!editor) return false;
    const root = editor.closest('.stilo-editor');
    return root ? !!root._ttsHoverActive : false;
}

// ─── Magnifier (lente d'ingrandimento) ───────────────────────────────────────

export function initMagnifier(editorId) {
    const editor = document.getElementById(editorId);
    if (!editor) return;
    const root = editor.closest('.stilo-editor');
    if (!root) return;
    if (root._magnifierEl) return;

    const magnifier = document.createElement('div');
    magnifier.className = 'se-magnifier';
    magnifier.innerHTML = '<div class="se-magnifier-content"></div>';
    document.body.appendChild(magnifier);

    root._magnifierActive = false;
    root._magnifierEl = magnifier;

    document.addEventListener('mousemove', (e) => {
        if (!root._magnifierActive) return;
        const el = e.target;
        if (!el || !root.contains(el)) {
            magnifier.style.display = 'none';
            document.body.classList.remove('se-magnifier-active');
            return;
        }

        magnifier.style.display = 'flex';
        document.body.classList.add('se-magnifier-active');
        magnifier.style.left = (e.clientX + 25) + 'px';
        magnifier.style.top = (e.clientY + 25) + 'px';

        const content = magnifier.querySelector('.se-magnifier-content');
        content.innerHTML = '';

        const btn = el.closest('button, [class*="se-btn"], [class*="se-ribbon-tab"], [class*="se-a11y-btn"], [class*="se-action-btn"], [class*="se-scroll-btn"], [class*="se-statusbar-btn"], [class*="se-fly-tab"], [class*="se-tdd-card"], [class*="se-tdd-small-card"], [class*="se-format-item"]');
        if (btn) {
            const svg = btn.querySelector('svg');
            if (svg) {
                const clone = svg.cloneNode(true);
                clone.style.width = '70px';
                clone.style.height = '70px';
                clone.style.display = 'block';
                clone.style.margin = '0 auto';
                content.appendChild(clone);
            }
            const title = btn.getAttribute('title') || btn.textContent?.trim() || '';
            if (title) {
                const lbl = document.createElement('div');
                lbl.className = 'se-magnifier-label';
                lbl.textContent = title;
                content.appendChild(lbl);
            }
            return;
        }

        const txt = el.innerText || el.textContent || '';
        if (txt.trim()) {
            const rect = el.getBoundingClientRect();
            if (rect.width > 0 && rect.height > 0) {
                const xRel = e.clientX - rect.left;
                const len = txt.length;
                const charIdx = Math.floor((xRel / rect.width) * len);
                const half = 20;
                const start = Math.max(0, charIdx - half);
                const end = Math.min(len, charIdx + half);
                const snippet = txt.substring(start, end);
                const div = document.createElement('div');
                div.className = 'se-magnifier-text';
                div.textContent = snippet;
                content.appendChild(div);
            } else {
                const div = document.createElement('div');
                div.className = 'se-magnifier-text';
                div.textContent = txt.substring(0, 40);
                content.appendChild(div);
            }
        }
    });
}

export function toggleMagnifier(editorId) {
    const editor = document.getElementById(editorId);
    if (!editor) return false;
    const root = editor.closest('.stilo-editor');
    if (!root) return false;
    if (!root._magnifierEl) return false;
    root._magnifierActive = !root._magnifierActive;
    if (!root._magnifierActive) {
        root._magnifierEl.style.display = 'none';
        document.body.classList.remove('se-magnifier-active');
    }
    return root._magnifierActive;
}

export function isMagnifierActive(editorId) {
    const editor = document.getElementById(editorId);
    if (!editor) return false;
    const root = editor.closest('.stilo-editor');
    return root ? !!root._magnifierActive : false;
}

// ─── Object toolbar helpers ──────────────────────────────────────────────────

function _getImg(editorId) {
    const inst = window.__stiloGetInst(editorId);
    return inst?.lastClickedImg || null;
}

function _getTable(editorId) {
    const inst = window.__stiloGetInst(editorId);
    return inst?.lastClickedTable || null;
}

function _getEditorEl(editorId) {
    const el = document.getElementById(editorId);
    return el ? el.closest('.stilo-editor') : null;
}

export function resizeImage(editorId, preset) {
    const img = _getImg(editorId);
    if (!img) return;
    const sizes = { small: 150, medium: 300, large: 600, original: 0 };
    const w = sizes[preset];
    if (w) {
        img.style.width = w + 'px';
        img.style.height = 'auto';
    } else {
        img.style.width = '';
        img.style.height = '';
    }
    const inst = window.__stiloGetInst(editorId);
    if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', img.closest('.se-content')?.innerHTML || '');
}

export async function applyImageFilter(editorId, filter) {
    const img = _getImg(editorId);
    if (!img) return;
    let src = img.getAttribute('src') || '';
    if (!src.startsWith('data:')) {
        // External URL: load into canvas first
        try {
            src = await _toDataUrl(src);
        } catch { return; }
    }
    const inst = window.__stiloGetInst(editorId);
    if (!inst) return;
    try {
        const result = await inst.dotNetRef.invokeMethodAsync('ProcessImageAsync', src, filter);
        if (result && result.startsWith('data:')) {
            img.setAttribute('src', result);
        }
    } catch { /* fall through */ }
}

function _toDataUrl(url) {
    return new Promise((resolve, reject) => {
        const c = document.createElement('canvas');
        const i = new Image();
        i.crossOrigin = 'anonymous';
        i.onload = () => {
            c.width = i.naturalWidth;
            c.height = i.naturalHeight;
            c.getContext('2d').drawImage(i, 0, 0);
            resolve(c.toDataURL('image/png'));
        };
        i.onerror = reject;
        i.src = url;
    });
}

export function setImageAlt(editorId, alt) {
    const img = _getImg(editorId);
    if (!img) return;
    img.alt = alt;
}

export function toggleImageCaption(editorId) {
    const img = _getImg(editorId);
    if (!img) return;
    const parent = img.parentNode;
    if (parent && parent.nodeName === 'FIGURE' && parent.classList.contains('se-image-caption')) {
        const figcaption = parent.querySelector('figcaption');
        const txt = figcaption ? figcaption.innerHTML : '';
        parent.parentNode?.insertBefore(img, parent);
        const br = document.createElement('br');
        img.parentNode?.insertBefore(br, img.nextSibling);
        parent.remove();
    } else {
        const figure = document.createElement('figure');
        figure.className = 'se-image-caption';
        img.parentNode?.insertBefore(figure, img);
        figure.appendChild(img);
        const figcaption = document.createElement('figcaption');
        figcaption.contentEditable = 'true';
        figcaption.innerText = 'Caption';
        figure.appendChild(figcaption);
    }
    const inst = window.__stiloGetInst(editorId);
    if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', img.closest('.se-content')?.innerHTML || '');
}

export function replaceImageSource(editorId) {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = async () => {
        const file = input.files?.[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = async (e) => {
            const img = _getImg(editorId);
            if (!img) return;
            img.setAttribute('src', e.target.result);
            const inst = window.__stiloGetInst(editorId);
            if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', img.closest('.se-content')?.innerHTML || '');
        };
        reader.readAsDataURL(file);
    };
    input.click();
}

export function applyStylePreset(editorId, id, cssClass, css) {
    const inst = window.__stiloGetInst(editorId);
    if (!inst) return;
    const sel = window.getSelection();
    if (!sel || !sel.rangeCount) return;
    const range = sel.getRangeAt(0);
    if (range.collapsed) return;

    // Check if selection already has this preset class
    let hasPreset = false;
    const parentEl = range.commonAncestorContainer;
    if (parentEl.nodeType === 1) {
        hasPreset = parentEl.classList?.contains(cssClass);
    } else if (parentEl.parentNode) {
        hasPreset = parentEl.parentNode.classList?.contains(cssClass);
    }

    if (hasPreset) {
        // Remove the preset
        const els = inst.el.querySelectorAll('.' + cssClass);
        for (const el of els) {
            el.classList.remove(cssClass);
            el.removeAttribute('style');
            // unwrap if it's a span with only this class
            if (el.tagName === 'SPAN' && el.className.trim() === '' && el.parentNode) {
                el.parentNode.replaceChild(document.createTextNode(el.textContent), el);
            }
        }
    } else {
        // Apply the preset
        const contents = range.extractContents();
        const wrapper = document.createElement('span');
        wrapper.className = cssClass;
        wrapper.setAttribute('style', css);
        wrapper.appendChild(contents);
        range.insertNode(wrapper);
        // Normalize to avoid empty text nodes
        range.commonAncestorContainer.normalize && range.commonAncestorContainer.normalize();
    }
    inst.dotNetRef.invokeMethodAsync('OnContentChanged', inst.el.innerHTML);
}

// ─── Page Break ─────────────────────────────────────────────────────────────

export function insertPageBreak(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.focus();
    const div = document.createElement('div');
    div.className = 'se-page-break';
    div.contentEditable = 'false';
    const span = document.createElement('span');
    span.textContent = 'Page Break';
    div.appendChild(span);
    document.execCommand('insertHTML', false, div.outerHTML + '<p><br></p>');
}

// ─── Text Part Language (WCAG) ──────────────────────────────────────────────

export function applyTextLanguage(editorId, lang, dir) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.focus();
    const sel = window.getSelection();
    if (!sel || sel.isCollapsed || !sel.rangeCount) return;
    const range = sel.getRangeAt(0);
    if (!lang && !dir) {
        // Remove language markup from selection
        const parent = range.commonAncestorContainer;
        const span = parent.nodeType === 1
            ? parent.closest('[lang]')
            : parent.parentElement?.closest('[lang]');
        if (span && span.tagName === 'SPAN') {
            span.removeAttribute('lang');
            span.removeAttribute('dir');
            if (!span.getAttributeNames().length) {
                const parent2 = span.parentNode;
                while (span.firstChild) parent2.insertBefore(span.firstChild, span);
                span.remove();
                parent2.normalize();
            }
        }
        return;
    }
    const contents = range.extractContents();
    const wrapper = document.createElement('span');
    if (lang) wrapper.setAttribute('lang', lang);
    if (dir) wrapper.setAttribute('dir', dir);
    wrapper.appendChild(contents);
    range.insertNode(wrapper);
    range.commonAncestorContainer.normalize?.();
    const inst = window.__stiloGetInst(editorId);
    if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', inst.el.innerHTML);
}

export function setImageLink(editorId) {
    const img = _getImg(editorId);
    if (!img) return;
    const existingLink = img.closest('a');
    const url = prompt(existingLink ? 'Edit link URL:' : 'Enter link URL:', existingLink?.href || 'https://');
    if (!url) {
        if (existingLink) {
            const parent = existingLink.parentNode;
            existingLink.parentNode?.insertBefore(img, existingLink);
            existingLink.remove();
        }
        return;
    }
    if (existingLink) {
        existingLink.href = url;
    } else {
        const a = document.createElement('a');
        a.href = url;
        a.target = '_blank';
        img.parentNode?.insertBefore(a, img);
        a.appendChild(img);
    }
    const inst = window.__stiloGetInst(editorId);
    if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', img.closest('.se-content')?.innerHTML || '');
}

// ─── Minimap ────────────────────────────────────────────────────────────────────

export function initMinimap(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    const minimap = document.getElementById(editorId + '-minimap');
    if (!minimap) return;
    const content = minimap.querySelector('.se-minimap-content');
    if (!content) return;

    el._minimapData = { minimap, content, editorId };

    el.addEventListener('scroll', () => _updateMinimap(el));

    minimap.addEventListener('click', (e) => _onMinimapClick(el, minimap, e));

    const obs = new MutationObserver(() => _rebuildMinimapContent(el));
    obs.observe(el, { childList: true, subtree: true, characterData: true });
    el._minimapObserver = obs;

    _rebuildMinimapContent(el);
    _updateMinimap(el);
}

function _rebuildMinimapContent(el) {
    const data = el._minimapData;
    if (!data) return;
    const clone = el.cloneNode(true);
    clone.querySelectorAll('img, table, iframe, video, svg, .se-math, .se-codeblock').forEach(n => {
        const ph = document.createElement('span');
        ph.className = 'se-minimap-ph';
        ph.textContent = '\u2588';
        n.parentNode?.replaceChild(ph, n);
    });
    data.content.innerHTML = '';
    data.content.appendChild(clone);
    _updateMinimap(el);
}

function _updateMinimap(el) {
    const data = el._minimapData;
    if (!data) return;
    const { minimap, content } = data;
    const contentRect = content.getBoundingClientRect();
    if (contentRect.height === 0) return;
    const scaleX = minimap.clientWidth / contentRect.width;
    const scaleY = minimap.clientHeight / contentRect.height;
    const s = Math.min(scaleX, scaleY, 0.5);
    content.style.transform = `scale(${s})`;
    content.style.transformOrigin = 'top left';

    let indicator = minimap.querySelector('.se-minimap-indicator');
    if (!indicator) {
        indicator = document.createElement('div');
        indicator.className = 'se-minimap-indicator';
        minimap.appendChild(indicator);
    }
    const sh = el.scrollHeight || 1;
    const ch = el.clientHeight || 1;
    const scrollRatio = el.scrollTop / (sh - ch || 1);
    const indHeight = (ch / sh) * minimap.clientHeight;
    const indTop = scrollRatio * (minimap.clientHeight - indHeight);
    indicator.style.height = Math.max(16, indHeight) + 'px';
    indicator.style.top = indTop + 'px';
}

function _onMinimapClick(el, minimap, e) {
    const rect = minimap.getBoundingClientRect();
    const y = e.clientY - rect.top;
    const ratio = y / minimap.clientHeight;
    el.scrollTop = ratio * (el.scrollHeight - el.clientHeight);
}

// ─── Restricted Editing ────────────────────────────────────────────────────────

export function initRestrictedEditing(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el._restrictedEditing = true;
}

export function lockSelection(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0 || sel.isCollapsed) return;
    const range = sel.getRangeAt(0);
    const wrapper = document.createElement('div');
    wrapper.className = 'se-locked';
    wrapper.contentEditable = 'false';
    try {
        range.surroundContents(wrapper);
    } catch {
        // surroundContents fails if range spans multiple elements; use fragment approach
        const fragment = range.extractContents();
        wrapper.appendChild(fragment);
        range.insertNode(wrapper);
    }
    sel.removeAllRanges();
    _setCursor(wrapper, 0);
    const inst = window.__stiloGetInst(editorId);
    if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', el.innerHTML);
}

export function unlockRegion(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return;
    const node = sel.getAnchorNode();
    if (!node) return;
    const locked = node.parentElement?.closest('.se-locked');
    if (!locked) return;
    // Unwrap: replace .se-locked with its children
    const parent = locked.parentNode;
    while (locked.firstChild) parent.insertBefore(locked.firstChild, locked);
    parent.removeChild(locked);
    parent.normalize();
    const inst = window.__stiloGetInst(editorId);
    if (inst) inst.dotNetRef.invokeMethodAsync('OnContentChanged', el.innerHTML);
}

export function toggleLockDisplay(editorId) {
    const el = document.getElementById(editorId);
    if (!el) return;
    el.classList.toggle('se-show-locks');
}