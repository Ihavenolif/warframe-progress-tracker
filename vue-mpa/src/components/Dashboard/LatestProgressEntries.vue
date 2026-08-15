<template>
    <section class="dashboard-section">
        <div class="section-head">
            <div>
                <p class="eyebrow">Import history</p>
                <h2>Recent snapshots</h2>
                <p>Changes recorded by your latest profile imports.</p>
            </div>
            <span v-if="!loading && !errorMessage" class="dashboard-entry-count">{{ entries.length }}</span>
        </div>

        <div v-if="loading" class="dashboard-state-card">Loading latest progress...</div>
        <div v-else-if="errorMessage" class="dashboard-state-card dashboard-error" role="alert">
            <strong>Recent snapshots could not be loaded</strong>
            <span>{{ errorMessage }}</span>
        </div>
        <div v-else-if="entries.length === 0" class="dashboard-state-card empty-state">
            <strong>No progress updates yet</strong>
            <span>Import your profile to create first snapshot.</span>
        </div>
        <div v-else class="dashboard-entry-list">
            <ProgressEntry v-for="entry in entries" :key="entry.id" :entry="entry" />
        </div>
    </section>
</template>

<script>
import { authFetch } from '@/util/util';
import ProgressEntry from './ProgressEntry.vue';

export default {
    name: 'LatestProgressEntries',
    components: {
        ProgressEntry
    },
    data() {
        return {
            loading: true,
            errorMessage: '',
            entries: []
        };
    },
    mounted() {
        this.fetchEntries();
    },
    methods: {
        async fetchEntries() {
            const res = await authFetch('/api/mastery/dashboard/entries', { method: 'GET' });
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

            this.entries = await res.json();
        }
    }
};
</script>
