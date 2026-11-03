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

        focusElement: function (id) {
            setTimeout(function () {
                var el = document.getElementById(id);
                if (el) {
                    el.focus();
                    if (typeof el.select === 'function' && el.tagName !== 'SELECT' && el.type !== 'button') {
                        el.select();
                    }
                }
            }, 40);
        },

        isModalActive: function () {
            return document.querySelector('.purchase-bill-modal') !== null;
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

            // OP / IP / Counter specific fields
            addIfFound('sales_ip_no_input');
            addIfFound('sales_patient_input');
            addIfFound('sales_mobile_input');
            addIfFound('sales_op_no_input');
            addIfFound('sales_address_input');
            addIfFound('sales_consultant_input');
            addIfFound('sales_billdate_input');
            addIfFound('sales_store_select');

            // Table rows (item, batch, qty, disc)
            var rows = document.querySelectorAll('.purchase-items-table-wrapper tbody tr');
            rows.forEach(function (tr, rIdx) {
                var item = document.getElementById('item-input-' + rIdx);
                if (item && item.offsetParent !== null && !item.disabled) seq.push(item);

                var batch = document.getElementById('batch-' + rIdx);
                if (batch && batch.offsetParent !== null && !batch.disabled) seq.push(batch);

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
                if (!self.isModalActive() && e.key !== 'F8' && !(e.ctrlKey && (e.key === 'n' || e.key === 'N'))) {
                    return;
                }

                // 1. New Bill: Ctrl+N or F8
                if ((e.ctrlKey && (e.key === 'n' || e.key === 'N')) || e.key === 'F8') {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'Ctrl+N');
                    }
                    return;
                }

                // 2. Switch Bill Tabs: Ctrl+1, Ctrl+2, Ctrl+3
                if (e.ctrlKey && (e.key === '1' || e.key === '2' || e.key === '3')) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'Ctrl+' + e.key);
                    }
                    return;
                }

                // 3. Alt + Del: Delete current row
                if (e.altKey && (e.key === 'Delete' || e.key === 'Del' || e.keyCode === 46)) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'Alt+Del');
                    }
                    return;
                }

                // 4. Ctrl + E: Finish billing details -> jump to payment mode
                if (e.ctrlKey && (e.key === 'e' || e.key === 'E')) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (self.dotNetHelper) {
                        self.dotNetHelper.invokeMethodAsync('HandlePharmacyGlobalShortcut', 'Ctrl+E');
                    } else {
                        self.focusElement('sales_paymode_select');
                    }
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

                // 12. Enter: Move to next field (when not inside special autocomplete handling or textarea)
                if (e.key === 'Enter' && !e.shiftKey && !e.ctrlKey && !e.altKey) {
                    var act = document.activeElement;
                    if (act && act.tagName === 'TEXTAREA') return;

                    // If active element is a button, like save or clear, let native/blazor onclick execute
                    if (act && act.tagName === 'BUTTON' && act.id !== 'sales_branch_picker' && act.id !== 'sales_counter_picker') {
                        return;
                    }

                    // If inside an item search input or batch input or consultant or patient input,
                    // let the specific Blazor @onkeydown handle selecting the active dropdown item first.
                    // But if it's a general input or select:
                    if (act && (
                        act.id === 'sales_type_select' ||
                        act.id === 'sales_mobile_input' ||
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
