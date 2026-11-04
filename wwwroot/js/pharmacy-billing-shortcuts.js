// Pharmacy Billing Keyboard Shortcuts & Mouse-Free Traversal Controller
(function () {
    window.pharmacyBilling = {
        dotNetHelper: null,
        isInitialized: false,

        init: function (dotNet) {
            this.dotNetHelper = dotNet;
            if (!this.isInitialized) {
                this.isInitialized = true;
                this.attachListeners();
            }
        },

        dispose: function () {
            this.dotNetHelper = null;
        },

        focusElement: function (id) {
            function tryFocus() {
                var el = document.getElementById(id);
                if (el) {
                    el.focus();
                    if (typeof el.select === 'function' && el.tagName !== 'SELECT' && el.type !== 'button') {
                        el.select();
                    }
                    return true;
                }
                return false;
            }
            if (!tryFocus()) {
                setTimeout(tryFocus, 60);
                setTimeout(tryFocus, 180);
            }
        },

        isModalActive: function () {
            return document.querySelector('.modal-card') !== null ||
                   document.querySelector('.modal-overlay') !== null ||
                   document.querySelector('.purchase-items-table-wrapper') !== null ||
                   document.querySelector('.purchase-bill-modal') !== null;
        },

        getNavSequence: function () {
            var seq = [];
            function addIfFound(id) {
                var el = document.getElementById(id);
                if (el && el.offsetParent !== null && !el.disabled) {
                    seq.push(el);
                }
            }

            addIfFound('sales_branch_picker');
            addIfFound('sales_counter_picker');
            addIfFound('sales_type_select');

            // OP / IP / Counter specific fields in natural workflow order
            addIfFound('sales_patient_input');
            addIfFound('sales_mobile_input');
            addIfFound('sales_op_no_input');
            addIfFound('sales_ip_no_input');
            addIfFound('sales_consultant_input');
            addIfFound('sales_address_input');
            addIfFound('sales_billdate_input');
            addIfFound('sales_store_select');

            // Table rows (item, batch, days, qty, disc)
            var rows = document.querySelectorAll('.purchase-items-table-wrapper tbody tr');
            rows.forEach(function (tr, rIdx) {
                var item = document.getElementById('item-input-' + rIdx);
                if (item && item.offsetParent !== null && !item.disabled) seq.push(item);

                var batch = document.getElementById('batch-' + rIdx);
                if (batch && batch.offsetParent !== null && !batch.disabled) seq.push(batch);

                var days = document.getElementById('days-' + rIdx);
                if (days && days.offsetParent !== null && !days.disabled) seq.push(days);

                var qty = document.getElementById('qty-' + rIdx);
                if (qty && qty.offsetParent !== null && !qty.disabled) seq.push(qty);

                var disc = document.getElementById('disc-' + rIdx);
                if (disc && disc.offsetParent !== null && !disc.disabled) seq.push(disc);
            });

            addIfFound('sales_paymode_select');
            addIfFound('sales_paystatus_select');
            addIfFound('sales_dis_pct_input');
            addIfFound('sales_dis_amt_input');
            addIfFound('sales_remarks_input');
            addIfFound('sales_btn_save');
            addIfFound('sales_btn_print');

            return seq;
        },

        moveFocus: function (direction) {
            var seq = this.getNavSequence();
            if (!seq || seq.length === 0) return false;

            var active = document.activeElement;
            var curIdx = -1;
            for (var i = 0; i < seq.length; i++) {
                if (seq[i] === active || seq[i].contains(active)) {
                    curIdx = i;
                    break;
                }
            }

            var nextIdx = curIdx + direction;
            if (nextIdx >= 0 && nextIdx < seq.length) {
                var target = seq[nextIdx];
                target.focus();
                if (typeof target.select === 'function' && target.tagName !== 'SELECT' && target.type !== 'button') {
                    target.select();
                }
                return true;
            }
            return false;
        },

        attachListeners: function () {
            var self = this;
            window.addEventListener('keydown', function (e) {
                if (!self.isModalActive() && e.key !== 'F8') {
                    return;
                }

                // 1. New Bill: F8 only (Ctrl+N removed)
                if (e.key === 'F8') {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'F8');
                    }
                    return;
                }

                // 2. Switch Bill Tabs: Ctrl+1/2/3 or Alt+1/2/3
                var isTab1 = (e.ctrlKey || e.altKey) && (e.key === '1' || e.code === 'Digit1' || e.code === 'Numpad1');
                var isTab2 = (e.ctrlKey || e.altKey) && (e.key === '2' || e.code === 'Digit2' || e.code === 'Numpad2');
                var isTab3 = (e.ctrlKey || e.altKey) && (e.key === '3' || e.code === 'Digit3' || e.code === 'Numpad3');

                if (isTab1 || isTab2 || isTab3) {
                    e.preventDefault();
                    e.stopPropagation();
                    var tabKey = isTab1 ? 'Ctrl+1' : (isTab2 ? 'Ctrl+2' : 'Ctrl+3');
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', tabKey);
                    }
                    return;
                }

                // 3. Alt + Del: Delete current row
                if (e.altKey && (e.key === 'Delete' || e.key === 'Del' || e.keyCode === 46)) {
                    e.preventDefault();
                    e.stopPropagation();
                    var act = document.activeElement;
                    var rowIdx = -1;
                    if (act && act.id) {
                        var m = act.id.match(/-(\d+)$/);
                        if (m) rowIdx = parseInt(m[1], 10);
                    }
                    if (self.dotNetHelper) {
                        try {
                            var param = rowIdx >= 0 ? ('Alt+Del:' + rowIdx) : 'Alt+Del';
                            self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', param);
                        } catch (err) { }
                    }
                    return;
                }

                // 4. Ctrl + E: Finish billing details -> jump to payment mode
                if (e.ctrlKey && (e.key === 'e' || e.key === 'E')) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'Ctrl+E');
                    }
                    self.focusElement('sales_paymode_select');
                    return;
                }

                // 5. Ctrl + I: Toggle Shortcuts Info Modal
                if (e.ctrlKey && (e.key === 'i' || e.key === 'I')) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'Ctrl+I');
                    }
                    return;
                }

                // 6. Save Bill: F2
                if (e.key === 'F2') {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'F2');
                    }
                    return;
                }

                // 7. Print Bill: F3
                if (e.key === 'F3') {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'F3');
                    }
                    return;
                }

                // 8. Clear Bill: F4
                if (e.key === 'F4') {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'F4');
                    }
                    return;
                }

                // 9. Conversion Modal: F9
                if (e.key === 'F9') {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'F9');
                    }
                    return;
                }

                // 10. Escape: Close open dropdown, info popup, or cancel billing form
                if (e.key === 'Escape') {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'Esc');
                    }
                    return;
                }

                // 11. Shift+Enter: Move to previous field
                if (e.key === 'Enter' && e.shiftKey) {
                    e.preventDefault();
                    e.stopPropagation();
                    self.moveFocus(-1);
                    return;
                }

                // 12. Enter: Move to next field in continuous sequence
                if (e.key === 'Enter' && !e.shiftKey && !e.ctrlKey && !e.altKey) {
                    var act = document.activeElement;
                    if (act && act.tagName === 'TEXTAREA') return;

                    // If active element is save button or print button, let native click trigger
                    if (act && (act.id === 'sales_btn_save' || act.id === 'sales_btn_print')) {
                        return;
                    }

                    // Handled fields where Enter advances focus
                    if (act && (
                        act.id === 'sales_branch_picker' ||
                        act.id === 'sales_counter_picker' ||
                        act.id === 'sales_type_select' ||
                        act.id === 'sales_patient_input' ||
                        act.id === 'sales_mobile_input' ||
                        act.id === 'sales_op_no_input' ||
                        act.id === 'sales_ip_no_input' ||
                        act.id === 'sales_consultant_input' ||
                        act.id === 'sales_address_input' ||
                        act.id === 'sales_billdate_input' ||
                        act.id === 'sales_store_select' ||
                        act.id === 'sales_paymode_select' ||
                        act.id === 'sales_paystatus_select' ||
                        act.id === 'sales_dis_pct_input' ||
                        act.id === 'sales_dis_amt_input' ||
                        act.id === 'sales_remarks_input'
                    )) {
                        e.preventDefault();
                        e.stopPropagation();
                        self.moveFocus(1);
                        return;
                    }
                }
            }, true);
        }
    };
    window.focusElement = function (id) { window.pharmacyBilling.focusElement(id); };
})();