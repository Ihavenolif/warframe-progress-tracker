<script>
import ImportProgressForm from '@/components/Forms/ImportProgressForm.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';

export default {
    name: "ImportPage",
    components: {
        NavbarElement,
        ImportProgressForm
    }
}
</script>

<template>
    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left">
        </div>
        <div class="column middle">
            <h1>Import Progress</h1>
            <ImportProgressForm></ImportProgressForm>
        </div>
        <div class="column right">
        </div>
    </div>
</template>