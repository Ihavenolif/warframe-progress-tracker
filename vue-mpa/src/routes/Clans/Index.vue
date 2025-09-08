<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <h2>Your Clans</h2>

        <div>
            <div class="clan-container" v-for="clan in clans" :key="clan.id">
                <div class="left">
                    <p>{{ clan["name"] }}</p>
                    {{ clan["memberCount"] }} members
                </div>


                <div class="button-right">
                    <a :href="`/clans/${clan['name']}/progress`">View progress</a>
                </div>

                <div class="button-right">
                    <a :href="`/clans/${clan['name']}`">Details</a>
                </div>

            </div>
        </div>

        <p v-if="clans && clans.length == 0">You are not part of any clan.</p>

        <hr>

        <div v-if="inputVisible">

            <input v-model="newClanName" placeholder="Enter new clan name" type="text" />
            <p style="color: red">{{ errorMessage }}</p>
        </div>


        <div style="text-align: right;">
            <SimpleButton @click="createClan">Create clan</SimpleButton>
        </div>
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
            inputVisible: false,
            newClanName: "",
            errorMessage: ""
        }
    },
    async mounted() {
        const res = await authFetch("/api/clans/myClans", {
            method: "GET",
            headers: {
                'Content-Type': 'application/json'
            }
        })

        if (res.status == 404) {
            window.location.href = "/settings"
        }

        this.clans = await res.json();
    },
    methods: {
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
                window.location.href = "/settings"
            }
            if (res.ok) {
                window.location.reload();
            } else {
                this.errorMessage = await res.text();
            }
        }
    }
};
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