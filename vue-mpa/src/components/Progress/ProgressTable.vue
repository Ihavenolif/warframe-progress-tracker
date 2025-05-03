<template>
    <table>
        <thead>
            <tr>
                <th id="itemNameHead" onclick="sortBy('name')">Item name <i class="fa fa-caret-down table-head-caret"
                        style="display: none;" id="name-caret"></i></th>
                <th id="classHead" onclick="sortBy('class')">Item Class <i class="fa fa-caret-down table-head-caret"
                        style="display: none;" id="class-caret"></i></th>
                <th id="masteredHead" onclick="sortBy('state')">Mastered <i class="fa fa-caret-down table-head-caret"
                        style="display: inline-block;" id="state-caret"></i></th>
            </tr>
        </thead>

        <tbody id="tableBody">
            <tr v-for="(item, index) in itemList" :key="index">
                <td>{{ item["itemName"] }}</td>
                <td>{{ item["itemClass"] }}</td>
                <ProgressTableCell v-bind:xp-gained="item['xpGained']" v-bind:xp-required="item['xpRequired']"></ProgressTableCell>
            </tr>
        </tbody>
    </table>
</template>

<script>
import ProgressTableCell from './ProgressTableCell.vue';
import {BASE_URL, getPlayerName} from '@/util/util';

export default{
    name: "ProgressTable",
    computed: {
        username() {
            return this.$store.state.username;
        },
        token(){
            return this.$store.state.token;
        }
    },
    components: {
        ProgressTableCell
    },
    data() {
        return{
            itemList:[]
        }
    },
    methods:{
        async getMasteryItems(){
            if(!this.username) window.location.href = "login";

            const playerName = await getPlayerName();
            console.log(playerName);

            const res = await fetch(`${BASE_URL}/api/mastery/${playerName}`,{
                headers:{
                    'Authorization': `Bearer ${this.token}`
                },
                method: "GET"
            })

            if(!res.ok) {
                console.log(await res.text());
                return;
            }

            const data = await res.json()
            this.itemList = data;
        }
    },
    mounted(){
        this.getMasteryItems();
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
    background-color: #f2f2f2;

    .mastery-state-0 {
        background-color: rgb(92, 233, 92);
    }

    .mastery-state-1 {
        background-color: rgb(238, 238, 119);
    }

    .mastery-state-2 {
        background-color: rgb(235, 130, 130);
    }
}

tr:nth-child(odd) {

    .mastery-state-0 {
        background-color: lightgreen;
    }

    .mastery-state-1 {
        background-color: rgb(242, 242, 178);
    }

    .mastery-state-2 {
        background-color: rgb(239, 164, 164);
    }
}


@media screen and (max-width: 600px) {

    th,
    td {
        font-size: 70%;
    }
}
</style>