<template>
    <section class="dashboard-section">
        <div class="section-head">
            <div>
                <h2>Mastery progress</h2>
                <p>{{ rangeLabel }}, grouped by day</p>
            </div>
            <div class="range-switch">
                <button class="btn" :class="{ 'is-active': selectedRange === 7 }" @click="setRange(7)">Week</button>
                <button class="btn" :class="{ 'is-active': selectedRange === 30 }" @click="setRange(30)">Month</button>
            </div>
        </div>

        <p v-if="loading">Loading mastery progress...</p>
        <p v-else-if="errorMessage" class="dashboard-error">{{ errorMessage }}</p>
        <div v-else class="chart-wrap">
            <Bar :data="chartData" :options="chartOptions" />
        </div>
    </section>
</template>

<script>
import { authFetch } from '@/util/util';
import { Bar } from 'vue-chartjs';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    BarElement,
    Tooltip
} from 'chart.js';

ChartJS.register(CategoryScale, LinearScale, BarElement, Tooltip);

export default {
    name: 'MasteryProgressChart',
    components: {
        Bar
    },
    data() {
        return {
            selectedRange: 7,
            loading: true,
            errorMessage: '',
            dailyProgress: []
        }
    },
    computed: {
        rangeLabel() {
            return this.selectedRange === 7 ? 'Last week' : 'Last month';
        },
        chartData() {
            return {
                labels: this.dailyProgress.map(day => this.formatDate(day.date)),
                datasets: [{
                    label: 'Mastery XP gained',
                    data: this.dailyProgress.map(day => day.masteryXpGained),
                    backgroundColor: '#2e8b57',
                    borderColor: '#246f46',
                    borderWidth: 1,
                    borderRadius: 0
                }]
            };
        },
        chartOptions() {
            return {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            label: context => `${this.formatNumber(context.parsed.y)} mastery XP`
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            autoSkip: true,
                            maxTicksLimit: this.selectedRange === 7 ? 7 : 10
                        }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            precision: 0,
                            callback: value => this.formatNumber(value)
                        }
                    }
                }
            };
        }
    },
    mounted() {
        this.fetchProgress();
    },
    methods: {
        async setRange(days) {
            if (days === this.selectedRange) return;
            this.selectedRange = days;
            await this.fetchProgress();
        },
        async fetchProgress() {
            this.loading = true;
            this.errorMessage = '';

            const res = await authFetch(`/api/mastery/dashboard/daily?days=${this.selectedRange}`, { method: 'GET' });
            this.loading = false;

            if (!res) return;
            if (res.status === 404) {
                this.$router.push({ name: 'settings' });
                return;
            }
            if (!res.ok) {
                this.errorMessage = await res.text();
                return;
            }

            this.dailyProgress = await res.json();
        },
        formatDate(value) {
            return new Date(value).toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
        },
        formatNumber(value) {
            return new Intl.NumberFormat().format(value || 0);
        }
    }
}
</script>
