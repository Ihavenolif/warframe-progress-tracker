<template>
    <table>
        <thead>
            <tr>
                <th id="itemNameHead" v-on:click="sortTable('itemName')">Item name <i
                        v-if="this.sorting.key === 'itemName'"><span
                            :class="['fa', 'table-head-caret', this.sorting.asc ? 'fa-caret-down' : 'fa-caret-up']"></span></i>
                </th>
                <th id="classHead" v-on:click="sortTable('itemClass')">Item Class <i
                        v-if="this.sorting.key === 'itemClass'"><span
                            :class="['fa', 'table-head-caret', this.sorting.asc ? 'fa-caret-down' : 'fa-caret-up']"></span></i>
                </th>
                <th id="masteredHead" v-on:click="sortTable('mastery')">Mastered <i
                        v-if="this.sorting.key === 'mastery'"><span
                            :class="['fa', 'table-head-caret', this.sorting.asc ? 'fa-caret-down' : 'fa-caret-up']"></span></i>
                </th>
            </tr>
        </thead>

        <tbody id="tableBody">
            <tr v-for="(item, index) in itemList" :key="index">
                <td>{{ item["itemName"] }}</td>
                <td>{{ item["itemClass"] }}</td>
                <ProgressTableCell v-bind:xp-gained="item['xpGained']" v-bind:xp-required="item['xpRequired']">
                </ProgressTableCell>
            </tr>
        </tbody>
    </table>
</template>

<script>
import ProgressTableCell from './ProgressTableCell.vue';
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
        ProgressTableCell
    },
    data() {
        return {
            itemList: [],
            sorting: { key: "", asc: true }
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
            this.itemList = data;
        },
        sortTable(sortKey) {
            if (this.sorting.key == sortKey) this.sorting.asc = !this.sorting.asc;
            else {
                this.sorting.key = sortKey;
                this.sorting.asc = true;
            }

            if (this.sorting.key == "mastery") {
                this.itemList.sort((a, b) => {
                    if (a["xpGained"] >= a["xpRequired"] && b["xpGained"] >= b["xpRequired"]) return 0;

                    const masteredRateA = a["xpGained"] / a["xpRequired"]
                    const masteredRateB = b["xpGained"] / b["xpRequired"]
                    return (masteredRateB < masteredRateA ? -1 : 1) * (this.sorting.asc ? 1 : -1);
                })
            }
            else {
                this.itemList.sort((a, b) => {
                    return (a[this.sorting.key] < b[this.sorting.key] ? -1 : 1) * (this.sorting.asc ? 1 : -1);
                })
            }

        }
    },
    async mounted() {
        await this.getMasteryItems();
        this.sortTable("itemName");
        this.sortTable("itemClass");
        this.sortTable("mastery");
    }
}
</script>

<style>
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
    background-color: #fefefe;
    filter: brightness(0.95);
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


@media screen and (max-width: 600px) {

    th,
    td {
        font-size: 70%;
    }
}
</style>