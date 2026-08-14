<template>
    <div class="relic-details">
        <section>
            <h3>Refinements</h3>
            <div class="refinement-grid">
                <div v-for="refinement in refinements" :key="refinement" class="refinement-tile">
                    <span>{{ refinement }}</span>
                    <strong>{{ refinementCount(refinement) }}</strong>
                </div>
            </div>
            <p v-if="hasAliases" class="alias-note">Counts include internal aliases for the same refinement.</p>
        </section>
        <section>
            <h3>Rewards</h3>
            <div class="reward-list">
                <div v-for="reward in relic.rewards" :key="reward.uniqueName" class="reward-row">
                    <span class="rarity" :class="reward.rarity.toLowerCase()">{{ reward.rarity }}</span>
                    <span>{{ reward.itemName || reward.uniqueName }}</span>
                    <span v-if="reward.itemCount > 1">×{{ reward.itemCount }}</span>
                </div>
            </div>
        </section>
    </div>
</template>

<script>
export default {
    name: 'RelicDetails',
    props: {
        relic: { type: Object, required: true },
        refinements: { type: Array, required: true }
    },
    computed: {
        hasAliases() {
            return this.refinements.some(refinement =>
                this.relic.variants.filter(variant => variant.refinement === refinement).length > 1);
        }
    },
    methods: {
        refinementCount(refinement) {
            return this.relic.variants.filter(variant => variant.refinement === refinement)
                .reduce((total, variant) => total + variant.quantity, 0);
        }
    }
};
</script>

<style scoped>
.relic-details { display: grid; grid-template-columns: minmax(280px, .9fr) minmax(320px, 1.1fr); gap: 30px; padding: 4px 24px 24px 130px; border-top: 1px solid #e5eaed; }
.relic-details h3 { font-size: .8rem; letter-spacing: .1em; text-transform: uppercase; }
.refinement-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; }
.refinement-tile { display: grid; gap: 5px; padding: 12px 8px; background: #eef2f4; text-align: center; font-size: .7rem; }
.refinement-tile strong { font-size: 1.25rem; }
.alias-note { color: #607080; font-size: .8rem; }
.reward-list { display: grid; gap: 6px; }
.reward-row { display: grid; grid-template-columns: 76px 1fr auto; gap: 10px; align-items: center; padding: 8px 10px; background: #f5f7f8; font-size: .9rem; }
.rarity { padding: 3px 6px; color: white; font-size: .68rem; font-weight: 800; text-align: center; text-transform: uppercase; }
.rarity.common { background: #8b765f; }
.rarity.uncommon { background: #8b9399; }
.rarity.rare { background: #b58a20; }

@media (max-width: 800px) {
    .relic-details { grid-template-columns: 1fr; padding-left: 24px; }
}
@media (max-width: 560px) {
    .refinement-grid { grid-template-columns: 1fr 1fr; }
    .relic-details { padding: 4px 14px 18px; }
    .reward-row { grid-template-columns: 68px 1fr; }
}
</style>
