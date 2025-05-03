<template>
    <td class="mastery-state-2" v-if="xpGained == null">Unowned</td>
    <td class="mastery-state-1" v-else-if="xpGained < xpRequired">{{ xpGained }}/{{ xpRequired }}</td>
    <td class="mastery-state-0" v-else>Mastered</td>
    
</template>

<script>
export default{
    name: "ProgressTableCell",
    props:{
        xpGained:{
            required: true
        },
        xpRequired:{
            required: true
        }
    }
}
</script>