<template>
    <div class="recommendation-results">
        <p class="recommendation-result-count">{{ recommendations.length }} ranked relics</p>
        <RelicRecommendationCard v-for="relic in recommendations" :key="relic.relicId"
            :relic="relic" :expanded="expandedRelicId === relic.relicId"
            @toggle="toggle(relic.relicId)" />
    </div>
</template>

<script>
import RelicRecommendationCard from './RelicRecommendationCard.vue';

export default {
    name: 'RelicRecommendationResults',
    components: { RelicRecommendationCard },
    props: {
        recommendations: { type: Array, required: true }
    },
    data() {
        return { expandedRelicId: null };
    },
    watch: {
        recommendations() {
            this.expandedRelicId = null;
        }
    },
    methods: {
        toggle(relicId) {
            this.expandedRelicId = this.expandedRelicId === relicId ? null : relicId;
        }
    }
};
</script>
