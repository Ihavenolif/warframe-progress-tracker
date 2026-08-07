<template>
    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left"></div>
        <main class="column middle dashboard-page">
            <h1>Dashboard</h1>
            <MasteryProgressChart />
            <LatestProgressEntries />
        </main>
        <div class="column right"></div>
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

<style scoped>
.dashboard-page {
    text-align: left;
}

</style>
