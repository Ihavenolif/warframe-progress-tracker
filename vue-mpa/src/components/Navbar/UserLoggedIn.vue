<template>
    <template v-if="shouldDisplayWarframe">
        <RouterLink to="/dashboard" active-class="active">Dashboard</RouterLink>
        <RouterLink to="/progress" active-class="active">Progress</RouterLink>
        <RouterLink to="/relics" active-class="active">Relics</RouterLink>
        <RouterLink to="/clans" active-class="active">Clans</RouterLink>
    </template>

    <NavbarDropdown :title="username" :is-right-aligned="true">
        <NavbarDropdownEntry href="/settings">Settings</NavbarDropdownEntry>
        <NavbarDropdownEntry href="/logout">Log out</NavbarDropdownEntry>
    </NavbarDropdown>

    <div class="right-aligned">
        <RouterLink to="/admin" v-if="shouldDisplayAdmin" exact-active-class="active">Admin</RouterLink>
    </div>
</template>

<script>
import NavbarDropdown from './NavbarDropdown.vue';
import NavbarDropdownEntry from './NavbarDropdownEntry.vue';
import { parseJwt, getRoles } from '@/util/util';


export default {
    computed: {
        username() {
            return this.$store.state.username;
        },
        token() {
            return this.$store.state.token;
        },
        shouldDisplayWarframe() {
            return getRoles(parseJwt(this.token)).includes('WARFRAME');
        },
        shouldDisplayAdmin() {
            return getRoles(parseJwt(this.token)).includes('ADMIN');
        }
    },
    components: {
        NavbarDropdown,
        NavbarDropdownEntry
    }
}

</script>
