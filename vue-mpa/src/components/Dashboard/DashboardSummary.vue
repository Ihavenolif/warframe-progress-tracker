<template>
    <section class="dashboard-section dashboard-summary" aria-label="Progress overview">
        <div v-if="loading" class="dashboard-state-card">Loading progress overview...</div>
        <div v-else-if="errorMessage" class="dashboard-state-card dashboard-error" role="alert">
            <strong>Progress overview could not be loaded</strong>
            <span>{{ errorMessage }}</span>
        </div>
        <template v-else-if="summary">
            <div class="dashboard-summary__headline">
                <div class="dashboard-summary__rank">
                    <div class="dashboard-summary__rank-metric">
                        <span>Current rank</span>
                        <strong v-if="summary.masteryRank > 30" class="dashboard-summary__legendary-rank"
                            :aria-label="`Legendary Rank ${summary.masteryRank - 30}`">
                            <img :src="legendaryRankIcon" alt="" aria-hidden="true">
                            {{ summary.masteryRank - 30 }}
                        </strong>
                        <strong v-else :aria-label="`Mastery Rank ${summary.masteryRank}`">
                            {{ summary.masteryRank }}
                        </strong>
                    </div>
                    <div class="dashboard-summary__rank-metric dashboard-summary__rank-metric--xp">
                        <span>XP to next rank</span>
                        <strong>{{ formatNumber(rankProgress.remaining) }}</strong>
                    </div>
                    <div class="dashboard-completion-track" role="progressbar" aria-label="Progress to next rank"
                        :aria-valuenow="rankProgress.earned" aria-valuemin="0" :aria-valuemax="rankProgress.required">
                        <span :style="{ width: `${rankProgress.percent}%` }"></span>
                    </div>
                </div>
                <div class="dashboard-summary__stat">
                    <span>Latest import</span>
                    <strong>{{ formatImportDate(summary.latestImport) }}</strong>
                </div>
                <div class="dashboard-summary__stat dashboard-summary__gain">
                    <span>Last 7 days</span>
                    <strong>+{{ formatNumber(summary.masteryXpGained7Days) }}</strong>
                </div>
                <div class="dashboard-summary__stat dashboard-summary__gain">
                    <span>Last 30 days</span>
                    <strong>+{{ formatNumber(summary.masteryXpGained30Days) }}</strong>
                </div>
            </div>

            <div class="dashboard-summary__details">
                <article class="dashboard-summary__panel dashboard-summary__items">
                    <header>
                        <div>
                            <span>Arsenal</span>
                            <h3>Item progress</h3>
                        </div>
                        <strong>{{ summary.items.mastered }} / {{ summary.items.total }}</strong>
                    </header>
                    <div class="dashboard-completion-track" role="progressbar" aria-label="Mastered items"
                        :aria-valuenow="summary.items.mastered" aria-valuemin="0"
                        :aria-valuemax="summary.items.total">
                        <span :style="{ width: `${completionPercent(summary.items.mastered, summary.items.total)}%` }"></span>
                    </div>
                    <div class="dashboard-summary__states">
                        <RouterLink :to="{ name: 'progress', query: { states: 'mastered' } }">
                            <strong>{{ summary.items.mastered }}</strong>
                            <span>Mastered</span>
                        </RouterLink>
                        <RouterLink :to="{ name: 'progress', query: { states: 'in-progress' } }">
                            <strong>{{ summary.items.started }}</strong>
                            <span>Started</span>
                        </RouterLink>
                        <RouterLink :to="{ name: 'progress', query: { states: 'unowned' } }">
                            <strong>{{ summary.items.unowned }}</strong>
                            <span>Unowned</span>
                        </RouterLink>
                        <RouterLink class="is-ready" :to="{ name: 'progress', query: { states: 'craft-ready' } }">
                            <strong>{{ summary.items.craftReady }}</strong>
                            <span>Craft-ready</span>
                        </RouterLink>
                    </div>
                </article>

                <article class="dashboard-summary__panel dashboard-summary__missions">
                    <header>
                        <div>
                            <span>Star chart</span>
                            <h3>Mission progress</h3>
                        </div>
                    </header>
                    <div class="dashboard-summary__progress-row">
                        <div>
                            <strong>Normal</strong>
                            <span>{{ summary.missions.normalCompleted }} / {{ summary.missions.normalTotal }}</span>
                        </div>
                        <div class="dashboard-completion-track" role="progressbar" aria-label="Normal missions"
                            :aria-valuenow="summary.missions.normalCompleted" aria-valuemin="0"
                            :aria-valuemax="summary.missions.normalTotal">
                            <span :style="{ width: `${completionPercent(summary.missions.normalCompleted, summary.missions.normalTotal)}%` }"></span>
                        </div>
                    </div>
                    <div class="dashboard-summary__progress-row dashboard-summary__progress-row--steel-path">
                        <div>
                            <strong>Steel Path</strong>
                            <span>{{ summary.missions.steelPathCompleted }} / {{ summary.missions.steelPathTotal }}</span>
                        </div>
                        <div class="dashboard-completion-track" role="progressbar" aria-label="Steel Path missions"
                            :aria-valuenow="summary.missions.steelPathCompleted" aria-valuemin="0"
                            :aria-valuemax="summary.missions.steelPathTotal">
                            <span :style="{ width: `${completionPercent(summary.missions.steelPathCompleted, summary.missions.steelPathTotal)}%` }"></span>
                        </div>
                    </div>
                </article>

                <article class="dashboard-summary__panel dashboard-summary__intrinsics">
                    <header>
                        <div>
                            <span>Intrinsic skills</span>
                            <h3>Intrinsics</h3>
                        </div>
                    </header>
                    <div class="dashboard-summary__progress-row dashboard-summary__progress-row--railjack">
                        <div>
                            <strong>Railjack</strong>
                            <span>{{ summary.intrinsics.railjack }} / {{ summary.intrinsics.railjackTotal }}</span>
                        </div>
                        <div class="dashboard-completion-track" role="progressbar" aria-label="Railjack intrinsics"
                            :aria-valuenow="summary.intrinsics.railjack" aria-valuemin="0"
                            :aria-valuemax="summary.intrinsics.railjackTotal">
                            <span :style="{ width: `${completionPercent(summary.intrinsics.railjack, summary.intrinsics.railjackTotal)}%` }"></span>
                        </div>
                    </div>
                    <div class="dashboard-summary__progress-row dashboard-summary__progress-row--duviri">
                        <div>
                            <strong>Duviri</strong>
                            <span>{{ summary.intrinsics.duviri }} / {{ summary.intrinsics.duviriTotal }}</span>
                        </div>
                        <div class="dashboard-completion-track" role="progressbar" aria-label="Duviri intrinsics"
                            :aria-valuenow="summary.intrinsics.duviri" aria-valuemin="0"
                            :aria-valuemax="summary.intrinsics.duviriTotal">
                            <span :style="{ width: `${completionPercent(summary.intrinsics.duviri, summary.intrinsics.duviriTotal)}%` }"></span>
                        </div>
                    </div>
                </article>

                <article class="dashboard-summary__panel dashboard-summary__categories">
                    <header>
                        <div>
                            <span>Catalog</span>
                            <h3>Mastery remaining</h3>
                        </div>
                        <RouterLink class="dashboard-summary__view-all" :to="{ name: 'progress' }">View all</RouterLink>
                    </header>
                    <p class="dashboard-summary__category-summary">
                        {{ remainingCategories.length }} classes in progress / {{ completedCategoryCount }} complete
                    </p>
                    <div v-if="remainingCategories.length" class="dashboard-summary__category-grid">
                        <RouterLink v-for="category in remainingCategories" :key="category.category"
                            class="dashboard-summary__category"
                            :to="{ name: 'progress', query: { classes: category.category } }"
                            :aria-label="`View ${formatCategory(category.category)} progress`">
                            <div>
                                <strong>{{ formatCategory(category.category) }}</strong>
                                <span>{{ category.mastered }} / {{ category.total }} &gt;</span>
                            </div>
                            <div class="dashboard-completion-track" role="progressbar"
                                :aria-label="`${formatCategory(category.category)} mastery`"
                                :aria-valuenow="category.mastered" aria-valuemin="0" :aria-valuemax="category.total">
                                <span :style="{ width: `${completionPercent(category.mastered, category.total)}%` }"></span>
                            </div>
                        </RouterLink>
                    </div>
                    <p v-else class="dashboard-summary__empty">All available categories mastered.</p>
                </article>
            </div>
        </template>
    </section>
