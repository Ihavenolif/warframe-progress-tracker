<template>
    <NavbarElement />
    <main class="relic-page">
        <header class="page-heading">
            <div>
                <p class="eyebrow">Void relic inventory</p>
                <h1>Relics</h1>
                <p>Browse known relics and quantities from your latest profile import.</p>
            </div>
            <RouterLink class="import-link" to="/progress/import">Refresh inventory</RouterLink>
        </header>

        <section class="filters" aria-label="Relic filters">
            <label class="search-field">
                <span>Search relics or rewards</span>
                <input v-model="draftSearch" type="search" placeholder="e.g. Lith A12 or Braton Prime" @input="queueSearch">
            </label>

            <div class="filter-group">
                <span>Era</span>
                <div class="chips">
                    <button v-for="value in eras" :key="value || 'all'" type="button"
                        :class="{ selected: filters.era === value }" @click="setFilter('era', value)">
                        {{ value || 'All eras' }}
                    </button>
                </div>
            </div>

            <label>
                <span>Ownership</span>
                <select v-model="filters.owned" @change="filtersChanged">
                    <option value="all">All relics</option>
                    <option value="owned">Owned</option>
                    <option value="unowned">Unowned</option>
                </select>
            </label>
            <label>
                <span>Owned refinement</span>
                <select v-model="filters.refinement" @change="filtersChanged">
                    <option value="">Any refinement</option>
                    <option v-for="value in refinements" :key="value" :value="value">{{ value }}</option>
                </select>
            </label>
            <label>
                <span>Sort by</span>
                <select v-model="filters.sort" @change="filtersChanged">
                    <option value="name">Name</option>
                    <option value="era">Era</option>
                    <option value="owned">Owned count</option>
                </select>
            </label>
        </section>

        <LoadingIndicator v-if="loading" />
        <section v-else-if="noPlayer" class="state-card">
            <h2>Link a player first</h2>
            <p>Relic ownership is tied to your Warframe player profile.</p>
            <RouterLink to="/settings">Open settings</RouterLink>
        </section>
        <section v-else-if="error" class="state-card error-state">
            <h2>Relics could not be loaded</h2>
            <p>{{ error }}</p>
            <button type="button" @click="loadRelics">Try again</button>
        </section>
        <section v-else-if="relics.length === 0" class="state-card">
            <h2>No relics found</h2>
            <p>Change filters, or ask an admin to refresh PublicExport metadata.</p>
        </section>

        <section v-else class="relic-results" aria-live="polite">
            <div class="result-meta">
                <strong>{{ totalCount }} relics</strong>
                <span>Page {{ page }} of {{ Math.max(totalPages, 1) }}</span>
            </div>
            <article v-for="relic in relics" :key="relic.id" class="relic-card" :class="`era-${relic.era.toLowerCase()}`">
                <button class="relic-summary" type="button" :aria-expanded="expandedId === relic.id"
                    @click="toggleRelic(relic.id)">
                    <span class="era-mark">{{ relic.era }}</span>
                    <span class="relic-name">{{ relic.name }}</span>
                    <span class="owned-total"><strong>{{ relic.totalOwned }}</strong> owned</span>
                    <span class="expand-mark" aria-hidden="true">{{ expandedId === relic.id ? '−' : '+' }}</span>
                </button>

                <div v-if="expandedId === relic.id" class="relic-details">
                    <section>
                        <h3>Refinements</h3>
                        <div class="refinement-grid">
                            <div v-for="refinement in refinements" :key="refinement" class="refinement-tile">
                                <span>{{ refinement }}</span>
                                <strong>{{ refinementCount(relic, refinement) }}</strong>
                            </div>
                        </div>
                        <p v-if="hasAliases(relic)" class="alias-note">Counts include internal aliases for the same refinement.</p>
                    </section>
                    <section>
                        <h3>Rewards</h3>
                        <div class="reward-list">
                            <div v-for="reward in relic.rewards" :key="reward.uniqueName" class="reward-row">
                                <span class="rarity" :class="reward.rarity.toLowerCase()">{{ reward.rarity }}</span>
                                <span>{{ reward.itemName || reward.uniqueName }}</span>
                                <span v-if="reward.itemCount > 1">×{{ reward.itemCount }}</span>
                            </div>
                        </div>
                    </section>
                </div>
            </article>

            <nav class="pagination" aria-label="Relic pages">
                <button type="button" :disabled="page <= 1" @click="goToPage(page - 1)">Previous</button>
                <span>{{ page }} / {{ Math.max(totalPages, 1) }}</span>
                <button type="button" :disabled="page >= totalPages" @click="goToPage(page + 1)">Next</button>
            </nav>
        </section>
    </main>
