<script>
import ImportProgressForm from '@/components/Forms/ImportProgressForm.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import ThreeColumnLayout from '@/components/ThreeColumnLayout.vue';

export default {
    name: 'ImportPage',
    components: {
        NavbarElement,
        ImportProgressForm,
        ThreeColumnLayout
    }
};
</script>

<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <h1>Import Progress</h1>
        <p>
            Download and run AlecaFrame parser, then upload generated <code>out.json</code> below.
        </p>
        <a class="btn btn-outline-secondary" href="/alecaframe-parser.exe" download>
            Download AlecaFrame Parser
        </a>
        <ImportProgressForm></ImportProgressForm>
    </ThreeColumnLayout>
</template>
