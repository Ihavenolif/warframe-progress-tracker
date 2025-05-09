<template>
    <td class="mastery-state-2" v-if="xpGained == null">Unowned</td>
    <td class="mastery-state-1" v-else-if="xpGained < xpRequired" style="padding: 0px"><ProgressBar :progress="(xpGained/xpRequired) * 100"> </ProgressBar></td>
    <td class="mastery-state-0" v-else>Mastered</td>
    
</template>

<script>
import ProgressBar from '../ProgressBar.vue';

export default{
    name: "ProgressTableCell",
    props:{
        xpGained:{
            required: true
        },
        xpRequired:{
            required: true
        }
    },
    components: {
        ProgressBar
    }
}
</script>