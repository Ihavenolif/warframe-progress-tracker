<template>

    <div class="flex-container">
        <CollapsibleContainer title="Filters">

            <div style="display: flex;">
                <div style="flex: 2; padding: 10px;">
                    <CollapsibleContainer title="Item Classes">
                        <div class="checkbox-grid">
                            <label v-for="itemClass in allItemClasses" :key="itemClass"
                                :class="['checkbox-item', selectedItemClasses.includes(itemClass) ? 'checked' : '']">
                                <input type="checkbox" :value="itemClass" v-model="selectedItemClasses"
                                    style="display: none;" />
                                <span>{{ itemClass }}</span>
                            </label>
                        </div>
                    </CollapsibleContainer>
                </div>

                <div style="flex: 1; padding: 10px;">
                    <input id="itemNameFilter" type="text" v-model="itemNameFilter" v-on:input="fetchAndFilterItems"
                        placeholder="Search">
                </div>
            </div>


        </CollapsibleContainer>

        <br>
        <br>

        <div class="table-container">
            <table>
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
        }
    },
    data() {
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
            itemNameFilter: ""
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
        }

    },
    async mounted() {
        this.sortTable("itemName");
        this.sortTable("itemClass");
        this.playerNames.forEach(name => {
            this.sortTable(name);
        });
    }
}
</script>

<style>
table {
    border-collapse: collapse;
    border-spacing: 0;
    width: 100%;
    white-space: nowrap;
    border-right: 1px solid #777;
    box-sizing: border-box;
    line-height: 10px !important;
}

body {
    overflow: hidden;
}

th,
td {
    text-align: left;
    padding: 10px;
    height: 38px !important;
    overflow-y: hidden;
}

tr {
    height: 38px !important;
    overflow-y: hidden;
}

th {
    background-color: #444;
    color: #f2f2f2;
}

tr:nth-child(even) {
    background-color: #e7e7e7;
}

.mastery-state-0 {
    background-color: rgb(92, 233, 92);
}

tr:nth-child(even) .mastery-state-0 {
    background-color: rgb(86, 216, 86);
}

.mastery-state-1 {
    background-color: rgb(238, 238, 119);
}

tr:nth-child(even) .mastery-state-1 {
    background-color: rgb(224, 224, 111);
}

.mastery-state-2 {
    background-color: rgb(235, 130, 130);
}

tr:nth-child(even) .mastery-state-2 {
    background-color: rgb(225, 125, 125);
}

.checkbox-item input {
    margin-right: 4px;
}

.checkbox-item {
    display: flex;
    align-items: center;
    padding: 14px 12px;
    border: 1px solid #ccc;
    border-radius: 3px;
    cursor: pointer;
    user-select: none;
    margin: 0
}

.checkbox-grid {
    display: flex;
    flex-wrap: wrap;
    gap: 3px;
    /* spacing between items */
}

label.checked {
    background-color: #1e69fe;
    color: #eee
}

.table-container {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: auto;
    border-top: 1px solid #777;
    border-bottom: 1px solid #777;
    border-left: 1px solid #777;
}


@media screen and (max-width: 600px) {

    th,
    td {
        font-size: 70%;
    }
}

.flex-container {
    height: calc(100vh - 20px - 48px);
    margin: 0;
    display: flex;
    flex-direction: column;
}
</style>