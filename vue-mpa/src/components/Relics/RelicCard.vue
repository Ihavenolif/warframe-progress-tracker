<template>
    <article class="relic-card" :class="`era-${relic.era.toLowerCase()}`">
        <button class="relic-summary" type="button" :aria-expanded="expanded" @click="$emit('toggle')">
            <span class="era-mark">{{ relic.era }}</span>
            <span class="relic-name">{{ relic.name }}</span>
            <span class="owned-total"><strong>{{ relic.totalOwned }}</strong> owned</span>
            <span class="expand-mark" aria-hidden="true">{{ expanded ? '−' : '+' }}</span>
        </button>
        <RelicDetails v-if="expanded" :relic="relic" :refinements="refinements" />
    </article>
</template>

<script>
import RelicDetails from './RelicDetails.vue';

export default {
    name: 'RelicCard',
    components: { RelicDetails },
    props: {
        relic: { type: Object, required: true },
        refinements: { type: Array, required: true },
        expanded: { type: Boolean, required: true }
    },
    emits: ['toggle']
};
</script>
