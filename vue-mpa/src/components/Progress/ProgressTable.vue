<template>

    <div class="progress-workspace">
        <aside v-show="filtersVisible" class="filter-sidebar" :style="sidebarProperties">
            <div class="filter-sidebar-head">
                <h2>Filters</h2>
                <button type="button" class="btn close-filters" aria-label="Hide filters" @click="filtersVisible = false">&times;</button>
            </div>

            <label class="search-filter" for="itemNameFilter">
                <input id="itemNameFilter" type="search" v-model="itemNameFilter" placeholder="Search items">
            </label>

            <CollapsibleContainer title="Item classes">
                <div class="checkbox-grid">
                    <label v-for="itemClass in allItemClasses" :key="itemClass"
                        :class="['checkbox-item', selectedItemClasses.includes(itemClass) ? 'checked' : '']">
                        <input type="checkbox" :value="itemClass" v-model="selectedItemClasses" />
                        <span>{{ itemClass }}</span>
                    </label>
                </div>
            </CollapsibleContainer>

            <CollapsibleContainer title="Mastery state">
                <div class="checkbox-grid mastery-filter-grid">
                    <label v-for="state in masteryStates" :key="state.value"
                        :class="['checkbox-item', selectedMasteryStates.includes(state.value) ? 'checked' : '']">
                        <input type="checkbox" :value="state.value" v-model="selectedMasteryStates"
                            @change="updateRouteQuery" />
                        <span>{{ state.label }}</span>
                    </label>
                </div>
            </CollapsibleContainer>

            <button v-if="hasActiveFilters" type="button" class="btn btn-outline-secondary clear-filters" @click="clearFilters">Clear filters</button>
            <RouterLink v-if="showImport" class="btn btn-outline-secondary import-progress-link" to="/progress/import">Import progress</RouterLink>

            <div class="sidebar-resize-handle" title="Resize filters" @pointerdown="startSidebarResize"></div>
        </aside>

        <button v-if="!filtersVisible" type="button" class="btn filter-bubble" @click="filtersVisible = true">Filters</button>

        <section class="table-pane">
            <div class="table-container">
                <slot name="heading"></slot>
                <div class="progress-result-meta" aria-live="polite">
                    Showing {{ filteredItems.length }} of {{ itemList.length }} items
                </div>
                <div v-if="filteredItems.length === 0" class="progress-empty-state">
                    <strong>{{ hasActiveFilters ? 'No items match active filters.' : 'No mastery items available.' }}</strong>
                    <span v-if="hasActiveFilters">Change search, item class, or mastery state filters to see results.</span>
                </div>
                <table v-else class="progress-table">
                    <thead>
                        <tr>
                            <th></th>
                            <th id="itemNameHead" v-on:click="sortTable('itemName')">Item name <i
                                    v-if="this.sorting.key === 'itemName'"><span
                                        :class="['fa', 'table-head-caret', this.sorting.asc ? 'fa-caret-down' : 'fa-caret-up']"></span></i>
                            </th>
                            <th id="classHead" v-on:click="sortTable('itemClass')">Item Class <i
                                    v-if="this.sorting.key === 'itemClass'"><span
                                        :class="['fa', 'table-head-caret', this.sorting.asc ? 'fa-caret-down' : 'fa-caret-up']"></span></i>
                            </th>
                            <th v-for="(name, index) in playerNames" :key="index" v-on:click="sortTable(name)">
                                {{ name }} <i v-if="this.sorting.key === name"><span
                                        :class="['fa', 'table-head-caret', this.sorting.asc ? 'fa-caret-down' : 'fa-caret-up']"></span></i>
                            </th>
                        </tr>
                    </thead>

                    <tbody id="tableBody">
                        <tr v-for="item in filteredItems" :key="item.uniqueName" class="progress-table-row">
                            <ProgressTableItem v-bind:item="item" v-bind:playerNames="playerNames" ref="progressTableItem">
                            </ProgressTableItem>
                        </tr>
                    </tbody>
                </table>
            </div>
        </section>
    </div>
</template>

