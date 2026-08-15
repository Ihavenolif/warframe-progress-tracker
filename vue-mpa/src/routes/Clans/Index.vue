<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <section class="clan-page">
        <h2>Your Clans</h2>

        <div>
            <div class="clan-card" v-for="clan in clans" :key="clan.id">
                <div class="clan-card__info">
                    <p>{{ clan["name"] }}</p>
                    {{ clan["memberCount"] }} members
                </div>


                <div class="clan-card__actions">
                    <RouterLink :to="{ name: 'clan-progress', params: { clanName: clan.name } }">View progress</RouterLink>
                </div>

                <div class="clan-card__actions">
                    <RouterLink :to="{ name: 'clan-details', params: { clanName: clan.name } }">Details</RouterLink>
                </div>

            </div>
        </div>

        <p v-if="clans && clans.length == 0">You are not part of any clan.</p>

        <h2>Pending invitations</h2>
        <div>
            <div class="clan-card" v-for="invitation in invitations" :key="invitation.id">
                <div class="clan-card__info">
                    <p>{{ invitation["clanName"] }}</p>
                </div>


                <div class="clan-card__actions">
                    <a @click="acceptInvitation(invitation.id)">Accept</a>
                </div>
                <div class="clan-card__actions">
                    <a @click="declineInvitation(invitation.id)">Decline</a>
                </div>

            </div>
        </div>

        <p v-if="invitations && invitations.length == 0">You have no pending invitations.</p>

        <hr>

        <div v-if="inputVisible">

            <input v-model="newClanName" placeholder="Enter new clan name" type="text" />
            <p class="text-danger">{{ errorMessage }}</p>
        </div>


        <div class="text-end">
            <SimpleButton @click="createClan">Create clan</SimpleButton>
        </div>
        </section>
    </ThreeColumnLayout>

</template>

<script>
import NavbarElement from "@/components/Navbar/NavbarElement.vue";
import SimpleButton from "@/components/SimpleButton.vue";
import ThreeColumnLayout from "@/components/ThreeColumnLayout.vue";
import { authFetch } from "@/util/util";

export default {
    name: "ClansIndex",
    components: {
        NavbarElement,
        ThreeColumnLayout,
        SimpleButton
    },
    data() {
        return {
            clans: null,
            invitations: null,
            inputVisible: false,
            newClanName: "",
            errorMessage: ""
        }
    },
    mounted() {
        this.loadData();
    },
    methods: {
        async loadData() {
            const res = await authFetch("/api/clans/myClans", {
                method: "GET",
                headers: {
                    'Content-Type': 'application/json'
                }
            })

            const res2 = await authFetch("/api/clans/invite/pending", {
                method: "GET",
                headers: {
                    'Content-Type': 'application/json'
                }
            })

            if (res.status == 404 || res2.status == 404) {
                this.$router.push({ name: 'settings' });
                return;
            }

            this.clans = await res.json();
            this.invitations = await res2.json();
        },
        async createClan() {
            if (!this.inputVisible) {
                this.inputVisible = true;
                return;
            }

            if (!this.newClanName) return;

            const res = await authFetch("/api/clans/create", {
                method: "PUT",
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    name: this.newClanName
                })
            })

            if (res.status == 404) {
                this.$router.push({ name: 'settings' });
                return;
            }
            if (res.ok) {
                this.inputVisible = false;
                this.newClanName = "";
                await this.loadData();
            } else {
                this.errorMessage = await res.text();
            }
        },
        async acceptInvitation(invitationId) {
            const res = await authFetch(`/api/clans/invite/${invitationId}/accept/`, {
                method: "POST"
            })

            if (res.status == 404) {
                this.$router.push({ name: 'settings' });
                return;
            }
            if (res.ok) {
                await this.loadData();
            } else {
                console.error(await res.text());
            }
        },
        async declineInvitation(invitationId) {
            const res = await authFetch(`/api/clans/invite/${invitationId}/decline/`, {
                method: "DELETE"
            })

            if (res.status == 404) {
                this.$router.push({ name: 'settings' });
                return;
            }
            if (res.ok) {
                await this.loadData();
            } else {
                console.error(await res.text());
            }
        }
    }
};
</script>
