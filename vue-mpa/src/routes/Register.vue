<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <RegisterForm></RegisterForm>
    </ThreeColumnLayout>
</template>

<script>
import RegisterForm from '@/components/Forms/RegisterForm.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import ThreeColumnLayout from '@/components/ThreeColumnLayout.vue';

export default {
    components: {
        NavbarElement,
        RegisterForm,
        ThreeColumnLayout
    },
    name: "RegisterPage"
};

</script>
