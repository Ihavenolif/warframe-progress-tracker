<template>

    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left">
        </div>
        <div class="column middle">
            <ProgressTable></ProgressTable>

        </div>
        <div class="column right">
    </div>
</div>

</template>

<script>
import ProgressTable from '@/components/Progress/ProgressTable.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';

export default {
    name: "ProgressPage",
    components: {
        ProgressTable,
        NavbarElement
    }
}
</script>