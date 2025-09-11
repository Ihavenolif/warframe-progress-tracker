<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <ProgressTable v-if="dataReady" :_playerNames="playerNames" :_itemList="itemList"></ProgressTable>
        <div v-else>
            <h2>Loading data. This may take a few seconds if this is loading for the first time, or after an
                update.</h2>

            <p v-for="message in loadingMessages" v-bind:key="message">{{ message }}</p>
            <p v-if="imagesLoading">Loading images ({{ imagesLoaded }} / {{ imagesNeedLoading }})</p>
        </div>

    </ThreeColumnLayout>




</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import ProgressTable from '@/components/Progress/ProgressTable.vue';
import { authFetch } from '@/util/util';
import {
    getImage,
    subscribe,
    ManifestLoadStartedSignal,
    ManifestLoadFinishedSignal,
    ManifestParseStartedSignal,
    ManifestParseFinishedSignal,
    ManifestFetchStartedSignal,
    ManifestFetchFinishedSignal
} from '@/util/images';
import ThreeColumnLayout from '@/components/ThreeColumnLayout.vue';

export default {
    name: "ClansProgress",
    components: {
        NavbarElement,
        ProgressTable,
        ThreeColumnLayout
    },
    computed: {
        clanName() {
            const pathParts = window.location.href.split('/');
            return decodeURIComponent(pathParts[pathParts.length - 2]);
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
            imagesLoading: false
        }
    },
    methods: {
        async getMasteryItems() {
            const res = await authFetch(`/api/clans/${this.clanName}/progress`, {
                method: "GET"
            })

            if (!res.ok) {
                window.location.href = "/clans";
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
            subscribe(ManifestLoadStartedSignal, () => {
                this.loadingMessages.push("Loading manifest from Warframe servers...");
            });
            subscribe(ManifestLoadFinishedSignal, () => {
                this.loadingMessages.push("Done loading manifest from Warframe servers.");
            });
            subscribe(ManifestParseStartedSignal, () => {
                this.loadingMessages.push("Parsing manifest...");
            });
            subscribe(ManifestParseFinishedSignal, () => {
                this.loadingMessages.push("Done parsing manifest.");
            });
            subscribe(ManifestFetchStartedSignal, () => {
                this.loadingMessages.push("Fetching manifest...");
            });
            subscribe(ManifestFetchFinishedSignal, () => {
                this.loadingMessages.push("Done fetching manifest.");
            });
        }
    },
    async mounted() {
        this.addLoadingEventListeners();
        await this.getMasteryItems();

        this.itemList = this.data.items;
        this.playerNames = this.data.playerNames;

        await this.fetchAllImages();
        this.dataReady = true;
    }
}
</script>