</template>

<script>
import LoadingIndicator from '@/components/LoadingIndicator.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import { authFetch } from '@/util/util';

export default {
    name: 'RelicBrowserPage',
    components: { LoadingIndicator, NavbarElement },
    data() {
        return {
            eras: ['', 'Lith', 'Meso', 'Neo', 'Axi'],
            refinements: ['Intact', 'Exceptional', 'Flawless', 'Radiant'],
            filters: { search: '', era: '', refinement: '', owned: 'all', sort: 'name' },
            draftSearch: '',
            relics: [],
            page: 1,
            pageSize: 20,
            totalCount: 0,
            totalPages: 0,
            loading: true,
            error: '',
            noPlayer: false,
            expandedId: null,
            searchTimer: null,
            requestNumber: 0
        };
    },
    created() {
        this.readRouteQuery();
        this.loadRelics();
    },
    beforeUnmount() {
        clearTimeout(this.searchTimer);
    },
    methods: {
        readRouteQuery() {
            const query = this.$route.query;
            this.filters.search = typeof query.search === 'string' ? query.search : '';
            this.filters.era = this.eras.includes(query.era) ? query.era : '';
            this.filters.refinement = this.refinements.includes(query.refinement) ? query.refinement : '';
            this.filters.owned = ['all', 'owned', 'unowned'].includes(query.owned) ? query.owned : 'all';
            this.filters.sort = ['name', 'era', 'owned'].includes(query.sort) ? query.sort : 'name';
            this.page = Math.max(Number.parseInt(query.page, 10) || 1, 1);
            this.draftSearch = this.filters.search;
        },
        queueSearch() {
            clearTimeout(this.searchTimer);
            this.searchTimer = setTimeout(() => {
                this.filters.search = this.draftSearch.trim();
                this.filtersChanged();
            }, 300);
        },
        setFilter(key, value) {
            this.filters[key] = value;
            this.filtersChanged();
        },
        filtersChanged() {
            this.page = 1;
            this.expandedId = null;
            this.updateRouteAndLoad();
        },
        goToPage(page) {
            this.page = page;
            this.expandedId = null;
            this.updateRouteAndLoad();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        },
        async updateRouteAndLoad() {
            const query = {};
            Object.entries(this.filters).forEach(([key, value]) => {
                if (value && value !== 'all' && !(key === 'sort' && value === 'name')) query[key] = value;
            });
            if (this.page > 1) query.page = String(this.page);
            await this.$router.replace({ query });
            await this.loadRelics();
        },
        async loadRelics() {
            const currentRequest = ++this.requestNumber;
            this.loading = true;
            this.error = '';
            this.noPlayer = false;
            const params = new URLSearchParams({
                owned: this.filters.owned,
                sort: this.filters.sort,
                page: String(this.page),
                pageSize: String(this.pageSize)
            });
            ['search', 'era', 'refinement'].forEach(key => {
                if (this.filters[key]) params.set(key, this.filters[key]);
            });
            try {
                const response = await authFetch(`/api/relics?${params}`);
                if (!response || currentRequest !== this.requestNumber) return;
                if (response.status === 404) {
                    this.noPlayer = true;
                    this.relics = [];
                    return;
                }
                if (!response.ok) throw new Error((await response.text()) || `Request failed (${response.status})`);
                const data = await response.json();
                this.relics = data.items;
                this.page = data.page;
                this.totalCount = data.totalCount;
                this.totalPages = data.totalPages;
            } catch (error) {
                if (currentRequest === this.requestNumber) this.error = error.message || 'Unknown error';
            } finally {
                if (currentRequest === this.requestNumber) this.loading = false;
            }
        },
        toggleRelic(id) {
            this.expandedId = this.expandedId === id ? null : id;
        },
        refinementCount(relic, refinement) {
            return relic.variants.filter(variant => variant.refinement === refinement)
                .reduce((total, variant) => total + variant.quantity, 0);
        },
        hasAliases(relic) {
            return this.refinements.some(refinement => relic.variants.filter(variant => variant.refinement === refinement).length > 1);
        }
    }
};
</script>

