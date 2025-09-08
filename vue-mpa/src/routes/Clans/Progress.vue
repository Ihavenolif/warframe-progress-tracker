<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <ProgressTable v-if="dataReady" :_playerNames="playerNames" :_itemList="itemList"></ProgressTable>
        <h2 v-else>Loading data. This may take a few seconds if this is loading for the first time, or after an
            update.</h2>
    </ThreeColumnLayout>




</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import ProgressTable from '@/components/Progress/ProgressTable.vue';
import { authFetch } from '@/util/util';
import { getImage } from '@/util/images';
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
            dataReady: false
        }
    },
    methods: {
        async getMasteryItems() {
            const res = await authFetch(`/api/clans/${this.clanName}/progress`, {
                method: "GET"
            })

            if (!res.ok) {
                //window.location.href = "/clans";
            }

            this.data = await res.json()
        },
        async fetchAllImages() {
            await Promise.all(this.itemList.map(item => this.loadItem(item)));
        },
        async fetchImage(uniqueName) {
            let imageSrc = await getImage(uniqueName);
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
    },
    async mounted() {
        await this.getMasteryItems();

        this.itemList = this.data.items;
        this.playerNames = this.data.playerNames;

        await this.fetchAllImages();
        this.dataReady = true;
    }
}
</script>