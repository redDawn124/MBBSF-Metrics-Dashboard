(function () {
    "use strict";

    document.addEventListener("click", function (event) {
        var button = event.target.closest("[data-note-toggle]");
        if (!button) {
            return;
        }

        var detailId = button.getAttribute("aria-controls");
        var detail = detailId ? document.getElementById(detailId) : null;
        if (!detail) {
            return;
        }

        var expanded = button.getAttribute("aria-expanded") === "true";

        button.setAttribute("aria-expanded", expanded ? "false" : "true");
        button.textContent = expanded ? "Show note" : "Hide note";
        detail.hidden = expanded;
    });
})();
