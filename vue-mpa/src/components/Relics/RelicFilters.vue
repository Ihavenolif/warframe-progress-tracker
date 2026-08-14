<template>
    <section class="filters" aria-label="Relic filters">
        <label class="search-field">
            <span>Search relics or rewards</span>
            <input v-model="draftSearch" type="search" placeholder="e.g. Lith A12 or Braton Prime" @input="queueSearch">
        </label>

        <div class="filter-group era-filter">
            <span>Era</span>
            <div class="chips">
                <button v-for="value in eras" :key="value || 'all'" type="button"
                    :class="{ selected: filters.era === value }" @click="setFilter('era', value)">
                    {{ value || 'All eras' }}
                </button>
            </div>
        </div>

        <div class="filter-group ownership-filter">
            <span>Ownership</span>
            <div class="chips">
                <button v-for="option in ownershipOptions" :key="option.value" type="button"
                    :class="{ selected: filters.owned === option.value }" @click="setFilter('owned', option.value)">
                    {{ option.label }}
                </button>
            </div>
        </div>
        <div class="filter-group sort-filter">
            <span>Sort by</span>
            <div class="chips">
                <button v-for="option in sortOptions" :key="option.value" type="button"
                    :class="{ selected: filters.sort === option.value }" @click="setFilter('sort', option.value)">
                    {{ option.label }}
                </button>
            </div>
        </div>
    </section>
</template>

<script>
export default {
    name: 'RelicFilters',
    props: {
        filters: { type: Object, required: true },
        eras: { type: Array, required: true }
    },
    emits: ['change'],
    data() {
        return {
            draftSearch: this.filters.search,
            searchTimer: null,
            ownershipOptions: [
                { value: 'all', label: 'All relics' },
                { value: 'owned', label: 'Owned' },
                { value: 'unowned', label: 'Unowned' }
            ],
            sortOptions: [
                { value: 'name', label: 'Name' },
                { value: 'era', label: 'Era' },
                { value: 'owned', label: 'Owned count' }
            ]
        };
    },
    watch: {
        'filters.search'(value) {
            this.draftSearch = value;
        }
    },
    beforeUnmount() {
        clearTimeout(this.searchTimer);
    },
    methods: {
        queueSearch() {
            clearTimeout(this.searchTimer);
            this.searchTimer = setTimeout(() => {
                this.setFilter('search', this.draftSearch.trim());
            }, 300);
        },
        setFilter(key, value) {
            this.$emit('change', { ...this.filters, [key]: value });
        }
    }
};
</script>

<style scoped>
.filters { display: grid; grid-template-columns: repeat(12, minmax(0, 1fr)); gap: 16px; align-items: end; padding: 20px; background: #eef2f4; border-top: 4px solid #263746; margin-bottom: 24px; }
.filters label, .filter-group { display: grid; gap: 7px; font-size: .8rem; font-weight: 700; }
.search-field, .era-filter, .ownership-filter, .sort-filter { grid-column: span 6; }
input { width: 100%; padding: 10px 11px; border: 1px solid #b9c3c9; background: white; color: #263746; font: inherit; }
.chips { display: flex; flex-wrap: wrap; gap: 6px; }
.chips button { padding: 8px 12px; border: 1px solid #aebbc3; background: white; cursor: pointer; }
.chips button.selected { background: #263746; border-color: #263746; color: white; }

@media (max-width: 800px) {
    .search-field { grid-column: 1 / -1; }
    .era-filter, .ownership-filter, .sort-filter { grid-column: span 6; }
}
@media (max-width: 560px) {
    .filters { padding: 14px; }
    .era-filter, .ownership-filter, .sort-filter { grid-column: 1 / -1; }
}
</style>
