<template>
    <div style="display: flex;">
        <div v-if="item.recipeUniqueName !== null" :style="{
            display: 'flex',
            flex: 1,
            justifyContent: 'center',
            alignItems: 'center',
            height: '38px'
        }" :class="[
            item[this.playerName]['blueprintOwned'] ? 'mastery-state-0' : 'mastery-state-2'
        ]">
            <ProgressTableComponentItem ref="componentItemBp" v-bind:item="blueprintItem"></ProgressTableComponentItem>
        </div>
        <div v-for="(component, index) in sortedComponents" :key="index" :style="{
            display: 'flex',
            flex: 1,
            justifyContent: 'center',
            alignItems: 'center',
            height: '38px'
        }" :class="[
            component['countOwned'] >= component['countRequired'] ? 'mastery-state-0' :
                component['blueprintOwned'] ? 'mastery-state-1' : 'mastery-state-2'
        ]">

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
                uniqueName: this.item.recipeUniqueName,
                imgSrc: this.item.bpImageSrc,
            };
        }
    },
    components: {
        ProgressTableComponentItem
    }
}
</script>