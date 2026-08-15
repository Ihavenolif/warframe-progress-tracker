<template>
    <NavbarElement></NavbarElement>

    <div class="three-column-layout">
        <div class="three-column-layout__side">
        </div>
        <main class="three-column-layout__main">
            <LoginForm></LoginForm>

        </main>
        <div class="three-column-layout__side">

        </div>
    </div>

    
</template>

<script>
import LoginForm from '@/components/Forms/LoginForm.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';

export default {
    components: {
        NavbarElement,
        LoginForm
    },
    name: "LoginPage"
};
</script>
