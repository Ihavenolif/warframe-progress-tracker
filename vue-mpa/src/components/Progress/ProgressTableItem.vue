<template>
    <td style="padding: 0px" width="32px"><img :src="imgSrc" alt="" height="32px"></td>
    <td>
        <span class="row-text">{{ item["itemName"] }}</span>
    </td>
    <td><span class="row-text">{{ item["itemClass"] }}</span></td>
    <ProgressTableCell v-bind:xp-gained="item['xpGained']" v-bind:xp-required="item['xpRequired']">
    </ProgressTableCell>
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
            console.log(uniqueName, imageSrc)
        }
    },
    /*mounted() {
        this.fetchImage();
    },*/
    watch: {
        item: {
            handler: function () {
                this.fetchImage();
            },
            deep: true
        }
    }
}

</script>
