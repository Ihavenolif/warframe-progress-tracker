<template>
    <NavbarElement></NavbarElement>

    <ThreeColumnLayout>
        <h2>Warframe</h2>
        <SimpleButton @click="updateWarframeData">Update Warframe database data</SimpleButton>
        <p>{{ updateResultMessage }}</p>
    </ThreeColumnLayout>
</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';
import SimpleButton from '@/components/SimpleButton.vue';
import ThreeColumnLayout from '@/components/ThreeColumnLayout.vue';
import { authFetch } from '@/util/util';

export default {
    name: 'AdminPanel',
    components: {
        NavbarElement,
        ThreeColumnLayout,
        SimpleButton
    },
    methods: {
        async updateWarframeData() {
            const res = await authFetch("/api/items/updateDatabase", {
                method: "POST"
            })

            if (res.status == 401 || res.status == 403) {
                window.location.href = "/"
                return
            }

            const data = await res.json()
            this.updateResultMessage = data.message
        }
    },
    data() {
        return {
            updateResultMessage: ""
        }
    }
}
</script>