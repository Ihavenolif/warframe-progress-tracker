<template>
    <NavbarElement />

    <main class="clan-index-page">
        <header class="page-heading">
            <div>
                <p class="eyebrow">Tenno network</p>
                <h1>Clans</h1>
                <p>Manage your clans, track shared progress, and answer invitations.</p>
            </div>
            <button class="btn btn-secondary create-clan-link" type="button" @click="showCreateForm">
                Create clan
            </button>
        </header>

        <form v-if="inputVisible" class="clan-create-panel" @submit.prevent="createClan">
            <label for="new-clan-name">
                <span>New clan name</span>
                <input id="new-clan-name" v-model.trim="newClanName" type="text" placeholder="Enter new clan name"
                    autocomplete="off" autofocus>
            </label>
            <div class="clan-create-actions">
                <button class="btn btn-outline-secondary" type="button" @click="cancelCreateClan">Cancel</button>
                <button class="btn btn-primary" type="submit" :disabled="!newClanName || creatingClan">
                    {{ creatingClan ? 'Creating...' : 'Create clan' }}
                </button>
            </div>
            <p v-if="errorMessage" class="clan-form-error" role="alert">{{ errorMessage }}</p>
        </form>

        <LoadingIndicator v-if="loading" />

        <section v-else-if="loadError" class="clan-state-card clan-state-card--error">
            <p class="eyebrow">Connection interrupted</p>
            <h2>Clans could not be loaded</h2>
            <p>{{ loadError }}</p>
            <button class="btn btn-secondary" type="button" @click="loadData">Try again</button>
        </section>

        <template v-else>
            <section class="clan-section" aria-labelledby="your-clans-heading">
                <div class="clan-section-heading">
                    <div>
                        <p class="eyebrow">Memberships</p>
                        <h2 id="your-clans-heading">Your clans</h2>
                    </div>
                    <span class="clan-count">{{ clans.length }}</span>
                </div>

                <div v-if="clans.length" class="clan-list">
                    <article v-for="clan in clans" :key="clan.id" class="clan-overview-card">
                        <div class="clan-card-mark" aria-hidden="true">{{ clanInitial(clan.name) }}</div>
                        <div class="clan-overview-card__info">
                            <span>Clan</span>
                            <h3>{{ clan.name }}</h3>
                            <p><strong>{{ clan.memberCount }}</strong> {{ memberLabel(clan.memberCount) }}</p>
                        </div>
                        <div class="clan-overview-card__actions">
                            <RouterLink class="btn btn-primary" :to="{ name: 'clan-progress', params: { clanName: clan.name } }">
                                View progress
                            </RouterLink>
                            <RouterLink class="btn btn-outline-secondary" :to="{ name: 'clan-details', params: { clanName: clan.name } }">
                                Manage clan
                            </RouterLink>
                        </div>
                    </article>
                </div>

                <div v-else class="clan-state-card">
                    <p class="eyebrow">No memberships</p>
                    <h3>Your clan roster is empty</h3>
                    <p>Create a clan or accept an invitation below to start tracking shared progress.</p>
                </div>
            </section>

            <section class="clan-section" aria-labelledby="invitations-heading">
                <div class="clan-section-heading">
                    <div>
                        <p class="eyebrow">Inbox</p>
                        <h2 id="invitations-heading">Pending invitations</h2>
                    </div>
                    <span class="clan-count clan-count--invites">{{ invitations.length }}</span>
                </div>

                <div v-if="invitations.length" class="clan-list">
                    <article v-for="invitation in invitations" :key="invitation.id"
                        class="clan-overview-card clan-overview-card--invitation">
                        <div class="clan-card-mark" aria-hidden="true">{{ clanInitial(invitation.clanName) }}</div>
                        <div class="clan-overview-card__info">
                            <span>Invitation from</span>
                            <h3>{{ invitation.clanName }}</h3>
                            <p>Join clan and add it to your roster.</p>
                        </div>
                        <div class="clan-overview-card__actions">
                            <button class="btn btn-primary" type="button" :disabled="actionInvitationId === invitation.id"
                                @click="acceptInvitation(invitation.id)">
                                Accept
                            </button>
                            <button class="btn btn-outline-secondary" type="button"
                                :disabled="actionInvitationId === invitation.id" @click="declineInvitation(invitation.id)">
                                Decline
                            </button>
                        </div>
                    </article>
                </div>

                <div v-else class="clan-state-card clan-state-card--compact">
                    <p>No pending invitations. New invitations will appear here.</p>
                </div>
            </section>
        </template>
    </main>
