<template>
    <NavbarElement />
    <main class="content-page relic-page">
        <RelicPageHeader />
        <RelicFilters :filters="filters" :eras="eras" @change="filtersChanged" />

        <LoadingIndicator v-if="loading" />
        <RelicStatus v-else-if="noPlayer" type="no-player" />
        <RelicStatus v-else-if="error" type="error" :message="error" @retry="loadRelics" />
        <RelicStatus v-else-if="relics.length === 0" type="empty" />
        <RelicResults v-else :relics="relics" :refinements="refinements" :total-count="totalCount"
            :page="page" :page-size="pageSize" :page-size-options="pageSizeOptions" :total-pages="totalPages"
            @page-change="goToPage" @page-size-change="changePageSize" />
    </main>
</template>

<script>
import LoadingIndicator from '@/components/LoadingIndicator.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import RelicFilters from '@/components/Relics/RelicFilters.vue';
import RelicPageHeader from '@/components/Relics/RelicPageHeader.vue';
import RelicResults from '@/components/Relics/RelicResults.vue';
import RelicStatus from '@/components/Relics/RelicStatus.vue';
import { authFetch } from '@/util/util';

export default {
    name: 'RelicBrowserPage',
    components: {
        LoadingIndicator,
        NavbarElement,
        RelicFilters,
        RelicPageHeader,
        RelicResults,
        RelicStatus
    },
    data() {
        return {
            eras: ['', 'Lith', 'Meso', 'Neo', 'Axi'],
            refinements: ['Intact', 'Exceptional', 'Flawless', 'Radiant'],
            filters: { search: '', era: '', owned: 'all', sort: 'name' },
            relics: [],
            page: 1,
            pageSize: 20,
            pageSizeOptions: [10, 20, 50, 100],
            totalCount: 0,
            totalPages: 0,
            loading: true,
            error: '',
            noPlayer: false,
            requestNumber: 0
        };
    },
    created() {
        this.readRouteQuery();
        this.loadRelics();
    },
    methods: {
        readRouteQuery() {
            const query = this.$route.query;
            this.filters = {
                search: typeof query.search === 'string' ? query.search : '',
                era: this.eras.includes(query.era) ? query.era : '',
                owned: ['all', 'owned', 'unowned'].includes(query.owned) ? query.owned : 'all',
                sort: ['name', 'era', 'owned'].includes(query.sort) ? query.sort : 'name'
            };
            this.page = Math.max(Number.parseInt(query.page, 10) || 1, 1);
            const pageSize = Number.parseInt(query.pageSize, 10);
            this.pageSize = this.pageSizeOptions.includes(pageSize) ? pageSize : 20;
        },
        filtersChanged(filters) {
            this.filters = filters;
            this.page = 1;
            this.updateRouteAndLoad();
        },
        goToPage(page) {
            this.page = page;
            this.updateRouteAndLoad();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        },
        changePageSize(pageSize) {
            this.pageSize = pageSize;
            this.page = 1;
            this.updateRouteAndLoad();
        },
        async updateRouteAndLoad() {
            const query = {};
            Object.entries(this.filters).forEach(([key, value]) => {
                if (value && value !== 'all' && !(key === 'sort' && value === 'name')) query[key] = value;
            });
            if (this.page > 1) query.page = String(this.page);
            if (this.pageSize !== 20) query.pageSize = String(this.pageSize);
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
            ['search', 'era'].forEach(key => {
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
                if (data.totalPages > 0 && this.page > data.totalPages) {
                    this.page = data.totalPages;
                    await this.updateRouteAndLoad();
                    return;
                }
                this.relics = data.items;
                this.page = data.page;
                this.totalCount = data.totalCount;
                this.totalPages = data.totalPages;
            } catch (error) {
                if (currentRequest === this.requestNumber) this.error = error.message || 'Unknown error';
            } finally {
                if (currentRequest === this.requestNumber) this.loading = false;
            }
        }
    }
};
</script>
