<template>
    <section class="dashboard-section dashboard-summary">
        <div class="section-head">
            <div>
                <p class="eyebrow">Current state</p>
                <h2>Progress overview</h2>
                <p>Mastery, collection readiness, and mission completion.</p>
            </div>
        </div>

        <div v-if="loading" class="dashboard-state-card">Loading progress overview...</div>
        <div v-else-if="errorMessage" class="dashboard-state-card dashboard-error" role="alert">
            <strong>Progress overview could not be loaded</strong>
            <span>{{ errorMessage }}</span>
        </div>
        <template v-else-if="summary">
            <div class="dashboard-summary__headline">
                <div class="dashboard-summary__rank">
                    <span>Current rank</span>
                    <strong>{{ summary.masteryRank }}</strong>
                    <small>mastery progression</small>
                </div>
                <div class="dashboard-summary__stat">
                    <span>Total mastery XP</span>
                    <strong>{{ formatNumber(summary.totalMasteryXp) }}</strong>
                </div>
                <div class="dashboard-summary__stat">
                    <span>Last 7 days</span>
                    <strong>+{{ formatNumber(summary.masteryXpGained7Days) }}</strong>
                </div>
                <div class="dashboard-summary__stat">
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
                        <div>
                            <strong>{{ summary.items.mastered }}</strong>
                            <span>Mastered</span>
                        </div>
                        <div>
                            <strong>{{ summary.items.started }}</strong>
                            <span>Started</span>
                        </div>
                        <div>
                            <strong>{{ summary.items.unowned }}</strong>
                            <span>Unowned</span>
                        </div>
                        <div class="is-ready">
                            <strong>{{ summary.items.craftReady }}</strong>
                            <span>Craft-ready</span>
                        </div>
                    </div>
                </article>

                <article class="dashboard-summary__panel dashboard-summary__missions">
                    <header>
                        <div>
                            <span>Star chart</span>
                            <h3>Mission progress</h3>
                        </div>
                    </header>
                    <div class="dashboard-summary__mission-row">
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
                    <div class="dashboard-summary__mission-row dashboard-summary__mission-row--steel-path">
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

                <article class="dashboard-summary__panel dashboard-summary__categories">
                    <header>
                        <div>
                            <span>Catalog</span>
                            <h3>Category completion</h3>
                        </div>
                        <strong>{{ summary.categories.length }}</strong>
                    </header>
                    <div v-if="summary.categories.length" class="dashboard-summary__category-grid">
                        <div v-for="category in summary.categories" :key="category.category"
                            class="dashboard-summary__category">
                            <div>
                                <strong>{{ formatCategory(category.category) }}</strong>
                                <span>{{ category.mastered }} / {{ category.total }}</span>
                            </div>
                            <div class="dashboard-completion-track" role="progressbar"
                                :aria-label="`${formatCategory(category.category)} mastery`"
                                :aria-valuenow="category.mastered" aria-valuemin="0" :aria-valuemax="category.total">
                                <span :style="{ width: `${completionPercent(category.mastered, category.total)}%` }"></span>
                            </div>
                        </div>
                    </div>
                    <p v-else class="dashboard-summary__empty">No mastery categories available.</p>
                </article>
            </div>
        </template>
    </section>
</template>

<script>
export default {
    name: 'DashboardSummary',
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
        formatNumber(value) {
            return new Intl.NumberFormat().format(value || 0);
        }
    }
};
</script>
