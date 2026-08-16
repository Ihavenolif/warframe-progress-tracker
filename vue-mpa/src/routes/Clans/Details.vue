<template>
    <NavbarElement />

    <main class="content-page clan-management-page">
        <header class="page-heading">
            <div>
                <p class="eyebrow">Clan management</p>
                <h1>{{ clanName }}</h1>
                <p v-if="!loading && !loadError">
                    <strong>{{ clanMembers.length }}</strong> {{ memberLabel(clanMembers.length) }} | Led by {{ leaderName }}
                </p>
                <p v-else>Review roster, roles, and invitations.</p>
            </div>
            <div class="clan-page-actions">
                <RouterLink class="btn btn-outline-secondary" :to="{ name: 'clans' }">All clans</RouterLink>
                <RouterLink class="btn btn-secondary" :to="{ name: 'clan-progress', params: { clanName } }">
                    View progress
                </RouterLink>
            </div>
        </header>

        <LoadingIndicator v-if="loading" />

        <section v-else-if="loadError" class="clan-state-card clan-state-card--error">
            <p class="eyebrow">Connection interrupted</p>
            <h2>Clan could not be loaded</h2>
            <p>{{ loadError }}</p>
            <button class="btn btn-secondary" type="button" @click="loadData">Try again</button>
        </section>

        <template v-else>
            <form v-if="amLeader && inputVisible" class="clan-invite-panel" @submit.prevent="invitePlayer">
                <div>
                    <p class="eyebrow">Recruitment</p>
                    <h2>Invite player</h2>
                    <p>Send clan invitation using exact in-game player name.</p>
                </div>
                <label for="invite-player-name">
                    <span>Player name</span>
                    <input id="invite-player-name" v-model.trim="invitePlayerName" type="text"
                        placeholder="Enter player name" autocomplete="off" autofocus>
                </label>
                <div class="clan-invite-actions">
                    <button class="btn btn-outline-secondary" type="button" @click="cancelInvite">Cancel</button>
                    <button class="btn btn-primary" type="submit" :disabled="!invitePlayerName || busyAction !== ''">
                        {{ busyAction === 'invite' ? 'Sending...' : 'Send invitation' }}
                    </button>
                </div>
                <p v-if="errorMessage" class="clan-form-error" role="alert">{{ errorMessage }}</p>
            </form>

            <p v-if="operationError" class="clan-operation-error" role="alert">{{ operationError }}</p>

            <section class="clan-section" aria-labelledby="members-heading">
                <div class="clan-section-heading">
                    <div>
                        <p class="eyebrow">Roster</p>
                        <h2 id="members-heading">Clan members</h2>
                    </div>
                    <div class="clan-section-tools">
                        <span class="clan-count">{{ clanMembers.length }}</span>
                        <button v-if="amLeader && !inputVisible" class="btn btn-primary" type="button" @click="showInviteForm">
                            Invite player
                        </button>
                    </div>
                </div>

                <div class="clan-member-list">
                    <article v-for="member in clanMembers" :key="member.username" class="clan-member-card"
                        :class="{ 'clan-member-card--leader': member.isLeader }">
                        <div class="clan-member-mark" aria-hidden="true">{{ memberInitial(member.username) }}</div>
                        <div class="clan-member-info">
                            <div class="clan-member-name">
                                <h3>{{ member.username }}</h3>
                                <span v-if="isCurrentUser(member)" class="clan-member-badge">You</span>
                                <span v-if="member.isLeader" class="clan-member-badge clan-member-badge--leader">
                                    <font-awesome-icon icon="crown" /> Leader
                                </span>
                            </div>
                            <p>
                                Mastery Rank
                                <strong v-if="member.masteryRank > 30" class="legendary-rank"
                                    :aria-label="`Legendary Rank ${member.masteryRank - 30}`">
                                    <img :src="legendaryRankIcon" alt="" aria-hidden="true">
                                    {{ member.masteryRank - 30 }}
                                </strong>
                                <strong v-else>{{ member.masteryRank }}</strong>
                            </p>
                        </div>
                        <div v-if="canManageMember(member)" class="clan-member-actions">
                            <button class="btn btn-outline-secondary" type="button" :disabled="busyAction !== ''"
                                @click="transferLeadership(member.username)">
                                Transfer leadership
                            </button>
                            <button class="btn clan-danger-button" type="button" :disabled="busyAction !== ''"
                                @click="removeMember(member.username)">
                                Remove
                            </button>
                        </div>
                        <div v-else-if="!amLeader && isCurrentUser(member)" class="clan-member-actions">
                            <button class="btn clan-danger-button" type="button" :disabled="busyAction !== ''" @click="leaveClan">
                                Leave clan
                            </button>
                        </div>
                    </article>
                </div>
            </section>

            <section v-if="amLeader" class="clan-section" aria-labelledby="sent-invitations-heading">
                <div class="clan-section-heading">
                    <div>
                        <p class="eyebrow">Recruitment</p>
                        <h2 id="sent-invitations-heading">Sent invitations</h2>
                    </div>
                    <span class="clan-count clan-count--invites">{{ pendingInvitations.length }}</span>
                </div>

                <div v-if="pendingInvitations.length" class="clan-member-list">
                    <article v-for="invitation in pendingInvitations" :key="invitation.id"
                        class="clan-member-card clan-member-card--invitation">
                        <div class="clan-member-mark" aria-hidden="true">{{ memberInitial(invitation.playerName) }}</div>
                        <div class="clan-member-info">
                            <span>Awaiting response</span>
                            <h3>{{ invitation.playerName }}</h3>
                            <p>Invitation remains pending.</p>
                        </div>
                        <div class="clan-member-actions">
                            <button class="btn btn-outline-secondary" type="button" :disabled="busyAction !== ''"
                                @click="cancelInvitation(invitation.id)">
                                Cancel invitation
                            </button>
                        </div>
                    </article>
                </div>

                <div v-else class="clan-state-card clan-state-card--compact">
                    <p>No pending invitations. Invite player to grow clan roster.</p>
                </div>
            </section>
        </template>
    </main>
