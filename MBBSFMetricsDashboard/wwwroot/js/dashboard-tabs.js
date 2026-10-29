(function () {
    "use strict";

    var chartInstances = new WeakMap();

    function buildConfig(canvas) {
        var raw = canvas.dataset.chartConfig;
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw);
        } catch (error) {
            console.error("Invalid chart configuration.", canvas, error);
            return null;
        }
    }

    function renderChart(canvas) {
        if (chartInstances.has(canvas) || typeof Chart === "undefined") {
            return;
        }

        var config = buildConfig(canvas);
        if (!config) {
            return;
        }

        chartInstances.set(canvas, new Chart(canvas, config));
    }

    function renderChartsIn(pane) {
        if (!pane) {
            return;
        }

        pane.querySelectorAll("canvas[data-chart-config]").forEach(renderChart);
    }

    function resizeChartsIn(pane) {
        if (!pane) {
            return;
        }

        pane.querySelectorAll("canvas[data-chart-config]").forEach(function (canvas) {
            var chart = chartInstances.get(canvas);
            if (chart) {
                chart.resize();
            }
        });
    }

    function paneFor(tab) {
        var selector = tab.getAttribute("data-bs-target");
        return selector ? document.querySelector(selector) : null;
    }

    document.addEventListener("DOMContentLoaded", function () {
        // Charts rendered while their pane is hidden would size to zero,
        // so only initialize the pane that is visible on load.
        renderChartsIn(document.querySelector("#metricsTabContent .tab-pane.active"));

        document.querySelectorAll("#metricsTabs [data-bs-toggle='tab']").forEach(function (tab) {
            tab.addEventListener("shown.bs.tab", function () {
                var pane = paneFor(tab);
                renderChartsIn(pane);
                resizeChartsIn(pane);
            });
        });
    });
})();
