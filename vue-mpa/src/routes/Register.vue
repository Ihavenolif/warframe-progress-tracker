<template>
    <NavbarElement></NavbarElement>

    <div class="three-column-layout">
        <div class="three-column-layout__side">
        </div>
        <main class="three-column-layout__main">
            <RegisterForm></RegisterForm>

        </main>
        <div class="three-column-layout__side">

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