</template>

<script>
import legendaryRankIcon from '@/assets/legendary-rank.png';

export default {
    name: 'DashboardSummary',
    data() {
        return {
            legendaryRankIcon
        };
    },
    props: {
        summary: {
            type: Object,
            default: null
        },
        loading: {
            type: Boolean,
            default: false
        },
        errorMessage: {
            type: String,
            default: ''
        }
    },
    computed: {
        completedCategoryCount() {
            return this.summary?.categories.filter(category =>
                category.category !== 'Plexus' && category.mastered >= category.total).length || 0;
        },
        remainingCategories() {
            if (!this.summary) return [];
            return this.summary.categories
                .filter(category => category.category !== 'Plexus' && category.mastered < category.total)
                .sort((first, second) =>
                    (second.total - second.mastered) - (first.total - first.mastered) ||
                    first.category.localeCompare(second.category));
        },
        rankProgress() {
            if (!this.summary) return { earned: 0, required: 0, remaining: 0, percent: 0 };

            const currentThreshold = this.masteryThreshold(this.summary.masteryRank);
            const nextThreshold = this.masteryThreshold(this.summary.masteryRank + 1);
            const required = nextThreshold - currentThreshold;
            const earned = Math.min(required, Math.max(0, this.summary.totalMasteryXp - currentThreshold));

            return {
                earned,
                required,
                remaining: Math.max(0, nextThreshold - this.summary.totalMasteryXp),
                percent: this.completionPercent(earned, required)
            };
        }
    },
    methods: {
        completionPercent(completed, total) {
            if (!total) return 0;
            return Math.min(100, Math.max(0, Math.round((completed / total) * 100)));
        },
        formatCategory(value) {
            return value
                .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
                .replace(/[_-]+/g, ' ');
        },
        formatImportDate(receipt) {
            if (!receipt) return 'Never';
            return new Date(receipt.importedAt).toLocaleDateString(undefined, {
                month: 'short',
                day: 'numeric'
            });
        },
        masteryThreshold(rank) {
            return rank <= 30
                ? 2500 * rank * rank
                : 2250000 + (rank - 30) * 147500;
        },
        formatNumber(value) {
            return new Intl.NumberFormat().format(value || 0);
        }
    }
};
</script>