</template>

<script>
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import legendaryRankIcon from '@/assets/legendary-rank.png';
import LoadingIndicator from '@/components/LoadingIndicator.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import { authFetch } from '@/util/util';

export default {
    name: 'ClansDetails',
    components: {
        FontAwesomeIcon,
        LoadingIndicator,
        NavbarElement
    },
    props: {
        clanName: {
            type: String,
            required: true
        }
    },
    data() {
        return {
            clanMembers: [],
            pendingInvitations: [],
            userData: null,
            loading: true,
            loadError: '',
            inputVisible: false,
            invitePlayerName: '',
            errorMessage: '',
            operationError: '',
            busyAction: '',
            legendaryRankIcon
        };
    },
    computed: {
        encodedClanName() {
            return encodeURIComponent(this.clanName);
        },
        leaderName() {
            return this.clanMembers.find(member => member.isLeader)?.username || '';
        },
        amLeader() {
            return this.userData?.playerName === this.leaderName;
        }
    },
    mounted() {
        this.loadData();
    },
    methods: {
        memberInitial(username) {
            return username?.trim().charAt(0).toUpperCase() || '?';
        },
        memberLabel(memberCount) {
            return memberCount === 1 ? 'member' : 'members';
        },
        isCurrentUser(member) {
            return member.username === this.userData?.playerName;
        },
        canManageMember(member) {
            return this.amLeader && !member.isLeader;
        },
        showInviteForm() {
            this.inputVisible = true;
            this.errorMessage = '';
        },
        cancelInvite() {
            this.inputVisible = false;
            this.invitePlayerName = '';
            this.errorMessage = '';
        },
        async loadData() {
            this.loading = true;
            this.loadError = '';

            try {
                const [membersResponse, userResponse] = await Promise.all([
                    authFetch(`/api/clans/${this.encodedClanName}/members`, {
                        method: 'GET',
                        headers: { 'Content-Type': 'application/json' }
                    }),
                    authFetch('/api/auth/me', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' }
                    })
                ]);

                if (membersResponse.status === 404) {
                    this.$router.push({ name: 'settings' });
                    return;
                }
                if (membersResponse.status === 403) {
                    this.$router.push({ name: 'clans' });
                    return;
                }
                if (!membersResponse.ok || !userResponse.ok) throw new Error('Request failed. Please try again.');

                this.userData = await userResponse.json();
                this.clanMembers = await membersResponse.json();

                if (this.amLeader) {
                    const invitationsResponse = await authFetch(`/api/clans/${this.encodedClanName}/pendingInvitations`, {
                        method: 'GET',
                        headers: { 'Content-Type': 'application/json' }
                    });
                    if (!invitationsResponse.ok) throw new Error('Pending invitations could not be loaded.');
                    this.pendingInvitations = await invitationsResponse.json();
                } else {
                    this.pendingInvitations = [];
                }
            } catch (error) {
                this.loadError = error.message || 'Unknown error';
            } finally {
                this.loading = false;
            }
        },
        async invitePlayer() {
            if (!this.invitePlayerName || this.busyAction) return;
            this.busyAction = 'invite';
            this.errorMessage = '';

            try {
                const response = await authFetch(`/api/clans/${this.encodedClanName}/invite`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ username: this.invitePlayerName })
                });

                if (!response.ok) {
                    this.errorMessage = await response.text();
                    return;
                }

                this.cancelInvite();
                await this.loadData();
            } catch (error) {
                this.errorMessage = error.message || 'Invitation could not be sent.';
            } finally {
                this.busyAction = '';
            }
        },
        async cancelInvitation(invitationId) {
            await this.runClanAction('cancel-invitation', `/api/clans/invite/${invitationId}/cancel`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' }
            });
        },
        async removeMember(username) {
            if (prompt('Please confirm the username of the player you want to remove:') !== username) {
                alert('Username did not match. Aborting.');
                return;
            }

            await this.runClanAction('remove-member', `/api/clans/${this.encodedClanName}/removePlayer`, {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username })
            });
        },
        async transferLeadership(username) {
            await this.runClanAction('transfer-leadership', `/api/clans/${this.encodedClanName}/changeLeader`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ username })
            });
        },
        async runClanAction(action, url, options) {
            if (this.busyAction) return;
            this.busyAction = action;
            this.operationError = '';

            try {
                const response = await authFetch(url, options);
                if (!response.ok) {
                    this.operationError = (await response.text()) || 'Clan action failed.';
                    return;
                }
                await this.loadData();
            } catch (error) {
                this.operationError = error.message || 'Clan action failed.';
            } finally {
                this.busyAction = '';
            }
        },
        async leaveClan() {
            if (!confirm('Are you sure you want to leave the clan?')) return;
            if (this.busyAction) return;
            this.busyAction = 'leave-clan';
            this.operationError = '';

            try {
                const response = await authFetch(`/api/clans/${this.encodedClanName}/leave`, {
                    method: 'DELETE',
                    headers: { 'Content-Type': 'application/json' }
                });
                if (!response.ok) {
                    this.operationError = (await response.text()) || 'Clan could not be left.';
                    return;
                }
                this.$router.push({ name: 'clans' });
            } catch (error) {
                this.operationError = error.message || 'Clan could not be left.';
            } finally {
                this.busyAction = '';
            }
        }
    }
};
</script>
