<template>
    <NavbarElement></NavbarElement>
    <div class="three-column-layout">
        <div class="three-column-layout__side">
        </div>
        <main class="three-column-layout__main">
            <h1> {{ header }} </h1>
        </main>
        <div class="three-column-layout__side">

        </div>
    </div>
</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import { authFetch } from '@/util/util';

export default {
    name: "LogoutPage",
    async mounted() {
        const res = await authFetch("/api/auth/logout", {
            method: "POST"
        });

        if (res.ok) {
            this.header = "Successfully logged out!";
            this.$store.commit('clearCredentials');

            this.redirectTimeout = setTimeout(() => {
                this.$router.push({ name: 'home' });
            }, 2000);
        } else {
            this.header = "Error logging out!";
        }
    },
    components: {
        NavbarElement
    },
    data() {
        return {
            header: "Logging out...",
            redirectTimeout: null
        }
    },
    beforeUnmount() {
        clearTimeout(this.redirectTimeout);
    }
}
</script>
