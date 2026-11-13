(function () {
    "use strict";

    var chartInstances = new WeakMap();
    var expandedChart = null;
    var expandedData = null;
    var palette = [
        "#0d6efd", "#6610f2", "#d63384", "#fd7e14",
        "#198754", "#20c997", "#6c757d", "#ffc107"
    ];

    function buildConfig(data, expanded) {
        var labels = data.subLabels && data.subLabels.length
            ? data.labels.map(function (label, index) {
                return [label, data.subLabels[index] || ""];
            })
            : (data.labels || []);

        var series = data.series || [];
        var first = series[0] || { label: "", values: [] };
        var values = first.values || [];

        if (data.type === "pie") {
            return {
                type: "pie",
                data: {
                    labels: labels,
                    datasets: [{
                        label: first.label,
                        data: values,
                        backgroundColor: labels.map(function (_, index) {
                            return palette[index % palette.length];
                        }),
                        borderColor: "#fff",
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: { position: "bottom" }
                    }
                }
            };
        }

        var yAxis = {
            beginAtZero: true,
            title: { display: true, text: first.label || "Value" }
        };

        if (data.maxValue != null) {
            yAxis.max = data.maxValue;
        }

        var xAxis = expanded
            ? { ticks: { autoSkip: false, maxRotation: 45, minRotation: 0 } }
            : { ticks: { autoSkip: true, maxRotation: 0, minRotation: 0 } };

        return {
            type: "bar",
            data: {
                labels: labels,
                datasets: [{
                    label: first.label,
                    data: values,
                    backgroundColor: "#0d6efd",
                    borderRadius: 4,
                    maxBarThickness: expanded ? 72 : 48
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    x: xAxis,
                    y: yAxis
                }
            }
        };
    }

    function readChartData(canvas) {
        var raw = canvas.dataset.chartData;
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw);
        } catch (error) {
            console.error("Invalid chart data.", canvas, error);
            return null;
        }
    }

    function renderChart(canvas) {
        if (chartInstances.has(canvas) || typeof Chart === "undefined") {
            return;
        }

        var data = readChartData(canvas);
        if (!data) {
            return;
        }

        chartInstances.set(canvas, new Chart(canvas, buildConfig(data, false)));
    }

    function renderChartsIn(pane) {
        if (!pane) {
            return;
        }

        pane.querySelectorAll("canvas[data-chart-data]").forEach(renderChart);
    }

    function resizeChartsIn(pane) {
        if (!pane) {
            return;
        }

        pane.querySelectorAll("canvas[data-chart-data]").forEach(function (canvas) {
            var chart = chartInstances.get(canvas);
            if (chart) {
                chart.resize();
            }
        });
    }

    function renderExpandedChart() {
        var canvas = document.getElementById("chartExpandModalCanvas");
        if (!canvas || !expandedData || typeof Chart === "undefined") {
            return;
        }

        if (expandedChart) {
            expandedChart.destroy();
            expandedChart = null;
        }

        expandedChart = new Chart(canvas, buildConfig(expandedData, true));
    }

    function openExpandedChart(button) {
        var card = button.closest(".metrics-chart-card");
        var canvas = card && card.querySelector("canvas[data-chart-data]");
        var data = canvas && readChartData(canvas);
        var modal = document.getElementById("chartExpandModal");
        if (!data || !modal || !window.bootstrap) {
            return;
        }

        expandedData = data;
        var title = modal.querySelector(".modal-title");
        if (title) {
            title.textContent = data.title || "Chart";
        }

        window.bootstrap.Modal.getOrCreateInstance(modal).show();
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

        document.addEventListener("click", function (event) {
            var button = event.target.closest("[data-chart-expand]");
            if (button) {
                openExpandedChart(button);
            }
        });

        var modal = document.getElementById("chartExpandModal");
        if (modal) {
            modal.addEventListener("shown.bs.modal", renderExpandedChart);
            modal.addEventListener("hidden.bs.modal", function () {
                if (expandedChart) {
                    expandedChart.destroy();
                    expandedChart = null;
                }
                expandedData = null;
            });
        }
    });

    // Allows the filtering code to re-render charts after replacing pane content.
    window.DashboardCharts = {
        renderIn: renderChartsIn,
        resizeIn: resizeChartsIn
    };
})();
