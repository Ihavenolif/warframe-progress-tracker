<template>

    <div class="progress-workspace">
        <aside v-show="filtersVisible" class="filter-sidebar" :style="sidebarStyle">
            <div class="filter-sidebar-head">
                <h2>Filters</h2>
                <button type="button" class="close-filters" aria-label="Hide filters" @click="filtersVisible = false">&times;</button>
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

            <button v-if="hasActiveFilters" type="button" class="clear-filters" @click="clearFilters">Clear filters</button>
            <RouterLink v-if="showImport" class="import-progress-link" to="/progress/import">Import progress</RouterLink>

            <div class="sidebar-resize-handle" title="Resize filters" @pointerdown="startSidebarResize"></div>
        </aside>

        <button v-if="!filtersVisible" type="button" class="filter-bubble" @click="filtersVisible = true">Filters</button>

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
                        <tr v-for="item in filteredItems" :key="item.uniqueName" style="height: 38px !important;">
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
        sidebarStyle() {
            return {
                width: `${this.sidebarWidth}px`,
                flexBasis: `${this.sidebarWidth}px`
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
        this.previousBodyOverflow = document.body.style.overflow;
        document.body.style.overflow = "hidden";
        this.sortTable("itemName");
        this.sortTable("itemClass");
        this.playerNames.forEach(name => {
            this.sortTable(name);
        });
    },
    beforeUnmount() {
        document.body.style.overflow = this.previousBodyOverflow;
        document.body.classList.remove("resizing-progress-sidebar");
        window.removeEventListener("pointermove", this.resizeSidebar);
        window.removeEventListener("pointerup", this.stopSidebarResize);
        window.removeEventListener("pointercancel", this.stopSidebarResize);
    }
}
</script>

<style>
.progress-workspace {
    --progress-border: #d4d4d4;
    flex: 1;
    min-height: 0;
    display: flex;
    overflow: hidden;
    border-top: 1px solid var(--progress-border);
    position: relative;
}

.filter-sidebar {
    width: 280px;
    flex: 0 0 280px;
    overflow-y: auto;
    padding: 14px;
    border-right: 1px solid var(--progress-border);
    background: #f5f5f5;
    position: relative;
}

.filter-sidebar-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    margin-bottom: 16px;
}

.filter-sidebar-head h2 {
    margin: 0;
}

.clear-filters {
    border: 1px solid var(--progress-border);
    background: #fff;
    padding: 7px 10px;
    cursor: pointer;
}

.close-filters {
    border: 0;
    background: transparent;
    padding: 0 4px;
    color: #444;
    font-size: 26px;
    line-height: 1;
    cursor: pointer;
}

.filter-bubble {
    position: absolute;
    left: 0;
    top: 50%;
    z-index: 4;
    transform: translateY(-50%);
    border: 1px solid #444;
    border-left: 0;
    border-radius: 0 20px 20px 0;
    background: #444;
    color: #fff;
    padding: 16px 12px 16px 9px;
    cursor: pointer;
    font-weight: 600;
    writing-mode: vertical-rl;
    box-shadow: 2px 2px 5px rgba(0, 0, 0, 0.25);
}

.filter-bubble:hover {
    background: #555;
}

.sidebar-resize-handle {
    position: absolute;
    top: 0;
    right: -4px;
    bottom: 0;
    width: 8px;
    cursor: col-resize;
    touch-action: none;
}

.sidebar-resize-handle:hover {
    background: rgba(0, 123, 255, 0.15);
}

body.resizing-progress-sidebar,
body.resizing-progress-sidebar * {
    cursor: col-resize !important;
    user-select: none !important;
}

.clear-filters {
    width: 100%;
    margin-top: 14px;
}

.import-progress-link {
    display: block;
    margin-top: 10px;
    border: 1px solid var(--progress-border);
    padding: 8px 10px;
    background: #fff;
    color: #2c3e50;
    text-align: center;
    text-decoration: none;
}

.import-progress-link:hover {
    background: #e7e7e7;
}

.search-filter {
    display: block;
    margin-bottom: 16px;
}

.search-filter input {
    width: 100%;
    padding: 9px;
    border: 1px solid var(--progress-border);
}

.table-pane {
    flex: 1;
    min-width: 0;
    min-height: 0;
    display: flex;
    flex-direction: column;
}

.progress-table {
    border-collapse: collapse;
    border-spacing: 0;
    width: 100%;
    white-space: nowrap;
    border-right: 1px solid var(--progress-border);
    box-sizing: border-box;
    line-height: 10px !important;
}

.progress-table th,
.progress-table td {
    text-align: left;
    padding: 10px;
    height: 38px !important;
    overflow-y: hidden;
}

.progress-table tr {
    height: 38px !important;
    overflow-y: hidden;
}

.progress-table th {
    background-color: #444;
    color: #f2f2f2;
    position: sticky;
    top: 0;
    z-index: 2;
}

.progress-table tr:nth-child(even) {
    background-color: #e7e7e7;
}

.mastery-state-0 {
    background-color: rgb(92, 233, 92);
}

.progress-table tr:nth-child(even) .mastery-state-0 {
    background-color: rgb(86, 216, 86);
}

.mastery-state-1 {
    background-color: rgb(238, 238, 119);
}

.progress-table tr:nth-child(even) .mastery-state-1 {
    background-color: rgb(224, 224, 111);
}

.mastery-state-2 {
    background-color: rgb(235, 130, 130);
}

.progress-table tr:nth-child(even) .mastery-state-2 {
    background-color: rgb(225, 125, 125);
}

.checkbox-item input {
    margin: 0 8px 0 0;
}

.checkbox-item {
    display: flex;
    align-items: center;
    padding: 14px 12px;
    border: 1px solid var(--progress-border);
    cursor: pointer;
    user-select: none;
    margin: 0;
    background: #fff;
}

.checkbox-grid {
    display: grid;
    gap: 4px;
}

label.checked {
    background-color: #1e69fe;
    color: #eee
}

.table-container {
    flex: 1;
    min-height: 0;
    overflow: auto;
    border-bottom: 1px solid var(--progress-border);
}


@media screen and (max-width: 600px) {

    .progress-table th,
    .progress-table td {
        font-size: 70%;
    }

    .filter-sidebar {
        position: absolute;
        inset: 0 auto 0 0;
        max-width: 85vw;
        z-index: 3;
        box-shadow: 3px 0 8px rgba(0, 0, 0, 0.2);
    }

    .sidebar-resize-handle {
        display: none;
    }
}
</style>
