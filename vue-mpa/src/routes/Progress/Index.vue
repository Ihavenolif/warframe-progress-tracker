<template>

    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left">
        </div>
        <div class="column middle">
            <div class="progress-page-head">
                <h1>Progress</h1>
                <RouterLink class="import-progress-link" to="/progress/import">Import progress</RouterLink>
            </div>
            <ProgressTable v-if="dataReady" :_playerNames="playerNames" :_itemList="itemList"></ProgressTable>
            <div v-else>
                <h2>Loading data. This may take a few seconds if this is loading for the first time, or after an
                    update.</h2>

                <p v-for="message in loadingMessages" v-bind:key="message">{{ message }}</p>
                <p v-if="imagesLoading">Loading images ({{ imagesLoaded }} / {{ imagesNeedLoading }})</p>
            </div>
        </div>
        <div class="column right">
        </div>
    </div>

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
.progress-page-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
}

.import-progress-link {
    border: 1px solid #555;
    padding: 9px 12px;
    color: #2c3e50;
    text-decoration: none;
}

.import-progress-link:hover {
    background: #e7e7e7;
}
</style>
