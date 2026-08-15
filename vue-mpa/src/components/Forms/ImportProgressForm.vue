<template>
    <div class="auth-form-shell">
        <form @submit.prevent="submitForm">
            <!--file upload form-->
            <label class="form-label" for="file">Select file to upload:</label>
            <input class="form-control" id="file" name="file" required="" type="file"
                v-on:change="form.file = $event.target.files[0]">

            <LoadingIndicator v-if="responseLoading" />

            <p class="text-danger">{{ errorMessage }}</p>

            <button class="btn btn-primary btn-block" type="submit" :disabled="responseLoading">Upload</button>
        </form>
    </div>
</template>


<script>
import { BASE_URL } from '@/util/util';
import LoadingIndicator from '../LoadingIndicator.vue';

export default {
    computed: {
        token() {
            return this.$store.state.token;
        }
    },
    data() {
        return {
            form: {
                file: null
            },
            errorMessage: "",
            responseLoading: false
        }
    },
    methods: {
        async submitForm() {
            if (this.responseLoading) return;

            this.responseLoading = true;
            const url = new URL(`${BASE_URL}/api/mastery/update`);

            const formData = new FormData();
            formData.append('jsonFile', this.form.file);
            const response = await fetch(url, {
                method: 'POST',
                body: formData,
                headers: {
                    "Authorization": `Bearer ${this.token}`
                }
            });

            this.responseLoading = false;

            if (response.ok) {
                this.$router.push({ name: 'progress' });
            } else {
                this.errorMessage = await response.text();
            }
        }
    },
    components: {
        LoadingIndicator
    }
}    
</script>
