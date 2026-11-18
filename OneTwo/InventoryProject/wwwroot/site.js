// Focus a DOM element by its id
window.focusElement = function (id) {
    var el = document.getElementById(id);
    if (el) {
        el.focus();
        if (el.select) el.select();
    }
};

// Global listener to navigate to the next field when Enter is pressed
document.addEventListener('keydown', function (e) {
    if (e.key === 'Enter') {
        var target = e.target;
        if (!target) return;
        
        var tag = target.tagName.toUpperCase();
        var type = (target.type || '').toLowerCase();
        
        // Allow default behavior for textareas (newlines) and buttons (clicks)
        if (tag === 'TEXTAREA' || tag === 'BUTTON' || type === 'submit' || type === 'button') {
            return;
        }
        
        if (tag === 'INPUT' || tag === 'SELECT') {
            // Scope navigation to the active modal card, form, or document body
            var container = target.closest('.modal-card') || target.closest('.modal-inner') || target.closest('form') || document.body;
            
            // Selector for standard editable fields
            var selectors = 'input:not([type="hidden"]):not([type="submit"]):not([type="button"]):not([disabled]):not([readonly]), select:not([disabled]), textarea:not([disabled])';
            var elements = Array.prototype.slice.call(container.querySelectorAll(selectors));
            
            // Filter elements to only include visible ones
            elements = elements.filter(function (el) {
                return el.offsetWidth > 0 || el.offsetHeight > 0 || el.getClientRects().length > 0;
            });
            
            var index = elements.indexOf(target);
            if (index > -1 && index < elements.length - 1) {
                e.preventDefault(); // Prevent accidental form submission
                var nextEl = elements[index + 1];
                nextEl.focus();
                if (nextEl.select && typeof nextEl.select === 'function') {
                    nextEl.select();
                }
            } else if (index === elements.length - 1) {
                e.preventDefault(); // Stop default submit at the last field to let user verify
            }
        }
    }
});
