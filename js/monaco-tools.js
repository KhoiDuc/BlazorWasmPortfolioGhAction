window.monacoTools = (function () {
    var vsPath = 'https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs';
    var loading;

    function ensureMonaco() {
        if (window.monaco) return Promise.resolve(window.monaco);
        if (loading) return loading;

        loading = new Promise(function (resolve, reject) {
            var script = document.createElement('script');
            script.src = vsPath + '/loader.js';
            script.onload = function () {
                require.config({ paths: { vs: vsPath } });
                require(['vs/editor/editor.main'], function () {
                    resolve(window.monaco);
                }, reject);
            };
            script.onerror = reject;
            document.head.appendChild(script);
        });
        return loading;
    }

    var diffEditor;
    var singleEditor;
    var dotNetRef;

    return {
        ensure: ensureMonaco,

        setDotNetRef: function (ref) { dotNetRef = ref; },

        createDiff: async function (containerId, original, modified, language) {
            await ensureMonaco();
            var el = document.getElementById(containerId);
            if (!el) throw new Error('Diff container not found');
            if (diffEditor) {
                diffEditor.dispose();
                diffEditor = null;
            }
            var originalModel = monaco.editor.createModel(original || '', language || 'plaintext');
            var modifiedModel = monaco.editor.createModel(modified || '', language || 'plaintext');
            diffEditor = monaco.editor.createDiffEditor(el, {
                originalEditable: true,
                automaticLayout: true,
                renderSideBySide: true,
                theme: 'vs'
            });
            diffEditor.setModel({ original: originalModel, modified: modifiedModel });
            originalModel.onDidChangeContent(function () {
                if (dotNetRef) dotNetRef.invokeMethodAsync('OnDiffContentChanged', originalModel.getValue(), true);
            });
            modifiedModel.onDidChangeContent(function () {
                if (dotNetRef) dotNetRef.invokeMethodAsync('OnDiffContentChanged', modifiedModel.getValue(), false);
            });
        },

        getDiffContent: function () {
            if (!diffEditor) return { original: '', modified: '' };
            var m = diffEditor.getModel();
            return { original: m.original.getValue(), modified: m.modified.getValue() };
        },

        setDiffContent: function (original, modified) {
            if (!diffEditor) return;
            var m = diffEditor.getModel();
            m.original.setValue(original || '');
            m.modified.setValue(modified || '');
        },

        setDiffLanguage: function (language) {
            if (!diffEditor) return;
            var m = diffEditor.getModel();
            monaco.editor.setModelLanguage(m.original, language);
            monaco.editor.setModelLanguage(m.modified, language);
        },

        setDiffWordWrap: function (on) {
            if (!diffEditor) return;
            var wrap = on ? 'on' : 'off';
            diffEditor.getOriginalEditor().updateOptions({ wordWrap: wrap });
            diffEditor.getModifiedEditor().updateOptions({ wordWrap: wrap });
        },

        formatDiff: async function () {
            if (!diffEditor) return { ok: false, error: 'Editor not ready' };
            var m = diffEditor.getModel();
            var lang = m.original.getLanguageId();
            if (lang === 'json') {
                try { JSON.parse(m.original.getValue()); } catch (e) { return { ok: false, error: 'Original JSON invalid: ' + e.message }; }
                try { JSON.parse(m.modified.getValue()); } catch (e) { return { ok: false, error: 'Modified JSON invalid: ' + e.message }; }
            }
            await diffEditor.getOriginalEditor().getAction('editor.action.formatDocument').run();
            await diffEditor.getModifiedEditor().getAction('editor.action.formatDocument').run();
            return { ok: true };
        },

        disposeDiff: function () {
            if (diffEditor) { diffEditor.dispose(); diffEditor = null; }
        },

        createEditor: async function (containerId, value, language) {
            await ensureMonaco();
            var el = document.getElementById(containerId);
            if (!el) throw new Error('Editor container not found');
            if (singleEditor) {
                singleEditor.dispose();
                singleEditor = null;
            }
            singleEditor = monaco.editor.create(el, {
                value: value || '',
                language: language || 'plaintext',
                automaticLayout: true,
                minimap: { enabled: false },
                theme: 'vs',
                wordWrap: 'on'
            });
            singleEditor.onDidChangeModelContent(function () {
                if (dotNetRef) dotNetRef.invokeMethodAsync('OnPasteContentChanged', singleEditor.getValue());
            });
        },

        getEditorValue: function () {
            return singleEditor ? singleEditor.getValue() : '';
        },

        setEditorValue: function (value) {
            if (singleEditor) singleEditor.setValue(value || '');
        },

        setEditorLanguage: function (language) {
            if (!singleEditor) return;
            monaco.editor.setModelLanguage(singleEditor.getModel(), language);
        },

        setEditorWordWrap: function (on) {
            if (singleEditor) singleEditor.updateOptions({ wordWrap: on ? 'on' : 'off' });
        },

        formatEditor: async function () {
            if (!singleEditor) return { ok: false, error: 'Editor not ready' };
            var lang = singleEditor.getModel().getLanguageId();
            if (lang === 'json') {
                try { JSON.parse(singleEditor.getValue()); } catch (e) { return { ok: false, error: e.message }; }
            }
            await singleEditor.getAction('editor.action.formatDocument').run();
            return { ok: true };
        },

        getLanguages: async function () {
            await ensureMonaco();
            return monaco.languages.getLanguages().map(function (l) {
                return { id: l.id, aliases: l.aliases || [] };
            });
        },

        disposeEditor: function () {
            if (singleEditor) { singleEditor.dispose(); singleEditor = null; }
        }
    };
})();
