<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <h2>Warframe</h2>
        <LoadingIndicator v-if="loading" />
        <p>{{ updateResultMessage }}</p>
        <SimpleButton @click="updateWarframeData">Update Warframe database data</SimpleButton>
    </ThreeColumnLayout>
</template>

<script>
import LoadingIndicator from '@/components/LoadingIndicator.vue';
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import SimpleButton from '@/components/SimpleButton.vue';
import ThreeColumnLayout from '@/components/ThreeColumnLayout.vue';
import { authFetch } from '@/util/util';

export default {
    name: 'AdminPanel',
    components: {
        NavbarElement,
        ThreeColumnLayout,
        SimpleButton,
        LoadingIndicator
    },
    methods: {
        async updateWarframeData() {
            this.loading = true
            const res = await authFetch("/api/items/updateDatabase", {
                method: "POST"
            })

            if (res.status == 401 || res.status == 403) {
                window.location.href = "/"
                return
            }

            const data = await res.json()
            this.updateResultMessage = data.message
            this.loading = false
        }
    },
    data() {
        return {
            updateResultMessage: "",
            loading: false

        }
    }
}
</script>