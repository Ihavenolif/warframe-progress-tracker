<template>
    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left">
        </div>
        <div class="column middle">
            <RegisterForm></RegisterForm>

        </div>
        <div class="column right">

        </div>
    </div>
</template>

<script>
import RegisterForm from '@/components/Forms/RegisterForm.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';

export default {
    components: {
        NavbarElement,
        RegisterForm
    },
    name: "RegisterPage"
};

</script>