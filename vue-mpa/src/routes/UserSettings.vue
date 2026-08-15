<template>
    <NavbarElement />

    <main class="settings-page">
        <header class="page-heading">
            <div>
                <p class="eyebrow">Account</p>
                <h1>Settings</h1>
                <p>Manage login security and your linked Warframe profile.</p>
            </div>
            <span class="settings-user-badge">{{ username }}</span>
        </header>

        <div class="settings-grid">
            <section class="settings-card" aria-labelledby="login-settings-heading">
                <div class="settings-card-heading">
                    <div class="settings-card-mark" aria-hidden="true">{{ accountInitial }}</div>
                    <div>
                        <p class="eyebrow">Login identity</p>
                        <h2 id="login-settings-heading">Account security</h2>
                    </div>
                    <span class="settings-status settings-status--active">Active</span>
                </div>

                <dl class="settings-details">
                    <div>
                        <dt>Username</dt>
                        <dd>{{ username }}</dd>
                    </div>
                    <div>
                        <dt>Password</dt>
                        <dd>Protected</dd>
                    </div>
                </dl>

                <div class="settings-card-actions">
                    <button class="btn btn-outline-secondary" type="button" @click="changePassword">
                        Change password
                    </button>
                </div>
                <p v-if="passwordNotice" class="settings-notice" role="status">
                    Password changes are not available yet.
                </p>
            </section>

            <section class="settings-card settings-card--warframe" aria-labelledby="warframe-settings-heading">
                <div class="settings-card-heading">
                    <div class="settings-card-mark settings-card-mark--warframe" aria-hidden="true">W</div>
                    <div>
                        <p class="eyebrow">Game profile</p>
                        <h2 id="warframe-settings-heading">Warframe account</h2>
                    </div>
                    <span v-if="!loadingPlayer" class="settings-status"
                        :class="playerName ? 'settings-status--linked' : 'settings-status--unlinked'">
                        {{ playerName ? 'Linked' : 'Not linked' }}
                    </span>
                </div>

                <div v-if="loadingPlayer" class="settings-loading">Loading linked profile...</div>

                <template v-else>
                    <div class="settings-profile">
                        <span>Warframe player</span>
                        <strong>{{ playerName || 'No profile linked' }}</strong>
                        <p>{{ playerName ? 'Progress, relics, clans, and dashboard data are enabled.' : 'Link profile to unlock progress tracking features.' }}</p>
                    </div>

                    <form v-if="inputVisible" class="settings-link-form" @submit.prevent="linkAccount">
                        <label for="warframe-name">
                            <span>Warframe account name</span>
                            <input id="warframe-name" v-model.trim="playerNameInput" type="text"
                                placeholder="Enter exact player name" autocomplete="off" autofocus>
                        </label>
                        <div class="settings-link-actions">
                            <button class="btn btn-outline-secondary" type="button" @click="cancelLink">Cancel</button>
                            <button class="btn btn-primary" type="submit" :disabled="!playerNameInput || busy">
                                {{ busy ? 'Linking...' : 'Link account' }}
                            </button>
                        </div>
                    </form>

                    <p v-if="errorMessage" class="settings-error" role="alert">{{ errorMessage }}</p>

                    <div v-if="!inputVisible" class="settings-card-actions">
                        <button v-if="!playerName" class="btn btn-primary" type="button" @click="showLinkForm">
                            Link account
                        </button>
                        <button v-else class="btn settings-danger-button" type="button" :disabled="busy"
                            @click="unlinkAccount">
                            {{ busy ? 'Unlinking...' : 'Unlink account' }}
                        </button>
                    </div>
                </template>
            </section>
        </div>
    </main>
</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import { authFetch, getPlayerName } from '@/util/util';

export default {
    name: 'UserSettingsPage',
    components: {
        NavbarElement
    },
    data() {
        return {
            playerName: null,
            loadingPlayer: true,
            inputVisible: false,
            playerNameInput: '',
            errorMessage: '',
            passwordNotice: false,
            busy: false
        };
    },
    computed: {
        username() {
            return this.$store.state.username;
        },
        accountInitial() {
            return this.username?.trim().charAt(0).toUpperCase() || '?';
        }
    },
    mounted() {
        this.loadPlayerName();
    },
    methods: {
        changePassword() {
            this.passwordNotice = true;
        },
        showLinkForm() {
            this.inputVisible = true;
            this.errorMessage = '';
        },
        cancelLink() {
            this.inputVisible = false;
            this.playerNameInput = '';
            this.errorMessage = '';
        },
        async loadPlayerName() {
            this.loadingPlayer = true;
            this.errorMessage = '';

            try {
                this.playerName = await getPlayerName();
            } catch (error) {
                this.errorMessage = error.message || 'Linked profile could not be loaded.';
            } finally {
                this.loadingPlayer = false;
            }
        },
        async linkAccount() {
            if (!this.playerNameInput || this.busy) return;
            this.busy = true;
            this.errorMessage = '';

            try {
                const response = await authFetch('/api/user/addPlayer', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ playerName: this.playerNameInput })
                });

                if (!response.ok) {
                    this.errorMessage = (await response.text()) || 'Warframe account could not be linked.';
                    return;
                }

                const data = await response.json();
                this.$store.commit('setCredentials', { username: this.username, token: data.token });
                this.playerName = this.playerNameInput;
                this.cancelLink();
            } catch (error) {
                this.errorMessage = error.message || 'Warframe account could not be linked.';
            } finally {
                this.busy = false;
            }
        },
        async unlinkAccount() {
            if (!confirm('Are you sure you want to unlink your Warframe account? This action cannot be undone.')) return;
            if (this.busy) return;
            this.busy = true;
            this.errorMessage = '';

            try {
                const response = await authFetch('/api/user/removePlayer', { method: 'POST' });
                if (!response.ok) {
                    this.errorMessage = (await response.text()) || 'Warframe account could not be unlinked.';
                    return;
                }

                const data = await response.json();
                this.$store.commit('setCredentials', { username: this.username, token: data.token });
                this.playerName = null;
            } catch (error) {
                this.errorMessage = error.message || 'Warframe account could not be unlinked.';
            } finally {
                this.busy = false;
            }
        }
    }
};
</script>
