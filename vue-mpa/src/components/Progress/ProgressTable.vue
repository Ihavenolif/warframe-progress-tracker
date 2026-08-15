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

            <button v-if="hasActiveFilters" type="button" class="btn btn-outline-secondary clear-filters" @click="clearFilters">Clear filters</button>
            <RouterLink v-if="showImport" class="btn btn-outline-secondary import-progress-link" to="/progress/import">Import progress</RouterLink>

            <div class="sidebar-resize-handle" title="Resize filters" @pointerdown="startSidebarResize"></div>
        </aside>

        <button v-if="!filtersVisible" type="button" class="btn filter-bubble" @click="filtersVisible = true">Filters</button>

        <section class="table-pane">
            <div class="table-container">
                <table class="progress-table">
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
            return this.itemList.filter(item => this.filterItem(item));
        },
        hasActiveFilters() {
            return this.itemNameFilter.length > 0 || this.selectedItemClasses.length > 0;
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
            sorting: { key: "", asc: true },
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
            itemNameFilter: "",
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
                this.sorting.asc = true;
            }

            if (this.playerNames.includes(this.sorting.key)) {
                this.itemList.sort((a, b) => {
                    // IF both are mastered, keep original order
                    if (a[this.sorting.key]["xpGained"] >= a["xpRequired"] && b[this.sorting.key]["xpGained"] >= b["xpRequired"]) return 0;

                    const maxRankA = getMaxRank(a["xpRequired"]);
                    const maxRankB = getMaxRank(b["xpRequired"]);

                    const masteredRateA = getRank(a["xpRequired"], a[this.sorting.key]["xpGained"]) / getMaxRank(a["xpRequired"]) //a[this.sorting.key]["xpGained"] / a["xpRequired"]
                    const masteredRateB = getRank(b["xpRequired"], b[this.sorting.key]["xpGained"]) / getMaxRank(b["xpRequired"]) //b[this.sorting.key]["xpGained"] / b["xpRequired"]

                    if (masteredRateA == masteredRateB) return 0;

                    if (masteredRateA > 0 && masteredRateB > 0 && masteredRateA < 1 && masteredRateB < 1) {
                        if (maxRankA != maxRankB) {
                            return (maxRankB < maxRankA ? -1 : 1) * (this.sorting.asc ? 1 : -1);
                        }
                    }

                    return (masteredRateB < masteredRateA ? -1 : 1) * (this.sorting.asc ? 1 : -1);
                })
            }
            else {
                this.itemList.sort((a, b) => {
                    return (a[this.sorting.key] < b[this.sorting.key] ? -1 : 1) * (this.sorting.asc ? 1 : -1);
                })
            }
        },
        filterItem(item) {
            const validClass = this.selectedItemClasses.length === 0 || this.selectedItemClasses.includes(item.itemClass);
            const validName = item.itemName.toLowerCase().includes(this.itemNameFilter.toLowerCase());
            return validClass && validName;
        },
        clearFilters() {
            this.itemNameFilter = "";
            this.selectedItemClasses = [];
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
        this.sortTable("itemName");
        this.sortTable("itemClass");
        this.playerNames.forEach(name => {
            this.sortTable(name);
        });
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
