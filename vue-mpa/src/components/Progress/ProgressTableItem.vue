<template>
    <td style="padding: 0px" width="32px"><img :src="item['imgSrc']" alt="" height="34px"></td>
    <td>
        <span class="row-text">{{ item["itemName"] }}</span>
    </td>
    <td><span class="row-text">{{ item["itemClass"] }}</span>
    </td>
    <ProgressTableCell v-for="(playerName, index) in playerNames" :key="index" ref="progressTableCell"
        v-bind:item="item" v-bind:playerName="playerName"></ProgressTableCell>
</template>

<script>
import ProgressTableCell from './ProgressTableCell.vue';

export default {
    name: "ProgressTableItem",
    components: {
        ProgressTableCell
    },
    props: {
        item: {
            required: true
        },
        playerNames: {
            required: true
        }
    }
}

</script>
