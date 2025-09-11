<template>
    <NavbarDropdown :title="username" :is-right-aligned="true">
        <NavbarDropdownEntry href="/settings">Settings</NavbarDropdownEntry>
        <NavbarDropdownEntry href="/logout">Log out</NavbarDropdownEntry>
    </NavbarDropdown>

    <NavbarDropdown title="Warframe" :is-right-aligned="true" v-if="shouldDisplayWarframe">
        <NavbarDropdownEntry href="/clans">Clans</NavbarDropdownEntry>
        <NavbarDropdownEntry href="/progress">Progress</NavbarDropdownEntry>
    </NavbarDropdown>

    <div class="right-aligned">
        <a href="/admin" v-if="shouldDisplayAdmin" :class="{
            'active': isAdminActive
        }">Admin</a>
    </div>




    <!--<div class="dropdown right-aligned">
        <button class="dropbtn">{{ username }}
            <i class="fa fa-caret-down"></i>
        </button>

        <div class="dropdown-content right-aligned">
            <a class='{% if page == "clans.html" %}active-inverted{% endif %}' href="/clans">Clans</a>
            <a class='{% if page == "progress/progress.html" %}active-inverted{% endif %}' href="/progress">Progress</a>
            <a class='{% if page == "settings.html" %}active-inverted{% endif %}' href="/settings">Settings</a>
            <a class='{% if page == "logout.html" %}active-inverted{% endif %}' href="/logout">Log out</a>
        </div>
    </div>-->

    <!--{% if page == "progress/progress.html" or page == "progress/import.html" %}-->

    <!--<div class="right-aligned">
        <a class='{% if page == "progress/import.html" %}active{% endif %}' href="/progress/import">Import</a>
    </div>-->

    <!--{% endif %}    -->
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
        },
        isAdminActive() {
            return window.location.pathname == "/admin";
        }
    },
    components: {
        NavbarDropdown,
        NavbarDropdownEntry
    }
}

</script>