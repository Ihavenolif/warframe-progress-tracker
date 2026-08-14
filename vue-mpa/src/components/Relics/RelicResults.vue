<template>
    <section class="relic-results" aria-live="polite">
        <div class="result-meta">
            <strong>{{ totalCount }} relics</strong>
            <span>Page {{ page }} of {{ safeTotalPages }}</span>
        </div>
        <RelicCard v-for="relic in relics" :key="relic.id" :relic="relic" :refinements="refinements"
            :expanded="expandedId === relic.id" @toggle="toggleRelic(relic.id)" />

        <nav class="pagination" aria-label="Relic pages">
            <button type="button" :disabled="page <= 1" @click="$emit('page-change', page - 1)">Previous</button>
            <span>{{ page }} / {{ safeTotalPages }}</span>
            <button type="button" :disabled="page >= totalPages" @click="$emit('page-change', page + 1)">Next</button>
        </nav>
    </section>
</template>

<script>
import RelicCard from './RelicCard.vue';

export default {
    name: 'RelicResults',
    components: { RelicCard },
    props: {
        relics: { type: Array, required: true },
        refinements: { type: Array, required: true },
        totalCount: { type: Number, required: true },
        page: { type: Number, required: true },
        totalPages: { type: Number, required: true }
    },
    emits: ['page-change'],
    data() {
        return { expandedId: null };
    },
    computed: {
        safeTotalPages() {
            return Math.max(this.totalPages, 1);
        }
    },
    watch: {
        relics() {
            this.expandedId = null;
        }
    },
    methods: {
        toggleRelic(id) {
            this.expandedId = this.expandedId === id ? null : id;
        }
    }
};
</script>

<style scoped>
.result-meta { display: flex; justify-content: space-between; margin: 0 2px 10px; color: #607080; }
.pagination { display: flex; justify-content: center; align-items: center; gap: 16px; margin-top: 24px; }
.pagination button { padding: 9px 16px; border: 0; background: #263746; color: white; cursor: pointer; }
.pagination button:disabled { opacity: .35; cursor: default; }
</style>
