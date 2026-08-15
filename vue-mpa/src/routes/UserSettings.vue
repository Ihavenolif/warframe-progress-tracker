<template>
    <NavbarElement></NavbarElement>

    <div class="three-column-layout">
        <div class="three-column-layout__side">
        </div>
        <main class="three-column-layout__main settings-page">
            <h2>
                Account settings
                <span class="fw-normal"> - {{ username }}</span>
            </h2>

            <!-- Change Password Button -->
            <button @click="changePassword" class="btn btn-primary">
                Change Password
            </button>

            <hr>

            <h2>Warframe</h2>
            <p>Account name: {{ playerName ?? "Not linked" }}</p>
            <input type="text" name="warframeName" id="warframeName" v-if="inputVisible" v-model="playerNameInput"
                placeholder="Enter your Warframe account name">
            <br v-if="inputVisible">
            <button v-if="!playerName" class="btn btn-primary" @click="linkAccount">Link account</button>
            <button v-else class="btn btn-primary" @click="unlinkAccount">Unlink account</button>

        </main>
        <div class="three-column-layout__side">

        </div>
    </div>


</template>

<script>

import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import { authFetch, getPlayerName } from '@/util/util';

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
        async changePassword() {
            // Placeholder for future functionality
            console.log("Change Password button clicked");
        },
        async linkAccount() {
            if (!this.inputVisible) {
                this.inputVisible = true;
                return;
            }
            else {
                if (!this.playerNameInput) return;

                const res = await authFetch("/api/user/addPlayer", {
                    method: "POST",
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ playerName: this.playerNameInput })
                })

                if (res.ok) {

                    const data = await res.json();
                    this.$store.commit("setCredentials", { username: this.username, token: data.token });
                    this.playerName = this.playerNameInput;
                    this.playerNameInput = "";
                    this.inputVisible = false;
                }
            }
        },
        async unlinkAccount() {
            if (!confirm("Are you sure you want to unlink your Warframe account? This action cannot be undone.")) return;
            const res = await authFetch("/api/user/removePlayer", {
                method: "POST"
            })

            if (res.ok) {
                const data = await res.json();
                this.$store.commit("setCredentials", { username: this.username, token: data.token });
                this.playerName = null;
            }
        }
    },
    async mounted() {
        getPlayerName().then(name => {
            this.playerName = name;
        });
    },
    data() {
        return {
            playerName: null,
            inputVisible: false,
            playerNameInput: ""
        }
    }
}

</script>
