const drawDataLabelsPlugin = {
    id: 'drawDataLabels',
    afterDraw: function(chart) {
        const ctx = chart.ctx;
        chart.data.datasets.forEach(function(dataset, i) {
            const meta = chart.getDatasetMeta(i);
            if (meta.hidden) return;
            meta.data.forEach(function(element, index) {
                const pos = element.tooltipPosition();
                ctx.fillStyle = '#1e293b';
                const fontSize = 15;
                ctx.font = 'bold ' + fontSize + 'px sans-serif';
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                const dataString = dataset.data[index].toString();
                ctx.strokeStyle = 'rgba(255, 255, 255, 0.9)';
                ctx.lineWidth = 3;
                ctx.strokeText(dataString, pos.x, pos.y);
                ctx.fillText(dataString, pos.x, pos.y);
            });
        });
    }
};

window.dashboardCharts = {
    charts: {},

    destroyChart: function (canvasId) {
        if (this.charts[canvasId]) {
            this.charts[canvasId].destroy();
            delete this.charts[canvasId];
        }
    },

    // 1. DUAL COLUMN BAR CHART
    initPastDaysTrend: function (canvasId, labels, dataTokens, dataCompleted) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        const gradIndigo = ctx.createLinearGradient(0, 0, 0, canvas.height || 240);
        gradIndigo.addColorStop(0, '#818cf8');
        gradIndigo.addColorStop(1, '#4f46e5');

        const gradEmerald = ctx.createLinearGradient(0, 0, 0, canvas.height || 240);
        gradEmerald.addColorStop(0, '#34d399');
        gradEmerald.addColorStop(1, '#059669');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Outpatients',
                        data: dataTokens,
                        backgroundColor: gradIndigo,
                        hoverBackgroundColor: '#4338ca',
                        hoverBorderWidth: 2,
                        hoverBorderColor: '#ffffff',
                        borderRadius: 8,
                        borderSkipped: false,
                        barPercentage: 0.45,
                        categoryPercentage: 0.6
                    },
                    {
                        label: 'Inpatients',
                        data: dataCompleted,
                        backgroundColor: gradEmerald,
                        hoverBackgroundColor: '#047857',
                        hoverBorderWidth: 2,
                        hoverBorderColor: '#ffffff',
                        borderRadius: 8,
                        borderSkipped: false,
                        barPercentage: 0.45,
                        categoryPercentage: 0.6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 1200, easing: 'easeOutQuart' },
                hover: { mode: 'nearest', intersect: true },
                plugins: {
                    legend: {
                        position: 'bottom',
                        align: 'center',
                        labels: { font: { family: 'Inter', size: 12, weight: '600' }, color: '#64748b', usePointStyle: true, pointStyle: 'circle', padding: 16 }
                    },
                    tooltip: { padding: 12, backgroundColor: 'rgba(15, 23, 42, 0.95)', cornerRadius: 10 }
                },
                scales: {
                    x: { grid: { display: false }, ticks: { color: '#94a3b8', font: { family: 'Inter', size: 11 } } },
                    y: { grid: { color: 'rgba(241, 245, 249, 0.9)' }, ticks: { color: '#94a3b8', font: { family: 'Inter', size: 11 } } }
                }
            }
        });
    },

    // 2. DOUGHNUT RING CHART
    initCompletionDonut: function (canvasId, completedPct) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        const remainingPct = Math.max(0, 100 - completedPct);
        const gradDonut = ctx.createLinearGradient(0, 0, canvas.width || 140, canvas.height || 140);
        gradDonut.addColorStop(0, '#6366f1');
        gradDonut.addColorStop(1, '#4338ca');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Completed Rate', 'Pending Rate'],
                datasets: [{
                    data: [completedPct, remainingPct],
                    backgroundColor: [gradDonut, '#e2e8f0'],
                    borderWidth: 0,
                    hoverOffset: 14,
                    hoverBorderWidth: 3,
                    hoverBorderColor: '#ffffff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '78%',
                animation: { animateRotate: true, animateScale: true, duration: 1200 },
                plugins: { legend: { display: false }, tooltip: { padding: 10, backgroundColor: 'rgba(15, 23, 42, 0.9)', cornerRadius: 8 } }
            }
        });
    },

    // 3. SPLINE CURVED WAVE AREA CHART
    initHourlyDistribution: function (canvasId, labels, dataToday) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        const gradientWave = ctx.createLinearGradient(0, 0, 0, canvas.height || 180);
        gradientWave.addColorStop(0, 'rgba(245, 158, 11, 0.7)'); // Rich Amber
        gradientWave.addColorStop(0.5, 'rgba(251, 191, 36, 0.3)'); // Soft Yellow
        gradientWave.addColorStop(1, 'rgba(255, 255, 255, 0)'); // Transparent

        this.charts[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Registrations',
                    data: dataToday,
                    borderColor: '#f59e0b',
                    backgroundColor: gradientWave,
                    fill: true,
                    tension: 0.5,
                    cubicInterpolationMode: 'monotone',
                    borderWidth: 3,
                    pointRadius: 0,
                    pointHoverRadius: 8,
                    pointHoverBackgroundColor: '#f59e0b',
                    pointHoverBorderColor: '#ffffff',
                    pointHoverBorderWidth: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 1400, easing: 'easeInOutCubic' },
                hover: { mode: 'index', intersect: false },
                plugins: { legend: { display: false }, tooltip: { padding: 10, backgroundColor: 'rgba(15, 23, 42, 0.9)', cornerRadius: 8 } },
                scales: {
                    x: { grid: { display: false }, ticks: { color: '#94a3b8', font: { family: 'Inter', size: 10 } } },
                    y: { grid: { color: 'rgba(241, 245, 249, 0.6)' }, ticks: { color: '#94a3b8', font: { family: 'Inter', size: 10 } } }
                }
            }
        });
    },

    // 4. POLAR AREA RADIAL CHART (For Investigation Workload Breakdown)
    initInvestigationPolarChart: function (canvasId, labels, counts, colors) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'polarArea',
            data: {
                labels: labels,
                datasets: [{
                    data: counts,
                    backgroundColor: colors || [
                        'rgba(99, 102, 241, 0.85)',
                        'rgba(20, 184, 166, 0.85)',
                        'rgba(139, 92, 246, 0.85)',
                        'rgba(16, 185, 129, 0.85)'
                    ],
                    borderWidth: 2,
                    borderColor: '#ffffff',
                    hoverBorderWidth: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                animation: { animateRotate: true, animateScale: true, duration: 1200 },
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { font: { family: 'Inter', size: 10, weight: '600' }, color: '#64748b', boxWidth: 10, usePointStyle: true }
                    },
                    tooltip: { padding: 10, backgroundColor: 'rgba(15, 23, 42, 0.9)', cornerRadius: 8 }
                },
                scales: {
                    r: { grid: { color: 'rgba(241, 245, 249, 0.8)' }, ticks: { display: false } }
                }
            }
        });
    },

    // 5. HORIZONTAL BAR CHART (For Specialty Division Load)
    initDivisionHorizontalBarChart: function (canvasId, labels, counts) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        const gradHoriz = ctx.createLinearGradient(0, 0, canvas.width || 200, 0);
        gradHoriz.addColorStop(0, '#6366f1');
        gradHoriz.addColorStop(1, '#818cf8');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Tokens',
                    data: counts,
                    backgroundColor: gradHoriz,
                    borderRadius: 6,
                    borderSkipped: false,
                    barThickness: 6
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 1200, easing: 'easeOutQuart' },
                plugins: { legend: { display: false }, tooltip: { padding: 10, backgroundColor: 'rgba(15, 23, 42, 0.9)', cornerRadius: 8 } },
                scales: {
                    x: { grid: { color: 'rgba(241, 245, 249, 0.6)' }, ticks: { color: '#94a3b8', font: { family: 'Inter', size: 10 } } },
                    y: { grid: { display: false }, ticks: { autoSkip: false, minRotation: 0, maxRotation: 0, color: '#334155', font: { family: 'Inter', size: 7.5, weight: '600' } } }
                }
            }
        });
    },

    // 6. DOCTOR LEADERBOARD HORIZONTAL BAR CHART
    initDoctorLeaderboardBarChart: function (canvasId, labels, todayCounts, totalCounts) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Today',
                        data: todayCounts,
                        backgroundColor: 'rgba(99, 102, 241, 0.85)',
                        hoverBackgroundColor: '#4338ca',
                        borderRadius: 6,
                        borderSkipped: false,
                        barPercentage: 0.5,
                        categoryPercentage: 0.65
                    },
                    {
                        label: 'Total Tokens',
                        data: totalCounts,
                        backgroundColor: 'rgba(16, 185, 129, 0.75)',
                        hoverBackgroundColor: '#047857',
                        borderRadius: 6,
                        borderSkipped: false,
                        barPercentage: 0.5,
                        categoryPercentage: 0.65
                    }
                ]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 1400, easing: 'easeOutQuart' },
                plugins: {
                    legend: {
                        position: 'top',
                        align: 'end',
                        labels: { font: { family: 'Inter', size: 11, weight: '600' }, color: '#64748b', usePointStyle: true, pointStyle: 'circle', padding: 14 }
                    },
                    tooltip: { padding: 12, backgroundColor: 'rgba(15, 23, 42, 0.95)', cornerRadius: 10 }
                },
                scales: {
                    x: { grid: { color: 'rgba(241, 245, 249, 0.8)' }, ticks: { color: '#94a3b8', font: { family: 'Inter', size: 10 } } },
                    y: { grid: { display: false }, ticks: { autoSkip: false, color: '#1e293b', font: { family: 'Inter', size: 11, weight: '700' } } }
                }
            }
        });
    },

    // 7. TAT HORIZONTAL BAR CHART
    
    initTatCircularChart: function (canvasId, labels, values, colors) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: values,
                    backgroundColor: colors,
                    borderWidth: 0,
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '70%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            usePointStyle: true,
                            padding: 15,
                            font: { size: 10, family: "'Inter', sans-serif" }
                        }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(context) {
                                return ' ' + context.label + ': ' + context.raw.toFixed(1) + ' mins';
                            }
                        }
                    }
                }
            }
        });
    },
    initTatBarChart: function (canvasId, labels, avgValues, colors) {
        this.destroyChart(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;
        const ctx = canvas.getContext('2d');

        this.charts[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Avg Minutes',
                    data: avgValues,
                    backgroundColor: colors,
                    borderRadius: 8,
                    borderSkipped: false,
                    barThickness: 10
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                animation: { duration: 1200, easing: 'easeOutQuart' },
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        padding: 10,
                        backgroundColor: 'rgba(15, 23, 42, 0.9)',
                        cornerRadius: 8,
                        callbacks: {
                            label: function(ctx) { return ' ' + ctx.raw.toFixed(1) + ' min avg'; }
                        }
                    }
                },
                scales: {
                    x: { grid: { color: 'rgba(241, 245, 249, 0.6)' }, ticks: { color: '#94a3b8', font: { family: 'Inter', size: 10 } } },
                    y: { grid: { display: false }, ticks: { autoSkip: false, color: '#334155', font: { family: 'Inter', size: 10, weight: '600' } } }
                }
            }
        });
    }
};









