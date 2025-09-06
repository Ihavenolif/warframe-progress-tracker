<template>
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

    <br><br>

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
                <!--<th id="masteredHead" v-on:click="sortTable('mastery')">Mastered <i
                        v-if="this.sorting.key === 'mastery'"><span
                            :class="['fa', 'table-head-caret', this.sorting.asc ? 'fa-caret-down' : 'fa-caret-up']"></span></i>
                </th>-->
            </tr>
        </thead>

        <tbody id="tableBody">
            <tr v-for="item in itemList" :key="item.uniqueName" :style="{
                display: (filterItem(item)) ? '' : 'none'
            }">
                <ProgressTableItem v-bind:item="item" v-bind:playerNames="playerNames" ref="progressTableItem">
                </ProgressTableItem>
            </tr>
        </tbody>
    </table>
</template>

<script>
import CollapsibleContainer from '../Collapsible.vue';
import ProgressTableItem from './ProgressTableItem.vue';
import { authFetch } from '@/util/util';

export default {
    name: "ProgressTable",
    computed: {
        username() {
            return this.$store.state.username;
        },
        token() {
            return this.$store.state.token;
        }
    },
    components: {
        ProgressTableItem,
        CollapsibleContainer
    },
    data() {
        return {
            playerNames: [],
            itemList: [],
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
        async getMasteryItems() {
            const res = await authFetch(`/api/mastery/me`, {
                method: "GET"
            })

            if (res.status == 404) {
                window.location.href = "/settings";
            }

            if (!res.ok) {
                console.log(await res.text());
                return;
            }

            const data = await res.json()
            this.itemList = data.items;
            this.playerNames = data.playerNames;

        },
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

                    const masteredRateA = a[this.sorting.key]["xpGained"] / a["xpRequired"]
                    const masteredRateB = b[this.sorting.key]["xpGained"] / b["xpRequired"]

                    if (masteredRateA == masteredRateB) return 0;

                    return (masteredRateB < masteredRateA ? -1 : 1) * (this.sorting.asc ? 1 : -1);
                })
            }
            else {
                this.itemList.sort((a, b) => {
                    return (a[this.sorting.key] < b[this.sorting.key] ? -1 : 1) * (this.sorting.asc ? 1 : -1);
                })
            }
        },
        fetchAllImages() {
            console.log(this.$refs.progressTableItem.length);
            this.$refs.progressTableItem.forEach((child) => {
                child.init();
            });
        },
        filterItem(item) {
            const validClass = this.selectedItemClasses.length === 0 || this.selectedItemClasses.includes(item.itemClass);
            const validName = item.itemName.toLowerCase().includes(this.itemNameFilter.toLowerCase());
            return validClass && validName;
        }

    },
    async mounted() {
        await this.getMasteryItems();
        this.sortTable("itemName");
        this.sortTable("itemClass");
        this.playerNames.forEach(name => {
            this.sortTable(name);
        });
        this.fetchAllImages();
    }
}
</script>

<style>
html {
    overflow-y: scroll;
    /* Always show vertical scrollbar */
}

body {
    margin: 0;
    height: 100vh;
    /* Optional: ensure body takes full height */
}

table {
    border-collapse: collapse;
    border-spacing: 0;
    width: 100%;
    border: 1px solid #ddd;
}

th,
td {
    text-align: left;
    padding: 10px;
}

th {
    background-color: #444;
    color: #f2f2f2;
}

tr:nth-child(even) {
    /*background-color: #fefefe;
    filter: brightness(0.95);*/
}

tr {
    border-bottom: 1px solid #ddd;
}

tr:hover {
    background-color: #fefefe;
    filter: brightness(0.9);
}

.mastery-state-0 {
    background-color: rgb(92, 233, 92);
}

.mastery-state-1 {
    background-color: rgb(238, 238, 119);
}

.mastery-state-2 {
    background-color: rgb(235, 130, 130);
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

@media screen and (max-width: 600px) {

    th,
    td {
        font-size: 70%;
    }
}
</style>