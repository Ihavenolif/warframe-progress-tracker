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
