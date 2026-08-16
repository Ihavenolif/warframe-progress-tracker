<template>
    <NavbarElement />

    <main class="content-page dashboard-page">
        <header class="page-heading">
            <div>
                <p class="eyebrow">Tenno activity</p>
                <h1>Dashboard</h1>
                <p>Follow mastery gains and review changes from recent profile imports.</p>
            </div>
            <RouterLink class="btn btn-secondary dashboard-import-link" to="/progress/import">Import progress</RouterLink>
        </header>

        <section class="dashboard-freshness" :class="`dashboard-freshness--${freshnessState}`">
            <div>
                <p class="eyebrow">Data freshness</p>
                <strong>{{ freshnessTitle }}</strong>
                <p>{{ freshnessMessage }}</p>
            </div>
            <span v-if="latestReceipt" class="dashboard-freshness__status">{{ freshnessState }}</span>
        </section>

        <DashboardSummary :summary="summary" :loading="summaryLoading" :error-message="summaryError" />
        <MasteryProgressChart />
        <LatestProgressEntries />
    </main>
</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import DashboardSummary from '@/components/Dashboard/DashboardSummary.vue';
import MasteryProgressChart from '@/components/Dashboard/MasteryProgressChart.vue';
import LatestProgressEntries from '@/components/Dashboard/LatestProgressEntries.vue';
import { authFetch } from '@/util/util';

export default {
    name: 'DashboardPage',
    components: {
        NavbarElement,
        DashboardSummary,
        MasteryProgressChart,
        LatestProgressEntries
    },
    data() {
        return {
            summary: null,
            summaryLoading: true,
            summaryError: '',
            now: Date.now(),
            freshnessTimer: null
        };
    },
    computed: {
        latestReceipt() {
            return this.summary?.latestImport || null;
        },
        freshnessLoading() {
            return this.summaryLoading;
        },
        freshnessError() {
            return Boolean(this.summaryError);
        },
        freshnessState() {
            if (!this.latestReceipt) return 'unknown';
            const staleAfter = 7 * 24 * 60 * 60 * 1000;
            return this.now - this.latestReceipt.importedAt > staleAfter ? 'stale' : 'fresh';
        },
        freshnessTitle() {
            if (this.freshnessLoading) return 'Checking latest import...';
            if (this.freshnessError) return 'Freshness unavailable';
            if (!this.latestReceipt) return 'No successful imports yet';
            return `Last imported ${this.formatRelativeTime(this.latestReceipt.importedAt)}`;
        },
        freshnessMessage() {
            if (this.freshnessLoading) return 'Loading profile receipt.';
            if (this.freshnessError) return 'Latest successful import could not be loaded.';
            if (!this.latestReceipt) return 'Import profile to establish dashboard freshness.';
            if (this.freshnessState === 'stale') return 'Profile data is over 7 days old. Import again for current progress.';
            return this.latestReceipt.changed
                ? 'Latest import changed stored progress.'
                : 'Latest import succeeded with no stored progress changes.';
        }
    },
    mounted() {
        this.fetchDashboardSummary();
        this.freshnessTimer = window.setInterval(() => {
            this.now = Date.now();
        }, 60000);
    },
    beforeUnmount() {
        window.clearInterval(this.freshnessTimer);
    },
    methods: {
        formatRelativeTime(timestamp) {
            const elapsed = Math.max(0, this.now - timestamp);
            const units = [
                ['year', 365 * 24 * 60 * 60 * 1000],
                ['month', 30 * 24 * 60 * 60 * 1000],
                ['week', 7 * 24 * 60 * 60 * 1000],
                ['day', 24 * 60 * 60 * 1000],
                ['hour', 60 * 60 * 1000],
                ['minute', 60 * 1000],
                ['second', 1000]
            ];
            const [unit, milliseconds] = units.find(([, size]) => elapsed >= size) || units[units.length - 1];
            const value = -Math.floor(elapsed / milliseconds);
            return new Intl.RelativeTimeFormat(undefined, { numeric: 'always' }).format(value, unit);
        },
        async fetchDashboardSummary() {
            try {
                const response = await authFetch('/api/mastery/dashboard/summary', { method: 'GET' });
                if (!response) return;
                if (response.status === 404) {
                    this.$router.push({ name: 'settings' });
                    return;
                }
                if (!response.ok) {
                    throw new Error((await response.text()) || `Request failed (${response.status})`);
                }
                this.summary = await response.json();
            } catch (error) {
                this.summaryError = error.message || 'Dashboard summary could not be loaded.';
            } finally {
                this.summaryLoading = false;
            }
        }
    }
};
</script>
