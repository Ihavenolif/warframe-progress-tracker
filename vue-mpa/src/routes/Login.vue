<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <LoginForm></LoginForm>
    </ThreeColumnLayout>
</template>

<script>
import LoginForm from '@/components/Forms/LoginForm.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import ThreeColumnLayout from '@/components/ThreeColumnLayout.vue';

export default {
    components: {
        NavbarElement,
        LoginForm,
        ThreeColumnLayout
    },
    name: "LoginPage"
};
</script>