</template>

<script>
import LoadingIndicator from '@/components/LoadingIndicator.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import { authFetch } from '@/util/util';

export default {
    name: 'ClansIndex',
    components: {
        LoadingIndicator,
        NavbarElement
    },
    data() {
        return {
            clans: [],
            invitations: [],
            loading: true,
            loadError: '',
            inputVisible: false,
            newClanName: '',
            errorMessage: '',
            creatingClan: false,
            actionInvitationId: null
        };
    },
    mounted() {
        this.loadData();
    },
    methods: {
        clanInitial(name) {
            return name?.trim().charAt(0).toUpperCase() || '?';
        },
        memberLabel(memberCount) {
            return memberCount === 1 ? 'member' : 'members';
        },
        showCreateForm() {
            this.inputVisible = true;
            this.errorMessage = '';
        },
        cancelCreateClan() {
            this.inputVisible = false;
            this.newClanName = '';
            this.errorMessage = '';
        },
        async loadData() {
            this.loading = true;
            this.loadError = '';

            try {
                const [clansResponse, invitationsResponse] = await Promise.all([
                    authFetch('/api/clans/myClans', {
                        method: 'GET',
                        headers: { 'Content-Type': 'application/json' }
                    }),
                    authFetch('/api/clans/invite/pending', {
                        method: 'GET',
                        headers: { 'Content-Type': 'application/json' }
                    })
                ]);

                if (clansResponse.status === 404 || invitationsResponse.status === 404) {
                    this.$router.push({ name: 'settings' });
                    return;
                }
                if (!clansResponse.ok || !invitationsResponse.ok) {
                    throw new Error('Request failed. Please try again.');
                }

                this.clans = await clansResponse.json();
                this.invitations = await invitationsResponse.json();
            } catch (error) {
                this.loadError = error.message || 'Unknown error';
            } finally {
                this.loading = false;
            }
        },
        async createClan() {
            if (!this.newClanName || this.creatingClan) return;

            this.creatingClan = true;
            this.errorMessage = '';

            try {
                const response = await authFetch('/api/clans/create', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ name: this.newClanName })
                });

                if (response.status === 404) {
                    this.$router.push({ name: 'settings' });
                    return;
                }
                if (!response.ok) {
                    this.errorMessage = await response.text();
                    return;
                }

                this.cancelCreateClan();
                await this.loadData();
            } finally {
                this.creatingClan = false;
            }
        },
        async acceptInvitation(invitationId) {
            await this.respondToInvitation(invitationId, 'POST', 'accept');
        },
        async declineInvitation(invitationId) {
            await this.respondToInvitation(invitationId, 'DELETE', 'decline');
        },
        async respondToInvitation(invitationId, method, action) {
            if (this.actionInvitationId !== null) return;
            this.actionInvitationId = invitationId;

            try {
                const response = await authFetch(`/api/clans/invite/${invitationId}/${action}/`, { method });

                if (response.status === 404) {
                    this.$router.push({ name: 'settings' });
                    return;
                }
                if (response.ok) {
                    await this.loadData();
                } else {
                    console.error(await response.text());
                }
            } finally {
                this.actionInvitationId = null;
            }
        }
    }
};
</script>