<style scoped>
.relic-page { max-width: 1120px; margin: 0 auto; padding: 36px 24px 64px; }
.page-heading { display: flex; justify-content: space-between; gap: 24px; align-items: end; margin-bottom: 28px; }
.page-heading h1 { margin: 2px 0 4px; font-size: clamp(2rem, 5vw, 3.3rem); line-height: 1; }
.page-heading p { margin: 0; color: #607080; }
.eyebrow { color: var(--accent) !important; font-size: .75rem; font-weight: 800; letter-spacing: .14em; text-transform: uppercase; }
.import-link { background: #263746; color: white; padding: 11px 16px; text-decoration: none; border-radius: 3px; white-space: nowrap; }
.filters { display: grid; grid-template-columns: minmax(260px, 2fr) repeat(3, minmax(145px, 1fr)); gap: 16px; align-items: end; padding: 20px; background: #eef2f4; border-top: 4px solid #263746; margin-bottom: 24px; }
.filters label, .filter-group { display: grid; gap: 7px; font-size: .8rem; font-weight: 700; }
.search-field, .filter-group { grid-column: span 2; }
input, select { width: 100%; padding: 10px 11px; border: 1px solid #b9c3c9; background: white; color: #263746; font: inherit; }
.chips { display: flex; flex-wrap: wrap; gap: 6px; }
.chips button { padding: 8px 12px; border: 1px solid #aebbc3; background: white; cursor: pointer; }
.chips button.selected { background: #263746; border-color: #263746; color: white; }
.result-meta { display: flex; justify-content: space-between; margin: 0 2px 10px; color: #607080; }
.relic-card { --era-color: #777; border: 1px solid #d5dde1; border-left: 5px solid var(--era-color); margin-bottom: 10px; background: white; box-shadow: 0 2px 8px rgba(38, 55, 70, .06); }
.era-lith { --era-color: #9d7454; } .era-meso { --era-color: #4b89a7; } .era-neo { --era-color: #789047; } .era-axi { --era-color: #9a5f9e; }
.relic-summary { display: grid; grid-template-columns: 90px 1fr auto 28px; gap: 16px; align-items: center; width: 100%; padding: 18px; border: 0; background: transparent; color: inherit; text-align: left; cursor: pointer; }
.era-mark { color: var(--era-color); font-size: .75rem; font-weight: 900; letter-spacing: .1em; text-transform: uppercase; }
.relic-name { font-size: 1.05rem; font-weight: 800; }
.owned-total { color: #607080; } .owned-total strong { color: #263746; font-size: 1.15rem; }
.expand-mark { font-size: 1.5rem; color: var(--era-color); }
.relic-details { display: grid; grid-template-columns: minmax(280px, .9fr) minmax(320px, 1.1fr); gap: 30px; padding: 4px 24px 24px 130px; border-top: 1px solid #e5eaed; }
.relic-details h3 { font-size: .8rem; letter-spacing: .1em; text-transform: uppercase; }
.refinement-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; }
.refinement-tile { display: grid; gap: 5px; padding: 12px 8px; background: #eef2f4; text-align: center; font-size: .7rem; }
.refinement-tile strong { font-size: 1.25rem; }
.alias-note { color: #607080; font-size: .8rem; }
.reward-list { display: grid; gap: 6px; }
.reward-row { display: grid; grid-template-columns: 76px 1fr auto; gap: 10px; align-items: center; padding: 8px 10px; background: #f5f7f8; font-size: .9rem; }
.rarity { padding: 3px 6px; color: white; font-size: .68rem; font-weight: 800; text-align: center; text-transform: uppercase; }
.rarity.common { background: #8b765f; } .rarity.uncommon { background: #8b9399; } .rarity.rare { background: #b58a20; }
.pagination { display: flex; justify-content: center; align-items: center; gap: 16px; margin-top: 24px; }
.pagination button, .state-card button { padding: 9px 16px; border: 0; background: #263746; color: white; cursor: pointer; }
.pagination button:disabled { opacity: .35; cursor: default; }
.state-card { padding: 40px; border: 1px solid #d5dde1; background: #f7f9fa; text-align: center; }
.error-state { border-color: #c98b92; }

@media (max-width: 800px) {
    .filters { grid-template-columns: 1fr 1fr; }
    .search-field, .filter-group { grid-column: 1 / -1; }
    .relic-details { grid-template-columns: 1fr; padding-left: 24px; }
}
@media (max-width: 560px) {
    .relic-page { padding: 24px 12px 48px; }
    .page-heading { align-items: start; flex-direction: column; }
    .filters { grid-template-columns: 1fr; padding: 14px; }
    .search-field, .filter-group { grid-column: auto; }
    .relic-summary { grid-template-columns: 60px 1fr 24px; gap: 8px; padding: 14px 12px; }
    .owned-total { grid-column: 2; grid-row: 2; }
    .expand-mark { grid-column: 3; grid-row: 1 / span 2; }
    .refinement-grid { grid-template-columns: 1fr 1fr; }
    .relic-details { padding: 4px 14px 18px; }
    .reward-row { grid-template-columns: 68px 1fr; }
}
</style>
