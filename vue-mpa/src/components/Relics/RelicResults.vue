<template>
    <section class="relic-results" aria-live="polite">
        <div class="result-meta">
            <strong>{{ totalCount }} relics</strong>
            <span>Page {{ page }} of {{ safeTotalPages }}</span>
        </div>
        <RelicCard v-for="relic in relics" :key="relic.id" :relic="relic" :refinements="refinements"
            :expanded="expandedId === relic.id" @toggle="toggleRelic(relic.id)" />

        <nav class="pagination" aria-label="Relic pages">
            <label class="pagination-size">
                <span>Per page</span>
                <select :value="pageSize" @change="$emit('page-size-change', Number($event.target.value))">
                    <option v-for="option in pageSizeOptions" :key="option" :value="option">{{ option }}</option>
                </select>
            </label>
            <div class="pagination-controls">
                <button class="btn btn-secondary" type="button" :disabled="page <= 1"
                    @click="$emit('page-change', 1)">First</button>
                <button class="btn btn-secondary" type="button" :disabled="page <= 1"
                    @click="$emit('page-change', page - 1)">Previous</button>
                <span class="pagination-position">{{ page }} / {{ safeTotalPages }}</span>
                <button class="btn btn-secondary" type="button" :disabled="page >= totalPages"
                    @click="$emit('page-change', page + 1)">Next</button>
                <button class="btn btn-secondary" type="button" :disabled="page >= totalPages"
                    @click="$emit('page-change', safeTotalPages)">Last</button>
            </div>
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
        pageSize: { type: Number, required: true },
        pageSizeOptions: { type: Array, required: true },
        totalPages: { type: Number, required: true }
    },
    emits: ['page-change', 'page-size-change'],
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
