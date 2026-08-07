<template>
    <NavbarElement></NavbarElement>

    <main class="clan-progress-page">
        <ProgressTable v-if="dataReady" :_playerNames="playerNames" :_itemList="itemList" :showImport="false"></ProgressTable>
        <div v-else class="clan-progress-loading">
            <h2>Loading data. This may take a few seconds if this is loading for the first time, or after an
                update.</h2>

            <p v-for="message in loadingMessages" v-bind:key="message">{{ message }}</p>
            <p v-if="imagesLoading">Loading images ({{ imagesLoaded }} / {{ imagesNeedLoading }})</p>
        </div>

    </main>




</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import ProgressTable from '@/components/Progress/ProgressTable.vue';
import { authFetch } from '@/util/util';
import {
    getImage
} from '@/util/images';
import { ManifestFetchStartedSignal, ManifestFetchFinishedSignal, ManifestLoadStartedSignal, ManifestLoadFinishedSignal, ManifestParseStartedSignal, ManifestParseFinishedSignal, subscribe } from '@/util/signals';

export default {
    name: "ClansProgress",
    props: {
        clanName: {
            type: String,
            required: true
        }
    },
    components: {
        NavbarElement,
        ProgressTable
    },
    computed: {
        encodedClanName() {
            return encodeURIComponent(this.clanName);
        }
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
            imagesLoading: false,
            unsubscribers: []
        }
    },
    methods: {
        async getMasteryItems() {
            const res = await authFetch(`/api/clans/${this.encodedClanName}/progress`, {
                method: "GET"
            })

            if (!res.ok) {
                this.$router.push({ name: 'clans' });
                return;
            }

            this.loadingMessages.push("Done fetching mastery data.");

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
        },
        addLoadingEventListeners() {
            this.unsubscribers.push(subscribe(ManifestLoadStartedSignal, () => {
                this.loadingMessages.push("Loading manifest from Warframe servers...");
            }));
            this.unsubscribers.push(subscribe(ManifestLoadFinishedSignal, () => {
                this.loadingMessages.push("Done loading manifest from Warframe servers.");
            }));
            this.unsubscribers.push(subscribe(ManifestParseStartedSignal, () => {
                this.loadingMessages.push("Parsing manifest...");
            }));
            this.unsubscribers.push(subscribe(ManifestParseFinishedSignal, () => {
                this.loadingMessages.push("Done parsing manifest.");
            }));
            this.unsubscribers.push(subscribe(ManifestFetchStartedSignal, () => {
                this.loadingMessages.push("Fetching manifest...");
            }));
            this.unsubscribers.push(subscribe(ManifestFetchFinishedSignal, () => {
                this.loadingMessages.push("Done fetching manifest.");
            }));
        },
        async loadData() {
            this.playerNames = [];
            this.itemList = [];
            this.data = {};
            this.dataReady = false;
            this.loadingMessages = ["Fetching mastery data..."];
            this.imagesNeedLoading = 0;
            this.imagesLoaded = 0;
            this.imagesLoading = false;

            await this.getMasteryItems();
            if (!this.data.items) return;

            this.itemList = this.data.items;
            this.playerNames = this.data.playerNames;

            await this.fetchAllImages();
            this.dataReady = true;
        }
    },
    async mounted() {
        this.addLoadingEventListeners();
        await this.loadData();
    },
    beforeUnmount() {
        this.unsubscribers.forEach(unsubscribe => unsubscribe());
    }
}
</script>

<style scoped>
.clan-progress-page {
    height: calc(100vh - 49px);
    display: flex;
    flex-direction: column;
    overflow: hidden;
}

.clan-progress-loading {
    overflow-y: auto;
    padding: 0 20px;
}
</style>
