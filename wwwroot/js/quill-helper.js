/**
 * Quill Helper for LabCare SFRichTextEditor
 */
window.QuillHelper = {
    editors: {},

    init: function (editorElement, toolbarElement, placeholder, initialValue, dotNetHelper) {
        if (!editorElement) return;

        // Register Size styles if not already done
        var Size = Quill.import('attributors/style/size');
        Size.whitelist = ['8px', '10px', '12px', '14px', '16px', '18px', '20px', '24px', '32px', '48px'];
        Quill.register(Size, true);

        // Register Font styles
        var Font = Quill.import('attributors/style/font');
        Font.whitelist = ['sans-serif', 'serif', 'monospace', 'roboto', 'inter', 'public-sans'];
        Quill.register(Font, true);

        var quill = new Quill(editorElement, {
            modules: {
                toolbar: toolbarElement
            },
            placeholder: placeholder || 'Compose an epic...',
            theme: 'snow'
        });

        if (initialValue) {
            quill.root.innerHTML = initialValue;
        }

        quill.on('text-change', function () {
            dotNetHelper.invokeMethodAsync('OnContentChanged', quill.root.innerHTML);
        });

        this.editors[editorElement.id] = quill;
    },

    getHtml: function (id) {
        var quill = this.editors[id];
        if (quill) {
            return quill.root.innerHTML;
        }
        return "";
    },

    setHtml: function (id, html) {
        var quill = this.editors[id];
        if (quill) {
            quill.root.innerHTML = html || "";
        }
    },

    changeFontSize: function (id, delta) {
        var quill = this.editors[id];
        if (!quill) return;

        var range = quill.getSelection();
        if (range) {
            var currentFormat = quill.getFormat(range);
            var currentSize = currentFormat.size || '14px'; // default
            
            var sizeInt = parseInt(currentSize);
            var newSize = (sizeInt + delta) + 'px';
            
            // Constrain
            if (sizeInt + delta < 8) newSize = '8px';
            if (sizeInt + delta > 72) newSize = '72px';

            quill.format('size', newSize);
        }
    }
};
