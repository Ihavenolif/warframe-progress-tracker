<template>
    <div style="display: flex;">
        <div v-if="item.recipeUniqueName !== null" :style="{
            display: 'flex',
            flex: 1,
            justifyContent: 'center',
            alignItems: 'center',
            height: '38px',
            backgroundColor: item[this.playerName].blueprintOwned ? 'rgb(92, 233, 92)' : 'rgb(235, 130, 130)'
        }">
            <ProgressTableComponentItem ref="componentItemBp" v-bind:item="blueprintItem"></ProgressTableComponentItem>
        </div>
        <div v-for="(component, index) in sortedComponents" :key="index" :style="{
            display: 'flex',
            flex: 1,
            justifyContent: 'center',
            alignItems: 'center',
            height: '38px',
            backgroundColor: component['countOwned'] >= component['countRequired'] ? 'rgb(92, 233, 92)' :
                component['blueprintOwned'] ? 'rgb(238, 238, 119)' : 'rgb(235, 130, 130)'
        }">

            <ProgressTableComponentItem ref="componentItem" v-bind:item="component"></ProgressTableComponentItem>


        </div>
    </div>

</template>

<script>
import ProgressTableComponentItem from './ProgressTableComponentItem.vue';

export default {
    name: "ProgressTableUnowned",
    props: {
        item: {
            required: true
        },
        playerName: {
            required: true
        }
    },
    computed: {
        sortedComponents() {
            if (!this.item[this.playerName]) return [];
            if (!this.item[this.playerName]['components']) return [];
            if (this.item[this.playerName]['components'].length === 0) return [];
            return [...this.item[this.playerName]['components']].sort((a, b) => a['uniqueName'].localeCompare(b['uniqueName']));
        },
        blueprintItem() {
            return {
                name: this.item.recipeName,
                uniqueName: this.item.recipeUniqueName
            };
        }
    },
    components: {
        ProgressTableComponentItem
    },
    methods: {
        async init() {
            if (this.$refs.componentItemBp) {
                this.$refs.componentItemBp.init();
            }
            if (this.$refs.componentItem && this.$refs.componentItem.length) {
                this.$refs.componentItem.map(async (child) => {
                    child.init();
                });
            }
        }
    }
}
</script>