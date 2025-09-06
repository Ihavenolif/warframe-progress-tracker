<template>
    <td style="padding: 0px" width="32px"><img :src="imgSrc" alt="" height="32px"></td>
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
import { getImage } from '@/util/images';

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
    },
    data() {
        return {
            imgSrc: null
        }
    },
    methods: {
        async fetchImage() {
            let uniqueName = this.item['uniqueName'];
            let imageSrc = await getImage(uniqueName);
            this.imgSrc = imageSrc;
        },
        async init() {
            await this.fetchImage();
            if (this.$refs.progressTableCell) {
                await Promise.all(this.$refs.progressTableCell.map(cell => cell.init()));
            }
        }
    },
    /*mounted() {
        this.fetchImage();
    },*/
    watch: {
        item: {
            handler: function () {
                this.init();
            },
            deep: true
        }
    }
}

</script>