<script>
import { getMaxRank, getRank } from '@/util/util';
import CollapsibleContainer from '../Collapsible.vue';
import ProgressTableItem from './ProgressTableItem.vue';

export default {
    name: "ProgressTable",
    computed: {
        username() {
            return this.$store.state.username;
        },
        token() {
            return this.$store.state.token;
        },
        filteredItems() {
            return this.itemList.filter(item => this.filterItem(item)).sort(this.compareItems);
        },
        hasActiveFilters() {
            return this.itemNameFilter.length > 0 || this.selectedItemClasses.length > 0 ||
                this.selectedMasteryStates.length > 0;
        },
        sidebarProperties() {
            return {
                '--sidebar-width': `${this.sidebarWidth}px`
            };
        }
    },
    components: {
        ProgressTableItem,
        CollapsibleContainer
    },
    props: {
        _playerNames: {
            type: Array,
            required: true
        },
        _itemList: {
            type: Array,
            required: true
        },
        showImport: {
            type: Boolean,
            default: true
        }
    },
    data() {
        const storedSidebarWidth = Number(localStorage.getItem("progressFilterSidebarWidth"));

        return {
            playerNames: this._playerNames,
            itemList: this._itemList,
            sorting: { key: 'itemName', asc: true },
            allItemClasses: [
                "Amp",
                "Archgun",
                "Archmelee",
                "Archwing",
                "Hound",
                "Kdrive",
                "Kitgun",
                "Melee",
                "Moa",
                "Necramech",
                "Pet",
                "Primary",
                "Secondary",
                "Sentinel",
                "Sentinel Weapon",
                "Warframe",
                "Zaw"
            ],
            selectedItemClasses: [],
            masteryStates: [
                { value: 'mastered', label: 'Mastered' },
                { value: 'in-progress', label: 'In progress' },
                { value: 'unowned', label: 'Unowned' },
                { value: 'craft-ready', label: 'Craft ready' }
            ],
            selectedMasteryStates: [],
            itemNameFilter: '',
            filtersVisible: true,
            sidebarWidth: Number.isFinite(storedSidebarWidth) && storedSidebarWidth > 0 ? storedSidebarWidth : 280,
            sidebarResizeStartX: 0,
            sidebarResizeStartWidth: 0
        }
    },
    methods: {
        sortTable(sortKey) {
            if (this.sorting.key == sortKey) this.sorting.asc = !this.sorting.asc;
            else {
                this.sorting.key = sortKey;
                this.sorting.asc = ['itemName', 'itemClass'].includes(sortKey);
            }
            this.updateRouteQuery();
        },
        compareItems(a, b) {
            let comparison;
            if (this.sorting.key === 'closest') {
                comparison = this.getClosestMasteryRate(a) - this.getClosestMasteryRate(b);
            } else if (this.playerNames.includes(this.sorting.key)) {
                comparison = this.getMasteryRate(a, this.sorting.key) - this.getMasteryRate(b, this.sorting.key);
            } else {
                comparison = String(a[this.sorting.key] || '').localeCompare(String(b[this.sorting.key] || ''));
            }
            if (comparison !== 0) return comparison * (this.sorting.asc ? 1 : -1);
            return a.itemName.localeCompare(b.itemName);
        },
        getMasteryRate(item, playerName) {
            const player = item[playerName];
            if (!player || player.xpGained == null) return -1;
            return getRank(item.xpRequired, player.xpGained) / getMaxRank(item.xpRequired);
        },
        getClosestMasteryRate(item) {
            return Math.max(...this.playerNames.map(name => this.getMasteryRate(item, name)), -1);
        },
        filterItem(item) {
            const validClass = this.selectedItemClasses.length === 0 || this.selectedItemClasses.includes(item.itemClass);
            const validName = item.itemName.toLowerCase().includes(this.itemNameFilter.toLowerCase());
            const validState = this.selectedMasteryStates.length === 0 || this.playerNames.some(name =>
                this.selectedMasteryStates.includes(this.getMasteryState(item, name)));
            return validClass && validName && validState;
        },
        getMasteryState(item, playerName) {
            const player = item[playerName];
            if (!player || player.xpGained == null) {
                if (this.isCraftReady(item, player)) return 'craft-ready';
                return 'unowned';
            }
            return player.xpGained >= item.xpRequired ? 'mastered' : 'in-progress';
        },
        isCraftReady(item, player) {
            return Boolean(item.recipeUniqueName) && player?.blueprintOwned === true &&
                Array.isArray(player.components) && player.components.every(component =>
                    (component.countOwned || 0) >= (component.countRequired || 0));
        },
        clearFilters() {
            this.itemNameFilter = '';
            this.selectedItemClasses = [];
            this.selectedMasteryStates = [];
            this.updateRouteQuery();
        },
        readRouteQuery() {
            const query = this.$route.query;
            const parseList = value => typeof value === 'string' ? value.split(',').filter(Boolean) : [];
            const validStates = this.masteryStates.map(state => state.value);
            const validSortKeys = ['itemName', 'itemClass', 'closest', ...this.playerNames];
            this.itemNameFilter = typeof query.search === 'string' ? query.search : '';
            this.selectedItemClasses = parseList(query.classes).filter(itemClass => this.allItemClasses.includes(itemClass));
            this.selectedMasteryStates = parseList(query.states).filter(state => validStates.includes(state));
            this.sorting.key = validSortKeys.includes(query.sort) ? query.sort : 'itemName';
            const defaultAscending = ['itemName', 'itemClass'].includes(this.sorting.key);
            this.sorting.asc = query.order === 'asc' ? true : query.order === 'desc' ? false : defaultAscending;
        },
        updateRouteQuery() {
            const query = {};
            if (this.itemNameFilter) query.search = this.itemNameFilter;
            if (this.selectedItemClasses.length) query.classes = [...this.selectedItemClasses].sort().join(',');
            if (this.selectedMasteryStates.length) query.states = [...this.selectedMasteryStates].sort().join(',');
            if (this.sorting.key !== 'itemName') query.sort = this.sorting.key;
            if (this.sorting.asc !== ['itemName', 'itemClass'].includes(this.sorting.key)) {
                query.order = this.sorting.asc ? 'asc' : 'desc';
            }
            this.$router.replace({ query });
        },
        startSidebarResize(event) {
            event.preventDefault();
            this.sidebarResizeStartX = event.clientX;
            this.sidebarResizeStartWidth = this.sidebarWidth;
            document.body.classList.add("resizing-progress-sidebar");
            window.addEventListener("pointermove", this.resizeSidebar);
            window.addEventListener("pointerup", this.stopSidebarResize, { once: true });
            window.addEventListener("pointercancel", this.stopSidebarResize, { once: true });
        },
        resizeSidebar(event) {
            const maxWidth = Math.max(220, Math.min(520, window.innerWidth - 300));
            this.sidebarWidth = Math.min(maxWidth, Math.max(220,
                this.sidebarResizeStartWidth + event.clientX - this.sidebarResizeStartX));
        },
        stopSidebarResize() {
            localStorage.setItem("progressFilterSidebarWidth", String(this.sidebarWidth));
            document.body.classList.remove("resizing-progress-sidebar");
            window.removeEventListener("pointermove", this.resizeSidebar);
            window.removeEventListener("pointercancel", this.stopSidebarResize);
        }

    },
    async mounted() {
        document.body.classList.add("progress-scroll-locked");
        this.readRouteQuery();
    },
    watch: {
        '$route.query': {
            handler() {
                this.readRouteQuery();
            },
            deep: true
        },
        itemNameFilter() {
            this.updateRouteQuery();
        },
        selectedItemClasses: {
            handler() {
                this.updateRouteQuery();
            },
            deep: true
        }
    },
    beforeUnmount() {
        document.body.classList.remove("progress-scroll-locked");
        document.body.classList.remove("resizing-progress-sidebar");
        window.removeEventListener("pointermove", this.resizeSidebar);
        window.removeEventListener("pointerup", this.stopSidebarResize);
        window.removeEventListener("pointercancel", this.stopSidebarResize);
    }
}
</script>
