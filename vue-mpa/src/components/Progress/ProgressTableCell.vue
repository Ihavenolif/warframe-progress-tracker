<template>
    <td class="mastery-state-2" v-if="item['xpGained'] == null">
        <ProgressTableUnowned ref="progressTableUnowned" v-bind:item="item"></ProgressTableUnowned>
    </td>
    <td class="mastery-state-1" v-else-if="item['xpGained'] < item['xpRequired']" style="padding: 0px">
        <ProgressBar :progress="(item['xpGained'] / item['xpRequired']) * 100"> </ProgressBar>
    </td>
    <td class="mastery-state-0" v-else>Mastered</td>

</template>

<script>
import ProgressBar from '../ProgressBar.vue';
import ProgressTableUnowned from './ProgressTableUnowned.vue';

export default {
    name: "ProgressTableCell",
    props: {
        item: {
            required: true
        }
    },
    components: {
        ProgressBar,
        ProgressTableUnowned
    },
    methods: {
        async init() {
            if (this.$refs.progressTableUnowned) {
                await this.$refs.progressTableUnowned.init();
            }
        }
    }
}
</script>

<style>
td.mastery-state-2 {
    padding: 0px !important;
}
</style>