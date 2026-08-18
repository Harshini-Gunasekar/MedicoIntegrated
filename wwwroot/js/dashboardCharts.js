function getChartThemeColors() {
    const isDark = document.documentElement.getAttribute('data-theme') === 'dark' || document.body.classList.contains('dark-mode');
    return {
        isDark: isDark,
        tickColor: isDark ? '#94a3b8' : '#64748b',
        labelColor: isDark ? '#cbd5e1' : '#334155',
        gridColor: isDark ? 'rgba(255, 255, 255, 0.08)' : 'rgba(226, 232, 240, 0.6)',
        emptyColor: isDark ? '#334155' : '#e2e8f0'
    };
}

window.dashboardCharts = {
    instances: {},

    destroyChart: function (canvasId) {
        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
            delete this.instances[canvasId];
        }
    },

    // 1. Doctor-Wise Total Diagnostic Orders (Horizontal Bar Chart)
    initHorizontalBarChart: function (canvasId, labels, data, datasetLabel) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const theme = getChartThemeColors();
        const defaultLabels = labels && labels.length ? labels : ['No Store Data'];
        const defaultData = data && data.length ? data : [0];
        const barColors = ['#10b981', '#8b5cf6', '#06b6d4', '#f59e0b', '#ec4899', '#3b82f6', '#f43f5e', '#64748b'];

        var bgColors = [];
        for (var i = 0; i < defaultLabels.length; i++) {
            bgColors.push(barColors[i % barColors.length]);
        }

        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: defaultLabels,
                datasets: [{
                    label: datasetLabel || 'Stock Units',
                    data: defaultData,
                    backgroundColor: bgColors,
                    borderRadius: 8,
                    borderSkipped: false,
                    maxBarThickness: 18
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 800, easing: 'easeOutQuart' },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        padding: 10,
                        cornerRadius: 8,
                        titleFont: { size: 11, weight: '700' },
                        bodyFont: { size: 11 }
                    }
                },
                scales: {
                    x: {
                        grid: { color: theme.gridColor },
                        ticks: { color: theme.tickColor, font: { size: 10, weight: '700' } }
                    },
                    y: {
                        grid: { display: false },
                        ticks: {
                            autoSkip: false,
                            color: theme.labelColor,
                            font: { size: 10, weight: '700' }
                        }
                    }
                }
            }
        });
    },

    // 2. Diagnostic Volume By Department (Doctor-Wise: Lab vs Scan vs ECG)
    initVerticalBarChart: function (canvasId, labels, labData, scanData, ecgData) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const theme = getChartThemeColors();
        if (Array.isArray(labels) && (Array.isArray(labData) || Array.isArray(scanData))) {
            const defaultLabels = labels.length ? labels : ['Doctor'];
            const lData = labData || [];
            const sData = scanData || [];
            const eData = ecgData || [];

            this.instances[canvasId] = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: defaultLabels,
                    datasets: [
                        {
                            label: 'Lab',
                            data: lData,
                            backgroundColor: '#06b6d4',
                            borderRadius: 4,
                            borderSkipped: false
                        },
                        {
                            label: 'Scan',
                            data: sData,
                            backgroundColor: '#8b5cf6',
                            borderRadius: 4,
                            borderSkipped: false
                        },
                        {
                            label: 'ECG',
                            data: eData,
                            backgroundColor: '#f43f5e',
                            borderRadius: 4,
                            borderSkipped: false
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: { duration: 800, easing: 'easeOutQuart' },
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top',
                            labels: { color: theme.labelColor, font: { size: 9, weight: '700' }, boxWidth: 10, padding: 6 }
                        },
                        tooltip: { backgroundColor: '#1e293b', padding: 8, cornerRadius: 6 }
                    },
                    scales: {
                        x: {
                            stacked: true,
                            grid: { display: false },
                            ticks: { autoSkip: false, color: theme.labelColor, font: { size: 9, weight: '700' } }
                        },
                        y: {
                            stacked: true,
                            grid: { color: theme.gridColor },
                            ticks: { color: theme.tickColor, font: { size: 10, weight: '700' }, stepSize: 1 }
                        }
                    }
                }
            });
        } else {
            const defaultLabels = labels && labels.length ? labels : ['Pathology Lab', 'Radiology Scan', 'Cardiology ECG'];
            const defaultData = labData && labData.length ? labData : [0, 0, 0];
            const deptColors = ['#06b6d4', '#8b5cf6', '#f43f5e'];

            this.instances[canvasId] = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: defaultLabels,
                    datasets: [{
                        label: 'Test Volume',
                        data: defaultData,
                        backgroundColor: deptColors,
                        borderRadius: 8,
                        borderSkipped: false,
                        barThickness: 32
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    animation: { duration: 800, easing: 'easeOutQuart' },
                    plugins: {
                        legend: { display: false },
                        tooltip: { backgroundColor: '#1e293b', padding: 8, cornerRadius: 6 }
                    },
                    scales: {
                        x: {
                            grid: { display: false },
                            ticks: { autoSkip: false, color: theme.labelColor, font: { size: 10, weight: '700' } }
                        },
                        y: {
                            grid: { color: theme.gridColor },
                            ticks: { color: theme.tickColor, font: { size: 10, weight: '700' }, stepSize: 1 }
                        }
                    }
                }
            });
        }
    },

    // 3. Hourly Overall Patient Inflow (Curved Red Line Chart)
    initHourlyPeakLineChart: function (canvasId, labels, data) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const theme = getChartThemeColors();
        const chartCtx = ctx.getContext('2d');
        const fillGradient = chartCtx.createLinearGradient(0, 0, 0, 200);
        fillGradient.addColorStop(0, 'rgba(244, 63, 94, 0.25)');
        fillGradient.addColorStop(1, 'rgba(244, 63, 94, 0.01)');

        const defaultLabels = labels && labels.length ? labels : ['08:00', '09:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00'];
        const defaultData = data && data.length ? data : new Array(defaultLabels.length).fill(0);

        this.instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: defaultLabels,
                datasets: [{
                    label: 'Patients Per Hour',
                    data: defaultData,
                    borderColor: '#f43f5e',
                    borderWidth: 2.5,
                    backgroundColor: fillGradient,
                    fill: true,
                    tension: 0.45,
                    pointRadius: 4,
                    pointHoverRadius: 7,
                    pointBackgroundColor: '#f43f5e',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 1000, easing: 'easeOutQuart' },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        padding: 8,
                        cornerRadius: 6
                    }
                },
                scales: {
                    x: {
                        grid: { color: theme.gridColor },
                        ticks: { color: theme.tickColor, font: { size: 10, weight: '700' } }
                    },
                    y: {
                        grid: { color: theme.gridColor },
                        ticks: { color: theme.tickColor, font: { size: 10, weight: '700' }, stepSize: 1 }
                    }
                }
            }
        });
    },

    // 4. Departmental Completion Rate (3-Segment Donut Chart)
    initSemiGaugeChart: function (canvasId, labPct, scanPct, ecgPct) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const theme = getChartThemeColors();
        const l = labPct !== undefined && !isNaN(labPct) ? Number(labPct) : 0;
        const s = scanPct !== undefined && !isNaN(scanPct) ? Number(scanPct) : 0;
        const e = ecgPct !== undefined && !isNaN(ecgPct) ? Number(ecgPct) : 0;

        const isZero = (l === 0 && s === 0 && e === 0);
        const dataVals = isZero ? [1] : [l, s, e];
        const bgColors = isZero ? [theme.emptyColor] : ['#06b6d4', '#8b5cf6', '#f43f5e'];
        const chartLabels = isZero ? ['No Data'] : ['Lab Completion', 'Scan Completion', 'ECG Completion'];

        this.instances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: chartLabels,
                datasets: [{
                    data: dataVals,
                    backgroundColor: bgColors,
                    borderWidth: 0,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '76%',
                animation: { duration: 800, easing: 'easeOutQuart' },
                plugins: {
                    legend: { display: false },
                    tooltip: { backgroundColor: '#0f172a', cornerRadius: 6, padding: 8 }
                }
            }
        });
    },

    // 5. Average Duration By Department (Donut Chart - Cyan/Purple/Pink)
    initDurationDonutChart: function (canvasId, labDur, scanDur, ecgDur) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const theme = getChartThemeColors();
        const l = labDur !== undefined && !isNaN(labDur) ? Number(labDur) : 0;
        const s = scanDur !== undefined && !isNaN(scanDur) ? Number(scanDur) : 0;
        const e = ecgDur !== undefined && !isNaN(ecgDur) ? Number(ecgDur) : 0;

        const isZero = (l === 0 && s === 0 && e === 0);
        const dataVals = isZero ? [1] : [l, s, e];
        const bgColors = isZero ? [theme.emptyColor] : ['#06b6d4', '#8b5cf6', '#f43f5e'];
        const chartLabels = isZero ? ['No Data'] : ['Lab Processing', 'Scan Imaging', 'ECG Recording'];

        this.instances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: chartLabels,
                datasets: [{
                    data: dataVals,
                    backgroundColor: bgColors,
                    borderWidth: 0,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '76%',
                animation: { duration: 800, easing: 'easeOutQuart' },
                plugins: {
                    legend: { display: false },
                    tooltip: { backgroundColor: '#0f172a', cornerRadius: 6, padding: 8 }
                }
            }
        });
    },

    // 6. Overall Diagnostics (Donut Chart - Completed vs In-Progress vs Pending Status)
    initFullDonutChart: function (canvasId, compVal, activeVal, waitVal, customLabels) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const theme = getChartThemeColors();
        const c = compVal !== undefined && !isNaN(compVal) ? Number(compVal) : 0;
        const a = activeVal !== undefined && !isNaN(activeVal) ? Number(activeVal) : 0;
        const w = waitVal !== undefined && !isNaN(waitVal) ? Number(waitVal) : 0;

        const isZero = (c === 0 && a === 0 && w === 0);
        const dataVals = isZero ? [1] : [c, a, w];
        const bgColors = isZero ? [theme.emptyColor] : ['#10b981', '#f59e0b', '#f43f5e'];
        const chartLabels = isZero ? ['No Data'] : (customLabels || ['Completed (Report Received)', 'In Consultation', 'Pending / Waiting']);

        this.instances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: chartLabels,
                datasets: [{
                    data: dataVals,
                    backgroundColor: bgColors,
                    borderWidth: 0,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '76%',
                animation: { duration: 800, easing: 'easeOutQuart' },
                plugins: {
                    legend: { display: false },
                    tooltip: { backgroundColor: '#0f172a', cornerRadius: 6, padding: 8 }
                }
            }
        });
    },

    // Generic Area Line Chart with gradient fill
    initAreaLineChart: function (canvasId, labels, data, datasetLabel, lineColor, fillColor) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const chartCtx = ctx.getContext('2d');
        let bgGradient = fillColor;
        if (!fillColor || fillColor.startsWith('#') || fillColor.startsWith('rgb')) {
            bgGradient = chartCtx.createLinearGradient(0, 0, 0, 240);
            bgGradient.addColorStop(0, lineColor ? lineColor + '33' : 'rgba(37, 99, 235, 0.35)');
            bgGradient.addColorStop(1, lineColor ? lineColor + '03' : 'rgba(37, 99, 235, 0.02)');
        }

        const defaultLabels = labels && labels.length ? labels : ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
        const defaultData = data && data.length ? data : [12, 19, 15, 25, 22, 30, 28];

        this.instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: defaultLabels,
                datasets: [{
                    label: datasetLabel || 'Value',
                    data: defaultData,
                    borderColor: lineColor || '#2563eb',
                    borderWidth: 3,
                    backgroundColor: bgGradient,
                    fill: true,
                    tension: 0.4,
                    pointBackgroundColor: lineColor || '#2563eb',
                    pointBorderColor: '#ffffff',
                    pointBorderWidth: 2,
                    pointRadius: 4,
                    pointHoverRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 900, easing: 'easeOutQuart' },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        titleFont: { size: 12, weight: '700' },
                        bodyFont: { size: 12 },
                        padding: 10,
                        cornerRadius: 8
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#64748b', font: { size: 11, weight: '600' } }
                    },
                    y: {
                        grid: { color: 'rgba(226, 232, 240, 0.7)' },
                        ticks: { color: '#64748b', font: { size: 11, weight: '600' } }
                    }
                }
            }
        });
    },

    // Generic Doughnut / Pie Chart with sleek styling
    initCustomDoughnutChart: function (canvasId, labels, data, bgColors) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const defaultLabels = labels && labels.length ? labels : ['Category A', 'Category B', 'Category C'];
        const defaultData = data && data.length ? data : [40, 35, 25];
        const defaultColors = bgColors && bgColors.length ? bgColors : ['#2563eb', '#10b981', '#f59e0b', '#8b5cf6', '#ec4899'];

        this.instances[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: defaultLabels,
                datasets: [{
                    data: defaultData,
                    backgroundColor: defaultColors,
                    borderWidth: 2,
                    borderColor: '#ffffff',
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                animation: { duration: 800, easing: 'easeOutQuart' },
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom',
                        labels: { color: '#475569', font: { size: 11, weight: '600' }, padding: 12, usePointStyle: true, pointStyle: 'circle' }
                    },
                    tooltip: { backgroundColor: '#0f172a', cornerRadius: 8, padding: 10 }
                }
            }
        });
    },

    // Generic Grouped Bar Chart
    initGroupedBarChart: function (canvasId, labels, dataset1Label, dataset1Data, dataset1Color, dataset2Label, dataset2Data, dataset2Color) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const defaultLabels = labels && labels.length ? labels : ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'];
        const d1 = dataset1Data && dataset1Data.length ? dataset1Data : [45, 52, 60, 48, 65, 70];
        const d2 = dataset2Data && dataset2Data.length ? dataset2Data : [38, 44, 50, 42, 58, 62];

        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: defaultLabels,
                datasets: [
                    {
                        label: dataset1Label || 'Dataset 1',
                        data: d1,
                        backgroundColor: dataset1Color || '#2563eb',
                        borderRadius: 6,
                        borderSkipped: false
                    },
                    {
                        label: dataset2Label || 'Dataset 2',
                        data: d2,
                        backgroundColor: dataset2Color || '#10b981',
                        borderRadius: 6,
                        borderSkipped: false
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 900, easing: 'easeOutQuart' },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: { color: '#475569', font: { size: 11, weight: '600' }, usePointStyle: true, pointStyle: 'circle' }
                    },
                    tooltip: { backgroundColor: '#0f172a', cornerRadius: 8, padding: 10 }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#64748b', font: { size: 11, weight: '600' } }
                    },
                    y: {
                        grid: { color: 'rgba(226, 232, 240, 0.7)' },
                        ticks: { color: '#64748b', font: { size: 11, weight: '600' } }
                    }
                }
            }
        });
    },

    // Dual-Axis Grouped Bar Chart (Primary Y for Valuation, Secondary Y for Qty)
    initDualAxisGroupedBarChart: function (canvasId, labels, dataset1Label, dataset1Data, dataset1Color, dataset2Label, dataset2Data, dataset2Color) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const defaultLabels = labels && labels.length ? labels : ['Item A', 'Item B', 'Item C'];
        const d1 = dataset1Data && dataset1Data.length ? dataset1Data : [1000, 3000, 4500];
        const d2 = dataset2Data && dataset2Data.length ? dataset2Data : [75, 80, 92];

        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: defaultLabels,
                datasets: [
                    {
                        label: dataset1Label || 'Valuation (₹)',
                        data: d1,
                        backgroundColor: '#ef4444',
                        hoverBackgroundColor: '#dc2626',
                        yAxisID: 'yValuation',
                        borderRadius: 8,
                        borderSkipped: false,
                        barPercentage: 0.6,
                        categoryPercentage: 0.7
                    },
                    {
                        label: dataset2Label || 'Qty (Units)',
                        data: d2,
                        backgroundColor: '#06b6d4',
                        hoverBackgroundColor: '#0891b2',
                        yAxisID: 'yQty',
                        borderRadius: 8,
                        borderSkipped: false,
                        barPercentage: 0.6,
                        categoryPercentage: 0.7
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 900, easing: 'easeOutQuart' },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: { color: '#334155', font: { size: 11, weight: '700' }, usePointStyle: true, pointStyle: 'circle', padding: 12 }
                    },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        titleFont: { size: 12, weight: '700' },
                        bodyFont: { size: 11, weight: '600' },
                        cornerRadius: 10,
                        padding: 12,
                        shadowOffsetX: 0,
                        shadowOffsetY: 4,
                        shadowBlur: 10,
                        shadowColor: 'rgba(0,0,0,0.15)',
                        callbacks: {
                            label: function (context) {
                                let label = context.dataset.label || '';
                                if (label) label += ': ';
                                if (context.dataset.yAxisID === 'yValuation') {
                                    label += '₹' + Number(context.raw).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                                } else {
                                    label += Number(context.raw).toLocaleString() + ' Units';
                                }
                                return label;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#475569', font: { size: 10, weight: '700' }, maxRotation: 20, minRotation: 0 }
                    },
                    yValuation: {
                        type: 'linear',
                        position: 'left',
                        title: { display: true, text: 'Valuation (₹)', color: '#dc2626', font: { size: 11, weight: '700' } },
                        grid: { color: 'rgba(226, 232, 240, 0.6)' },
                        ticks: { 
                            color: '#dc2626', 
                            font: { size: 10, weight: '600' },
                            callback: function(val) {
                                return '₹' + Number(val).toLocaleString('en-IN');
                            }
                        }
                    },
                    yQty: {
                        type: 'linear',
                        position: 'right',
                        title: { display: true, text: 'Qty (Units)', color: '#0891b2', font: { size: 11, weight: '700' } },
                        grid: { display: false },
                        ticks: { 
                            color: '#0891b2', 
                            font: { size: 10, weight: '600' },
                            callback: function(val) {
                                return Number(val).toLocaleString() + ' u';
                            }
                        }
                    }
                }
            }
        });
    },

    // Daily IP Revenue vs OP Revenue Comparison Chart
    initDailyRevenueComparisonChart: function (canvasId, labels, ipData, opData) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const chartCtx = ctx.getContext('2d');
        
        const ipGradient = chartCtx.createLinearGradient(0, 0, 0, 240);
        ipGradient.addColorStop(0, 'rgba(37, 99, 235, 0.35)');
        ipGradient.addColorStop(1, 'rgba(37, 99, 235, 0.02)');

        const opGradient = chartCtx.createLinearGradient(0, 0, 0, 240);
        opGradient.addColorStop(0, 'rgba(16, 185, 129, 0.35)');
        opGradient.addColorStop(1, 'rgba(16, 185, 129, 0.02)');

        const defaultLabels = labels && labels.length ? labels : ['03 Aug', '05 Aug', '08 Aug', '10 Aug', '12 Aug', '14 Aug'];
        const dIp = ipData && ipData.length ? ipData : [4500, 7200, 6800, 9100, 11200, 9500];
        const dOp = opData && opData.length ? opData : [3200, 4800, 5200, 6100, 7400, 6900];

        this.instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: defaultLabels,
                datasets: [
                    {
                        label: 'IP Revenue (₹)',
                        data: dIp,
                        borderColor: '#2563eb',
                        borderWidth: 3,
                        backgroundColor: ipGradient,
                        fill: true,
                        tension: 0.35,
                        pointBackgroundColor: '#2563eb',
                        pointBorderColor: '#ffffff',
                        pointBorderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 7
                    },
                    {
                        label: 'OP Revenue (₹)',
                        data: dOp,
                        borderColor: '#10b981',
                        borderWidth: 3,
                        backgroundColor: opGradient,
                        fill: true,
                        tension: 0.35,
                        pointBackgroundColor: '#10b981',
                        pointBorderColor: '#ffffff',
                        pointBorderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 7
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 900, easing: 'easeOutQuart' },
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                        labels: { color: '#334155', font: { size: 11, weight: '700' }, usePointStyle: true, pointStyle: 'circle', padding: 12 }
                    },
                    tooltip: {
                        backgroundColor: '#0f172a',
                        titleFont: { size: 12, weight: '700' },
                        bodyFont: { size: 11, weight: '600' },
                        cornerRadius: 10,
                        padding: 12,
                        callbacks: {
                            label: function (context) {
                                let label = context.dataset.label || '';
                                if (label) label += ': ';
                                label += '₹' + Number(context.raw).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
                                return label;
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { color: '#475569', font: { size: 10, weight: '700' } }
                    },
                    y: {
                        grid: { color: 'rgba(226, 232, 240, 0.6)' },
                        ticks: {
                            color: '#64748b',
                            font: { size: 10, weight: '600' },
                            callback: function (val) {
                                return '₹' + Number(val).toLocaleString('en-IN');
                            }
                        }
                    }
                }
            }
        });
    }
};


