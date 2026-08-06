window.dashboardCharts = {
    instances: {},

    destroyChart: function (canvasId) {
        if (this.instances[canvasId]) {
            this.instances[canvasId].destroy();
            delete this.instances[canvasId];
        }
    },

    // 1. Doctor-Wise Total Diagnostic Orders (Horizontal Bar Chart)
    initHorizontalBarChart: function (canvasId, labels, data) {
        this.destroyChart(canvasId);
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        const defaultLabels = labels && labels.length ? labels : ['No Doctor Data'];
        const defaultData = data && data.length ? data : [0];
        const barColors = ['#10b981', '#2563eb', '#8b5cf6', '#f59e0b', '#ec4899', '#06b6d4', '#f43f5e', '#64748b'];

        var bgColors = [];
        for (var i = 0; i < defaultLabels.length; i++) {
            bgColors.push(barColors[i % barColors.length]);
        }

        this.instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: defaultLabels,
                datasets: [{
                    label: 'Diagnostic Orders',
                    data: defaultData,
                    backgroundColor: bgColors,
                    borderRadius: 6,
                    borderSkipped: false,
                    maxBarThickness: 16
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
                        padding: 8,
                        cornerRadius: 6
                    }
                },
                scales: {
                    x: {
                        grid: { color: 'rgba(226, 232, 240, 0.6)' },
                        ticks: { color: '#64748b', font: { size: 10, weight: '700' } }
                    },
                    y: {
                        grid: { display: false },
                        ticks: {
                            autoSkip: false,
                            color: '#475569',
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
                            labels: { color: '#475569', font: { size: 9, weight: '700' }, boxWidth: 10, padding: 6 }
                        },
                        tooltip: { backgroundColor: '#1e293b', padding: 8, cornerRadius: 6 }
                    },
                    scales: {
                        x: {
                            stacked: true,
                            grid: { display: false },
                            ticks: { autoSkip: false, color: '#475569', font: { size: 9, weight: '700' } }
                        },
                        y: {
                            stacked: true,
                            grid: { color: 'rgba(226, 232, 240, 0.6)' },
                            ticks: { color: '#64748b', font: { size: 10, weight: '700' }, stepSize: 1 }
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
                            ticks: { autoSkip: false, color: '#475569', font: { size: 10, weight: '700' } }
                        },
                        y: {
                            grid: { color: 'rgba(226, 232, 240, 0.6)' },
                            ticks: { color: '#64748b', font: { size: 10, weight: '700' }, stepSize: 1 }
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
                        grid: { color: 'rgba(241, 245, 249, 0.8)' },
                        ticks: { color: '#64748b', font: { size: 10, weight: '700' } }
                    },
                    y: {
                        grid: { color: 'rgba(226, 232, 240, 0.6)' },
                        ticks: { color: '#64748b', font: { size: 10, weight: '700' }, stepSize: 1 }
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

        const l = labPct !== undefined && !isNaN(labPct) ? Number(labPct) : 0;
        const s = scanPct !== undefined && !isNaN(scanPct) ? Number(scanPct) : 0;
        const e = ecgPct !== undefined && !isNaN(ecgPct) ? Number(ecgPct) : 0;

        const isZero = (l === 0 && s === 0 && e === 0);
        const dataVals = isZero ? [1] : [l, s, e];
        const bgColors = isZero ? ['#e2e8f0'] : ['#06b6d4', '#8b5cf6', '#f43f5e'];
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

        const l = labDur !== undefined && !isNaN(labDur) ? Number(labDur) : 0;
        const s = scanDur !== undefined && !isNaN(scanDur) ? Number(scanDur) : 0;
        const e = ecgDur !== undefined && !isNaN(ecgDur) ? Number(ecgDur) : 0;

        const isZero = (l === 0 && s === 0 && e === 0);
        const dataVals = isZero ? [1] : [l, s, e];
        const bgColors = isZero ? ['#e2e8f0'] : ['#06b6d4', '#8b5cf6', '#f43f5e'];
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

        const c = compVal !== undefined && !isNaN(compVal) ? Number(compVal) : 0;
        const a = activeVal !== undefined && !isNaN(activeVal) ? Number(activeVal) : 0;
        const w = waitVal !== undefined && !isNaN(waitVal) ? Number(waitVal) : 0;

        const isZero = (c === 0 && a === 0 && w === 0);
        const dataVals = isZero ? [1] : [c, a, w];
        const bgColors = isZero ? ['#e2e8f0'] : ['#10b981', '#f59e0b', '#f43f5e'];
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
    }
};
