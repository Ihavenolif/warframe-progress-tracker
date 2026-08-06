<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <h2>{{ clanName }}</h2>

        <h3>Clan members</h3>
        <div>
            <div class="clan-container" v-for="member in clanMembers" :key="member.username">
                <div class="left">
                    <p>{{ member.username }} <font-awesome-icon icon="crown" style="color: gold;"
                            v-if="member.username == leaderName" /></p>
                    Mastery Rank: {{ member.masteryRank }}
                </div>

                <div class="button-right">
                    <a v-if="!amLeader && member.username == userData.playerName" @click="leaveClan">Leave clan</a>
                    <a v-if="amLeader && member.username != leaderName" @click="removeMember(member.username)">Remove
                        member</a>
                    <a v-if="amLeader && member.username != leaderName"
                        @click="transferLeadership(member.username)">Transfer leadership</a>
                </div>

            </div>
        </div>

        <h3>Pending invitations</h3>
        <div>
            <div class="clan-container" v-for="invitation in pendingInvitations" :key="invitation.id">
                <div class="left">
                    <p>{{ invitation.playerName }}</p>
                </div>

                <div class="button-right">
                    <a @click="cancelInvitation(invitation.id)">Cancel invitation</a>
                </div>

            </div>
        </div>

        <p v-if="pendingInvitations && pendingInvitations.length === 0">There are no pending invitations for this clan.
        </p>

        <hr>

        <div v-if="inputVisible">
            <input v-model="invitePlayerName" placeholder="Enter player name to invite" type="text" />
        </div>

        <p style="color: red">{{ errorMessage }}</p>

        <div style="text-align: right;">
            <SimpleButton @click="invitePlayer" v-if="amLeader">Invite player
            </SimpleButton>
        </div>
    </ThreeColumnLayout>

</template>

<script>
import NavbarElement from "@/components/Navbar/NavbarElement.vue";
import SimpleButton from "@/components/SimpleButton.vue";
import ThreeColumnLayout from "@/components/ThreeColumnLayout.vue";
import { authFetch } from "@/util/util";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";


export default {
    name: "ClansDetails",
    props: {
        clanName: {
            type: String,
            required: true
        }
    },
    components: {
        ThreeColumnLayout,
        NavbarElement,
        FontAwesomeIcon,
        SimpleButton
    },
    computed: {
        encodedClanName() {
            return encodeURIComponent(this.clanName);
        },
        leaderName() {
            if (this.userData && this.clanMembers) {
                const leader = this.clanMembers.find((member) => member.isLeader);
                if (leader) {
                    return leader.username;
                }
            }
            return "";
        },
        amLeader() {
            return this.userData && this.userData.playerName == this.leaderName;
        }
    },
    data() {
        return {
            clanMembers: null,
            pendingInvitations: null,
            userData: null,
            inputVisible: false,
            invitePlayerName: "",
            errorMessage: ""
        }
    },
    methods: {
        async loadData() {
            const res = await authFetch(`/api/clans/${this.encodedClanName}/members`, {
                method: "GET",
                headers: {
                    'Content-Type': 'application/json'
                }
            })

            const res2 = await authFetch("/api/auth/me", {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json'
                }
            })

            if (res.status == 404) {
                this.$router.push({ name: 'settings' });
                return;
            }

            if (res.status == 403) {
                this.$router.push({ name: 'clans' });
                return;
            }

            this.userData = await res2.json();
            this.clanMembers = await res.json();

            if (this.amLeader) {
                const res3 = await authFetch(`/api/clans/${this.encodedClanName}/pendingInvitations`, {
                    method: "GET",
                    headers: {
                        'Content-Type': 'application/json'
                    }
                })

                this.pendingInvitations = await res3.json();
            } else {
                this.pendingInvitations = null;
            }
        },
        async invitePlayer() {
            if (!this.inputVisible) this.inputVisible = true;

            if (!this.invitePlayerName) return;

            const res = await authFetch(`/api/clans/${this.encodedClanName}/invite`, {
                method: "PUT",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username: this.invitePlayerName })
            })

            if (res.ok) {
                this.inputVisible = false;
                this.invitePlayerName = "";
                await this.loadData();
                return;
            }

            const error = await res.text();

            if (res.status == 404 && error == "Player not found") {
                this.$router.push({ name: 'settings' });
                return;
            }

            this.errorMessage = error;
        },
        async cancelInvitation(invitationId) {
            await authFetch(`/api/clans/invite/${invitationId}/cancel`, {
                method: "DELETE",
                headers: {
                    'Content-Type': 'application/json'
                }
            })

            await this.loadData();
        },
        async removeMember(username) {
            if (prompt("Please confirm the username of the player you want to remove:") !== username) {
                alert("Username did not match. Aborting.");
                return;
            }

            await authFetch(`/api/clans/${this.encodedClanName}/removePlayer`, {
                method: "DELETE",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username: username })
            })

            await this.loadData();
        },
        async transferLeadership(username) {
            await authFetch(`/api/clans/${this.encodedClanName}/changeLeader`, {
                method: "POST",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username: username })
            })

            await this.loadData();
        },
        async leaveClan() {
            if (!confirm("Are you sure you want to leave the clan?")) return;

            await authFetch(`/api/clans/${this.encodedClanName}/leave`, {
                method: "DELETE",
                headers: {
                    'Content-Type': 'application/json'
                }
            })

            this.$router.push({ name: 'clans' });
        }
    },
    mounted() {
        this.loadData();
    }
}

</script>

<style>
div.clan-container {
    padding: 20px;
    border: #ccc 1px solid;
    border-bottom: none;
    background-color: #f2f2f2;
    height: calc(40px + 40px);
    min-height: 20px;
    display: block;

    div.left {
        float: left;
        display: block;
        width: fit-content;
    }

    p {
        font-weight: bold;
        margin: 0;
        width: fit-content;
    }

    button,
    a {
        display: block;
        float: right;
        top: 10px;
        padding: 10px;
        border: #ccc 1px solid;
        cursor: pointer;
        text-decoration: none;
        color: black;
    }

    div.button-right {
        display: block;
        float: right;
    }
}

div.clan-container:last-child {
    border-bottom: 1px solid #ccc;
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
