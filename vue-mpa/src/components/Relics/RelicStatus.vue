<template>
    <section class="state-card" :class="{ 'error-state': type === 'error' }">
        <template v-if="type === 'no-player'">
            <h2>Link a player first</h2>
            <p>Relic ownership is tied to your Warframe player profile.</p>
            <RouterLink to="/settings">Open settings</RouterLink>
        </template>
        <template v-else-if="type === 'error'">
            <h2>Relics could not be loaded</h2>
            <p>{{ message }}</p>
            <button type="button" @click="$emit('retry')">Try again</button>
        </template>
        <template v-else>
            <h2>No relics found</h2>
            <p>Change filters, or ask an admin to refresh PublicExport metadata.</p>
        </template>
    </section>
</template>

<script>
export default {
    name: 'RelicStatus',
    props: {
        type: { type: String, required: true },
        message: { type: String, default: '' }
    },
    emits: ['retry']
};
</script>

<style scoped>
.state-card { padding: 40px; border: 1px solid #d5dde1; background: #f7f9fa; text-align: center; }
.state-card button { padding: 9px 16px; border: 0; background: #263746; color: white; cursor: pointer; }
.error-state { border-color: #c98b92; }
</style>
