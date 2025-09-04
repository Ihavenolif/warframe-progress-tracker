<template>
    <div style="display: flex;">
        <div v-for="(component, index) in sortedItems" :key="index" :style="{
            display: 'flex',
            flex: 1,
            justifyContent: 'center',
            alignItems: 'center',
            height: '38px',
            backgroundColor: component['countOwned'] >= component['countRequired'] ? 'rgb(92, 233, 92)' :
                component['recipeOwned'] ? 'rgb(238, 238, 119)' : 'rgb(235, 130, 130)'
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
        }
    },
    computed: {
        sortedItems() {
            if (!this.item) return [];
            if (!this.item['components']) return [];
            if (this.item['components'].length === 0) return [];
            return [...this.item['components']].sort((a, b) => a['uniqueName'].localeCompare(b['uniqueName']));
        }
    },
    components: {
        ProgressTableComponentItem
    },
    methods: {
        async init() {

            if (this.$refs.componentItem && this.$refs.componentItem.length) {
                this.$refs.componentItem.forEach((child) => {
                    child.init();
                });
            }
        }
    }
}
</script>