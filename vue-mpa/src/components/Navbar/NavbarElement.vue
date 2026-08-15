<template>
    <div class="topnav" id="myTopnav">
        <RouterLink class="topnav-brand" to="/" exact-active-class="active">
            <span class="topnav-brand__mark" aria-hidden="true"></span>
            Orbitr
        </RouterLink>

        <UserMenu></UserMenu>

        <a href="javascript:void(0);" class="icon topnav-toggle" @click="responsiveNavbar">&#9776;</a>
    </div>
</template>

<script>
import UserMenu from './UserMenu.vue';

export default {
    methods: {
        responsiveNavbar() {
            const x = document.getElementById("myTopnav");
            if (x.className === "topnav") {
                x.className += " responsive";
            } else {
                x.className = "topnav";
            }
        }
    },
    components: {
        UserMenu
    }
}
</script>
