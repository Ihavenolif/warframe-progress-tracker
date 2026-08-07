<template>

    <NavbarElement></NavbarElement>

    <main class="progress-page">
        <ProgressTable v-if="dataReady" :_playerNames="playerNames" :_itemList="itemList"></ProgressTable>
        <div v-else class="progress-loading">
            <h2>Loading data. This may take a few seconds if this is loading for the first time, or after an
                update.</h2>

            <p v-for="message in loadingMessages" v-bind:key="message">{{ message }}</p>
            <p v-if="imagesLoading">Loading images ({{ imagesLoaded }} / {{ imagesNeedLoading }})</p>
        </div>
    </main>

</template>

<script>
import ProgressTable from '@/components/Progress/ProgressTable.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import {
    getImage
} from '@/util/images';

import { authFetch } from '@/util/util';

export default {
    name: "ProgressPage",
    components: {
        ProgressTable,
        NavbarElement
    },
    data() {
        return {
            playerNames: [],
            itemList: [],
            data: {},
            dataReady: false,
            loadingMessages: ["Fetching mastery data..."],
            imagesNeedLoading: 0,
            imagesLoaded: 0,
            imagesLoading: false
        }
    },
    async mounted() {
        await this.getMasteryItems();
        if (!this.data.items) return;

        this.itemList = this.data.items;
        this.playerNames = this.data.playerNames;

        await this.fetchAllImages();
        this.dataReady = true;
    },
    methods: {
        async getMasteryItems() {
            const res = await authFetch(`/api/mastery/me`, {
                method: "GET"
            })

            if (res.status == 404) {
                this.$router.push({ name: 'settings' });
                return;
            }

            this.loadingMessages.push("Done fetching mastery data.");

            if (!res.ok) {
                console.log(await res.text());
                return;
            }

            this.data = await res.json()

        },
        async fetchAllImages() {
            this.loadingMessages.push("Loading images...");
            this.imagesLoading = true;
            await Promise.all(this.itemList.map(item => this.loadItem(item)));
            this.loadingMessages.push("Done loading images.");

        },
        async fetchImage(uniqueName) {
            this.imagesNeedLoading++;
            let imageSrc = await getImage(uniqueName);
            this.imagesLoaded++;
            return imageSrc;
        },
        async loadItem(item) {
            item.imgSrc = await this.fetchImage(item.uniqueName);
            if (item.recipeUniqueName) {
                item.bpImageSrc = await this.fetchImage(item.recipeUniqueName);
            }
            for (let player of this.playerNames) {
                if (item[player] && item[player].components) {
                    for (let component of item[player].components) {
                        if (!component.uniqueName) continue;
                        component.imgSrc = await this.fetchImage(component.uniqueName);
                    }
                }
            }
        }
    }
}
</script>

<style scoped>
.progress-page {
    height: calc(100vh - 49px);
    display: flex;
    flex-direction: column;
    overflow: hidden;
    padding: 0;
}

.progress-loading {
    overflow-y: auto;
    padding: 0 20px;
}
</style>
