<template>
    <section class="recommendation-panel">
        <div class="recommendation-panel__head">
            <div>
                <p class="recommendation-eyebrow">Squad relics</p>
                <h2>Find useful relics</h2>
                <p>Select up to four members. Score equals missing Prime craft quantities covered.</p>
            </div>
            <strong>{{ selectedPlayerIds.length }} / 4 selected</strong>
        </div>

        <p v-if="memberLoading" class="recommendation-state">Loading clan members...</p>
        <div v-else-if="memberError" class="recommendation-state recommendation-state--error">
            <span>{{ memberError }}</span>
            <button class="btn btn-secondary" type="button" @click="loadMembers">Retry</button>
        </div>
        <template v-else>
            <div class="recommendation-member-list">
                <label v-for="member in members" :key="member.id"
                    :class="['recommendation-member', { 'is-selected': isSelected(member.id) }]">
                    <input type="checkbox" :checked="isSelected(member.id)"
                        :disabled="requestLoading || (!isSelected(member.id) && selectedPlayerIds.length >= 4)"
                        @change="toggleMember(member.id)">
                    <span>{{ member.username }}</span>
                    <small>MR {{ member.masteryRank }}</small>
                </label>
            </div>
            <div class="recommendation-actions">
                <button class="btn btn-primary" type="button"
                    :disabled="selectedPlayerIds.length === 0 || requestLoading" @click="recommend">
                    {{ requestLoading ? 'Ranking relics...' : 'Recommend relics' }}
                </button>
                <p v-if="selectedPlayerIds.length === 0">Select at least one member.</p>
            </div>
        </template>

        <p v-if="requestError" class="recommendation-state recommendation-state--error">{{ requestError }}</p>
        <p v-else-if="hasRequested && !requestLoading && recommendations.length === 0"
            class="recommendation-state">No owned useful relics found for selected squad.</p>
        <RelicRecommendationResults v-else-if="recommendations.length > 0"
            :recommendations="recommendations" />
    </section>
</template>

<script>
import { authFetch } from '@/util/util';
import RelicRecommendationResults from './RelicRecommendationResults.vue';

export default {
    name: 'RelicRecommendationPanel',
    components: { RelicRecommendationResults },
    props: {
        clanName: { type: String, required: true }
    },
    data() {
        return {
            members: [],
            selectedPlayerIds: [],
            recommendations: [],
            memberLoading: true,
            memberError: '',
            requestLoading: false,
            requestError: '',
            hasRequested: false,
            requestNumber: 0,
            memberRequestNumber: 0
        };
    },
    computed: {
        encodedClanName() {
            return encodeURIComponent(this.clanName);
        }
    },
    created() {
        this.loadMembers();
    },
    watch: {
        clanName() {
            this.requestNumber++;
            this.selectedPlayerIds = [];
            this.recommendations = [];
            this.requestLoading = false;
            this.requestError = '';
            this.hasRequested = false;
            this.loadMembers();
        }
    },
    methods: {
        isSelected(playerId) {
            return this.selectedPlayerIds.includes(playerId);
        },
        toggleMember(playerId) {
            if (this.isSelected(playerId)) {
                this.selectedPlayerIds = this.selectedPlayerIds.filter(id => id !== playerId);
            } else if (this.selectedPlayerIds.length < 4) {
                this.selectedPlayerIds = [...this.selectedPlayerIds, playerId];
            }
            this.requestNumber++;
            this.requestLoading = false;
            this.requestError = '';
            this.hasRequested = false;
            this.recommendations = [];
        },
        async loadMembers() {
            const currentRequest = ++this.memberRequestNumber;
            this.memberLoading = true;
            this.memberError = '';
            try {
                const response = await authFetch(`/api/clans/${this.encodedClanName}/members`);
                if (!response || currentRequest !== this.memberRequestNumber) return;
                if (!response.ok) throw new Error((await response.text()) || `Request failed (${response.status})`);
                this.members = await response.json();
            } catch (error) {
                if (currentRequest === this.memberRequestNumber) {
                    this.memberError = error.message || 'Could not load clan members.';
                }
            } finally {
                if (currentRequest === this.memberRequestNumber) this.memberLoading = false;
            }
        },
        async recommend() {
            if (this.selectedPlayerIds.length === 0) return;
            const currentRequest = ++this.requestNumber;
            this.requestLoading = true;
            this.requestError = '';
            this.hasRequested = false;
            this.recommendations = [];
            try {
                const response = await authFetch(
                    `/api/clans/${this.encodedClanName}/relic-recommendations`,
                    {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ playerIds: this.selectedPlayerIds })
                    });
                if (!response || currentRequest !== this.requestNumber) return;
                if (!response.ok) throw new Error((await response.text()) || `Request failed (${response.status})`);
                const data = await response.json();
                this.recommendations = data.recommendations;
                this.hasRequested = true;
            } catch (error) {
                if (currentRequest === this.requestNumber) this.requestError = error.message || 'Could not rank relics.';
            } finally {
                if (currentRequest === this.requestNumber) this.requestLoading = false;
            }
        }
    }
};
</script>
