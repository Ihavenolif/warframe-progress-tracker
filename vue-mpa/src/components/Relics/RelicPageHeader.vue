<template>
    <header class="page-heading">
        <div>
            <p class="eyebrow">Void relic inventory</p>
            <h1>Relics</h1>
            <p>Browse known relics and quantities from your latest profile import.</p>
        </div>
        <RouterLink class="btn btn-secondary import-link" to="/progress/import">Refresh inventory</RouterLink>
    </header>
</template>

<script>
export default {
    name: 'RelicPageHeader'
};
</script>
