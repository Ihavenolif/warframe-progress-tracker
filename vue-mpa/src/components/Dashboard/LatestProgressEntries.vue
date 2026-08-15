<template>
    <section class="dashboard-section">
        <h2>Latest progress entries</h2>

        <p v-if="loading">Loading latest progress...</p>
        <p v-else-if="errorMessage" class="dashboard-error">{{ errorMessage }}</p>
        <div v-else-if="entries.length === 0" class="empty-state">No progress updates saved yet.</div>
        <ProgressEntry v-for="entry in entries" v-else :key="entry.id" :entry="entry" />
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
        }
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
}
</script>
