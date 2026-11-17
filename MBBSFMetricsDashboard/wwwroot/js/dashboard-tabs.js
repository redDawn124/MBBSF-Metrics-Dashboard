(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll("[data-summary-open]").forEach(function (button) {
            button.addEventListener("click", function () {
                var panel = document.getElementById(button.getAttribute("aria-controls"));
                var dashboardView = button.closest(".tab-pane").querySelector("[id$='-dashboard-view']");
                if (!panel || !dashboardView) {
                    return;
                }

                var exitButton = panel.querySelector("[data-summary-close]");
                if (!exitButton) {
                    return;
                }

                dashboardView.hidden = true;
                panel.hidden = false;
                button.setAttribute("aria-expanded", "true");
                exitButton.focus();
            });
        });

        document.querySelectorAll("[data-summary-close]").forEach(function (button) {
            button.addEventListener("click", function () {
                var panel = button.closest(".dashboard-summary-panel");
                var tabPane = button.closest(".tab-pane");
                var openButton = tabPane && tabPane.querySelector("[data-summary-open]");
                var dashboardView = tabPane && tabPane.querySelector("[id$='-dashboard-view']");
                if (!panel || !openButton || !dashboardView) {
                    return;
                }

                panel.hidden = true;
                dashboardView.hidden = false;
                openButton.setAttribute("aria-expanded", "false");
                openButton.focus();
            });
        });

        document.querySelectorAll("[data-filter-open]").forEach(function (button) {
            button.addEventListener("click", function () {
                var panel = document.getElementById(button.getAttribute("aria-controls"));
                var dashboardView = button.closest(".tab-pane").querySelector("[id$='-dashboard-view']");
                if (!panel || !dashboardView) {
                    return;
                }

                dashboardView.hidden = true;
                panel.hidden = false;
                button.setAttribute("aria-expanded", "true");
                var firstControl = panel.querySelector("input:checked, select, input");
                if (firstControl) {
                    firstControl.focus();
                }
            });
        });

        document.querySelectorAll("[data-filter-close]").forEach(function (button) {
            button.addEventListener("click", function () {
                var panel = button.closest(".dashboard-filter-panel");
                var tabPane = button.closest(".tab-pane");
                var openButton = tabPane && tabPane.querySelector("[data-filter-open]");
                var dashboardView = tabPane && tabPane.querySelector("[id$='-dashboard-view']");
                if (!panel || !openButton || !dashboardView) {
                    return;
                }

                panel.hidden = true;
                dashboardView.hidden = false;
                openButton.setAttribute("aria-expanded", "false");
                openButton.focus();
            });
        });

        document.querySelectorAll("[data-filter-clear]").forEach(function (button) {
            button.addEventListener("click", function () {
                var panel = button.closest(".google-forms-filter-panel");
                if (!panel) {
                    return;
                }

                panel.querySelectorAll("select").forEach(function (select) {
                    select.selectedIndex = 0;
                });
                panel.querySelectorAll("input[type='number']").forEach(function (input) {
                    input.value = "";
                });
            });
        });

        document.querySelectorAll("[data-cognito-county]").forEach(function (countySelect) {
            countySelect.addEventListener("change", function () {
                var panel = countySelect.closest(".cognito-filter-panel");
                var schoolField = panel && panel.querySelector("[data-cognito-school-field]");
                var schoolSelect = schoolField && schoolField.querySelector("select");
                if (!schoolField || !schoolSelect) {
                    return;
                }

                schoolField.hidden = !countySelect.value;
                schoolSelect.selectedIndex = 0;
            });
        });

        document.querySelectorAll("[data-cognito-filter-clear]").forEach(function (button) {
            button.addEventListener("click", function () {
                var panel = button.closest(".cognito-filter-panel");
                if (!panel) {
                    return;
                }

                panel.querySelectorAll("select").forEach(function (select) {
                    select.selectedIndex = 0;
                });

                var schoolField = panel.querySelector("[data-cognito-school-field]");
                if (schoolField) {
                    schoolField.hidden = true;
                }
            });
        });
    });
})();
