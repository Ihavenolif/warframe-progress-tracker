<template>
    <article class="recommendation-card" :class="`era-${relic.era.toLowerCase()}`">
        <button class="recommendation-summary" type="button" :aria-expanded="expanded" @click="$emit('toggle')">
            <span class="recommendation-rank">{{ relic.score }}</span>
            <span class="recommendation-title">
                <small>{{ relic.era }}</small>
                <strong>{{ relic.name }}</strong>
            </span>
            <span class="recommendation-benefit">Helps {{ relic.benefitingPlayerCount }} {{ playerWord }}</span>
            <span class="recommendation-owners">{{ ownerSummary }}</span>
            <span class="recommendation-expand" aria-hidden="true">{{ expanded ? '−' : '+' }}</span>
        </button>
        <div v-if="expanded" class="recommendation-details">
            <section>
                <h3>Available copies</h3>
                <div class="recommendation-owner-list">
                    <div v-for="owner in relic.owners" :key="owner.playerId" class="recommendation-owner-row">
                        <strong>{{ owner.playerName }} · {{ owner.totalCount }}</strong>
                        <span v-for="refinement in refinements" :key="refinement.key">
                            {{ refinement.label }} {{ owner.refinements[refinement.key] }}
                        </span>
                    </div>
                </div>
            </section>
            <section>
                <h3>Useful rewards</h3>
                <div class="recommendation-reward-list">
                    <div v-for="reward in relic.usefulRewards" :key="reward.itemUniqueName"
                        class="recommendation-reward">
                        <div class="recommendation-reward__head">
                            <span class="rarity" :class="reward.rarity.toLowerCase()">{{ reward.rarity }}</span>
                            <strong>{{ reward.itemName || reward.itemUniqueName }}</strong>
                            <span>+{{ reward.needPoints }}</span>
                        </div>
                        <p v-for="player in reward.players" :key="player.playerId">
                            <strong>{{ player.playerName }}</strong> needs {{ player.missingCount }} for
                            {{ player.requiredFor.join(', ') }}
                        </p>
                    </div>
                </div>
            </section>
            <p class="recommendation-explanation">Score totals missing quantities covered. Drop chance and owned copy count do not change rank.</p>
        </div>
    </article>
</template>

<script>
export default {
    name: 'RelicRecommendationCard',
    props: {
        relic: { type: Object, required: true },
        expanded: { type: Boolean, required: true }
    },
    emits: ['toggle'],
    data() {
        return {
            refinements: [
                { key: 'intact', label: 'I' },
                { key: 'exceptional', label: 'E' },
                { key: 'flawless', label: 'F' },
                { key: 'radiant', label: 'R' }
            ]
        };
    },
    computed: {
        playerWord() {
            return this.relic.benefitingPlayerCount === 1 ? 'player' : 'players';
        },
        ownerSummary() {
            return this.relic.owners.map(owner => `${owner.playerName} ×${owner.totalCount}`).join(' · ');
        }
    }
};
</script>
