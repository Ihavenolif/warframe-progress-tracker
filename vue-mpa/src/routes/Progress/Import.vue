<script>
import ImportProgressForm from '@/components/Forms/ImportProgressForm.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';

export default {
    name: 'ImportPage',
    components: {
        NavbarElement,
        ImportProgressForm
    }
};
</script>

<template>
    <NavbarElement></NavbarElement>

    <div class="three-column-layout">
        <div class="three-column-layout__side">
        </div>
        <main class="three-column-layout__main">
            <h1>Import Progress</h1>
            <p>
                Download and run AlecaFrame parser, then upload generated <code>out.json</code> below.
            </p>
            <a class="btn btn-outline-secondary" href="/alecaframe-parser.exe" download>
                Download AlecaFrame Parser
            </a>
            <ImportProgressForm></ImportProgressForm>
        </main>
        <div class="three-column-layout__side">
        </div>
    </div>
</template>
