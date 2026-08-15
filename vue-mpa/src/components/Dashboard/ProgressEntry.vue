<template>
    <article class="progress-entry">
        <button class="entry-summary" type="button" :aria-expanded="expanded" @click="expanded = !expanded">
            <div class="entry-title">
                <span class="expand-indicator" aria-hidden="true">{{ expanded ? '−' : '+' }}</span>
                <div>
                    <h3>{{ formatDateTime(entry.createdAt) }}</h3>
                    <p>{{ formatNumber(entry.previousTotalMasteryXp) }} -> {{ formatNumber(entry.currentTotalMasteryXp) }} mastery XP</p>
                </div>
            </div>
            <strong>{{ signedNumber(entry.masteryXpGained) }} XP</strong>
        </button>

        <div v-if="expanded" class="entry-details">
            <div class="detail-column">
                <h4>Leveled items</h4>
                <table v-if="entry.leveledItems.length > 0">
                    <thead>
                        <tr><th>Item</th><th>Rank</th><th>XP</th></tr>
                    </thead>
                    <tbody>
                        <tr v-for="item in entry.leveledItems" :key="item.uniqueName">
                            <td>{{ item.name || item.uniqueName }}</td>
                            <td>{{ item.previousRank }} -> {{ item.currentRank }}</td>
                            <td>{{ signedNumber(item.masteryXpGained) }}</td>
                        </tr>
                    </tbody>
                </table>
                <p v-else class="muted">No item levels.</p>
            </div>

            <div class="detail-column">
                <h4>Missions</h4>
                <table v-if="missionRows.length > 0">
                    <thead>
                        <tr><th>Mission</th><th>Progress</th><th>XP</th></tr>
                    </thead>
                    <tbody>
                        <tr v-for="mission in missionRows" :key="`${mission.uniqueName}-${mission.progress}`">
                            <td>{{ mission.name || mission.uniqueName }}<span v-if="mission.planet">, {{ mission.planet }}</span></td>
                            <td>{{ mission.progress }}</td>
                            <td>{{ signedNumber(mission.masteryXpGained) }}</td>
                        </tr>
                    </tbody>
                </table>
                <p v-else class="muted">No mission progress.</p>
            </div>
        </div>
    </article>
</template>

<script>
export default {
    name: 'ProgressEntry',
    props: {
        entry: {
            type: Object,
            required: true
        }
    },
    data() {
        return {
            expanded: false
        }
    },
    computed: {
        missionRows() {
            return this.entry.missions.flatMap(mission => {
                const progressCount = Number(mission.completed) + Number(mission.steelPathCompleted);
                const masteryXpGained = progressCount > 0 ? mission.masteryXpGained / progressCount : 0;
                const rows = [];

                if (mission.completed) {
                    rows.push({ ...mission, progress: 'Completed', masteryXpGained });
                }
                if (mission.steelPathCompleted) {
                    rows.push({ ...mission, progress: 'Steel Path completed', masteryXpGained });
                }

                return rows;
            });
        }
    },
    methods: {
        formatNumber(value) {
            return new Intl.NumberFormat().format(value || 0);
        },
        signedNumber(value) {
            const number = value || 0;
            return `${number >= 0 ? '+' : ''}${this.formatNumber(number)}`;
        },
        formatDateTime(value) {
            return new Date(value).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' });
        }
    }
}
</script>
