<template>
    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left">
        </div>
        <div class="column middle">
            <LoginForm></LoginForm>

        </div>
        <div class="column right">

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