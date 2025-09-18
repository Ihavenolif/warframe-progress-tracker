<template>
    <div v-if="!username">
        <UserNotLoggedIn></UserNotLoggedIn>
    </div>
    <div v-else>
        <UserLoggedIn></UserLoggedIn>
    </div>

</template>

<script>

import UserLoggedIn from './UserLoggedIn.vue';
import UserNotLoggedIn from './UserNotLoggedIn.vue';
import { subscribe, TokenUpdateSignal } from '@/util/signals';

export default {
    components: {
        UserLoggedIn,
        UserNotLoggedIn
    },
    computed: {
        username() {
            return this.$store.state.username;
        }
    },
    mounted() {
        this.unsubscribe = subscribe(TokenUpdateSignal, () => {
            this.$forceUpdate();
        });
    }
}

</script>