<template>
    <NavbarElement></NavbarElement>

    <div class="three-column-layout">
        <div class="three-column-layout__side"></div>
        <main class="three-column-layout__main dashboard-page">
            <h1>Dashboard</h1>
            <MasteryProgressChart />
            <LatestProgressEntries />
        </main>
        <div class="three-column-layout__side"></div>
    </div>
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
}
</script>
