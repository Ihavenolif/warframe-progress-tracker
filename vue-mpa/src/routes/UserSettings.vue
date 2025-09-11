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

            <h2>Warframe</h2>
            <p>Account name: {{ playerName ?? "Not linked" }}</p>
            <input type="text" name="warframeName" id="warframeName" v-if="inputVisible" v-model="playerNameInput"
                placeholder="Enter your Warframe account name">
            <br v-if="inputVisible">
            <button v-if="!playerName" class="simple-btn" @click="linkAccount">Link account</button>
            <button v-else class="simple-btn" @click="unlinkAccount">Unlink account</button>

        </div>
        <div class="column right">

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
                    window.location.reload();
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
                window.location.reload();
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

<style>
.simple-btn {
    background-color: #007bff;
    color: white;
    border: none;
    padding: 10px 20px;
    text-align: center;
    text-decoration: none;
    display: inline-block;
    font-size: 16px;
    margin: 4px 2px;
    cursor: pointer;
}

input[type=text],
input[type=password] {
    width: 100%;
    padding: 12px 20px;
    margin: 8px 0;
    display: inline-block;
    border: 1px solid #ccc;
    box-sizing: border-box;
}
</style>