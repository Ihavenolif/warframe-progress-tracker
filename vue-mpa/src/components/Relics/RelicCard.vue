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

<style scoped>
.relic-card { --era-color: #777; border: 1px solid #d5dde1; border-left: 5px solid var(--era-color); margin-bottom: 10px; background: white; box-shadow: 0 2px 8px rgba(38, 55, 70, .06); }
.era-lith { --era-color: #9d7454; }
.era-meso { --era-color: #4b89a7; }
.era-neo { --era-color: #789047; }
.era-axi { --era-color: #9a5f9e; }
.relic-summary { display: grid; grid-template-columns: 90px 1fr auto 28px; gap: 16px; align-items: center; width: 100%; padding: 18px; border: 0; background: transparent; color: inherit; text-align: left; cursor: pointer; }
.era-mark { color: var(--era-color); font-size: .75rem; font-weight: 900; letter-spacing: .1em; text-transform: uppercase; }
.relic-name { font-size: 1.05rem; font-weight: 800; }
.owned-total { color: #607080; }
.owned-total strong { color: #263746; font-size: 1.15rem; }
.expand-mark { font-size: 1.5rem; color: var(--era-color); }

@media (max-width: 560px) {
    .relic-summary { grid-template-columns: 60px 1fr 24px; gap: 8px; padding: 14px 12px; }
    .owned-total { grid-column: 2; grid-row: 2; }
    .expand-mark { grid-column: 3; grid-row: 1 / span 2; }
}
</style>
