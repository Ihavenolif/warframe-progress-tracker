<template>
    <img :src="imgSrc ?? 'https://warframedrops.com/img/clem-in-action-glyph.png.webp'" :alt="item['name']"
        height="34px">
</template>

<script>
import { getImage } from '@/util/images';

export default {
    name: "ProgressTableComponentItem",
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
        },
        async init() {
            await this.fetchImage();
        }
    }
}
</script>