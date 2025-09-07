<template>

    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left">
        </div>
        <div class="column middle">
            <ProgressTable v-if="dataReady" :_playerNames="playerNames" :_itemList="itemList"></ProgressTable>
            <h2 v-else>Loading data. This may take a few seconds if this is loading for the first time, or after an
                update.</h2>
        </div>
        <div class="column right">
        </div>
    </div>

</template>

<script>
import ProgressTable from '@/components/Progress/ProgressTable.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import { getImage } from '@/util/images';
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
            dataReady: false
        }
    },
    async mounted() {
        await this.getMasteryItems();

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
                window.location.href = "/settings";
            }

            if (!res.ok) {
                console.log(await res.text());
                return;
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
    }
}
</script>