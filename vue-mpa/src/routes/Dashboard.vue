<template>
    <NavbarElement />

    <main class="dashboard-page">
        <header class="page-heading">
            <div>
                <p class="eyebrow">Tenno activity</p>
                <h1>Dashboard</h1>
                <p>Follow mastery gains and review changes from recent profile imports.</p>
            </div>
            <RouterLink class="btn btn-secondary dashboard-import-link" to="/progress/import">Import progress</RouterLink>
        </header>

        <MasteryProgressChart />
        <LatestProgressEntries />
    </main>
</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import MasteryProgressChart from '@/components/Dashboard/MasteryProgressChart.vue';
import LatestProgressEntries from '@/components/Dashboard/LatestProgressEntries.vue';

export default {
    name: 'DashboardPage',
    components: {
        NavbarElement,
        MasteryProgressChart,
        LatestProgressEntries
    }
};
</script>
