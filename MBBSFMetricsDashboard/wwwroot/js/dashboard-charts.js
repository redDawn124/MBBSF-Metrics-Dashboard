(function () {
    "use strict";

    var chartInstances = new WeakMap();
    var palette = [
        "#0d6efd", "#6610f2", "#d63384", "#fd7e14",
        "#198754", "#20c997", "#6c757d", "#ffc107"
    ];

    function buildConfig(data) {
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

        return {
            type: "bar",
            data: {
                labels: labels,
                datasets: [{
                    label: first.label,
                    data: values,
                    backgroundColor: "#0d6efd",
                    borderRadius: 4,
                    maxBarThickness: 48
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    x: {
                        ticks: {
                            autoSkip: true,
                            maxRotation: 0,
                            minRotation: 0
                        }
                    },
                    y: yAxis
                }
            }
        };
    }

    function renderChart(canvas) {
        if (chartInstances.has(canvas) || typeof Chart === "undefined") {
            return;
        }

        var raw = canvas.dataset.chartData;
        if (!raw) {
            return;
        }

        var data;
        try {
            data = JSON.parse(raw);
        } catch (error) {
            console.error("Invalid chart data.", canvas, error);
            return;
        }

        chartInstances.set(canvas, new Chart(canvas, buildConfig(data)));
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

    // Allows the filtering code to re-render charts after replacing pane content.
    window.DashboardCharts = {
        renderIn: renderChartsIn,
        resizeIn: resizeChartsIn
    };
})();
