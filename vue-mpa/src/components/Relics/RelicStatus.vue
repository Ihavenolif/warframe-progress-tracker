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
            <button class="btn btn-secondary" type="button" @click="$emit('retry')">Try again</button>
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
