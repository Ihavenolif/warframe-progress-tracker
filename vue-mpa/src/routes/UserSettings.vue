<template>
    <NavbarElement></NavbarElement>

    <div class="row">
        <div class="column left">
        </div>
        <div class="column middle">
            <h2>
                Account settings
                <span style="font-weight: normal;"> - {{ username }}</span>
            </h2>

            <!-- Change Password Button -->
            <button @click="changePassword" class="simple-btn">
                Change Password
            </button>

            <hr>

        </div>
        <div class="column right">

        </div>
    </div>


</template>

<script>

import NavbarElement from '@/components/Navbar/NavbarElement.vue';

export default {
    components: {
        NavbarElement
    },
    name: "UserSettingsPage",
    computed: {
        username() {
            return this.$store.state.username;
        }
    },
    methods: {
        changePassword() {
            // Placeholder for future functionality
            console.log("Change Password button clicked");
        }
    }
}

</script>

<style></style>