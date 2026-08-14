<template>
    <header class="page-heading">
        <div>
            <p class="eyebrow">Void relic inventory</p>
            <h1>Relics</h1>
            <p>Browse known relics and quantities from your latest profile import.</p>
        </div>
        <RouterLink class="import-link" to="/progress/import">Refresh inventory</RouterLink>
    </header>
</template>

<script>
export default {
    name: 'RelicPageHeader'
};
</script>

<style scoped>
.page-heading { display: flex; justify-content: space-between; gap: 24px; align-items: end; margin-bottom: 28px; }
.page-heading h1 { margin: 2px 0 4px; font-size: clamp(2rem, 5vw, 3.3rem); line-height: 1; }
.page-heading p { margin: 0; color: #607080; }
.eyebrow { color: var(--accent) !important; font-size: .75rem; font-weight: 800; letter-spacing: .14em; text-transform: uppercase; }
.import-link { background: #263746; color: white; padding: 11px 16px; text-decoration: none; border-radius: 3px; white-space: nowrap; }

@media (max-width: 560px) {
    .page-heading { align-items: start; flex-direction: column; }
}
</style>
