<template>
    <section class="relic-filters" aria-label="Relic filters">
        <label class="relic-search-field">
            <span>Search relics or rewards</span>
            <input v-model="draftSearch" type="search" placeholder="e.g. Lith A12 or Braton Prime" @input="queueSearch">
        </label>

        <div class="relic-filter-group relic-era-filter">
            <span>Era</span>
            <div class="relic-filter-chips">
                <button v-for="value in eras" :key="value || 'all'" type="button"
                    :class="{ 'is-selected': filters.era === value }" @click="setFilter('era', value)">
                    {{ value || 'All eras' }}
                </button>
            </div>
        </div>

        <div class="relic-filter-group relic-ownership-filter">
            <span>Ownership</span>
            <div class="relic-filter-chips">
                <button v-for="option in ownershipOptions" :key="option.value" type="button"
                    :class="{ 'is-selected': filters.owned === option.value }" @click="setFilter('owned', option.value)">
                    {{ option.label }}
                </button>
            </div>
        </div>
        <div class="relic-filter-group relic-sort-filter">
            <span>Sort by</span>
            <div class="relic-filter-chips">
                <button v-for="option in sortOptions" :key="option.value" type="button"
                    :class="{ 'is-selected': filters.sort === option.value }" @click="setFilter('sort', option.value)">
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
