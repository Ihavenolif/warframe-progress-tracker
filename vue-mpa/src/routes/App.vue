<template>
    <NavbarElement></NavbarElement>

    <h1>Front page</h1>
    <p>todo</p>

</template>

<script>
import NavbarElement from '../components/Navbar/NavbarElement.vue';

export default {
    name: 'AppPage',
    components: {
        NavbarElement
    }
}
</script>

<style>
#app {
    font-family: Avenir, Helvetica, Arial, sans-serif;
    -webkit-font-smoothing: antialiased;
    -moz-osx-font-smoothing: grayscale;
    text-align: center;
    color: #2c3e50;
}
</style>
