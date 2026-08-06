<template>
    <NavbarDropdown :title="username" :is-right-aligned="true">
        <NavbarDropdownEntry href="/settings">Settings</NavbarDropdownEntry>
        <NavbarDropdownEntry href="/logout">Log out</NavbarDropdownEntry>
    </NavbarDropdown>

    <NavbarDropdown title="Warframe" :is-right-aligned="true" v-if="shouldDisplayWarframe">
        <NavbarDropdownEntry href="/clans">Clans</NavbarDropdownEntry>
        <NavbarDropdownEntry href="/progress">Progress</NavbarDropdownEntry>
        <NavbarDropdownEntry href="/progress/import">Import progress</NavbarDropdownEntry>
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
