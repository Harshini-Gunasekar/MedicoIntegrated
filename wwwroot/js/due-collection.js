// due-collection.js
// Helper called by DueCollection.razor via IJSRuntime to directly set the DOM
// .value PROPERTY on collected-amount inputs after Auto Select runs.
//
// Why this is needed:
//   Blazor Server diffs the render tree and patches the DOM via setAttribute().
//   Browsers maintain a separate internal .value property for <input> elements
//   once they have been rendered; setAttribute('value', x) does NOT update what
//   the user sees. Setting element.value = x directly via JavaScript always does.

window.DueCollection = window.DueCollection || {};

window.DueCollection.setInputValue = function (id, value) {
    const el = document.getElementById(id);
    if (el) {
        el.value = value;
    }
};
