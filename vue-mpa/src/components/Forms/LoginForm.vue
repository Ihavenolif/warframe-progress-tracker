<template>
    <div class="auth-form-shell">
        <form @submit.prevent="submitForm">
            <label class="form-label" for="username">Username</label>
            <input class="form-control" id="username" name="username" required="" type="text" value=""
                v-model="form.username">
            <label class="form-label" for="password">Password</label>
            <input class="form-control" id="password" name="password" required="" type="password" value=""
                v-model="form.password">

            <p class="text-danger">{{ errorMessage }}</p>

            <button class="btn btn-primary btn-block" type="submit">Login</button>
        </form>
    </div>
</template>

<script>
import { BASE_URL } from '@/util/util.js';

export default {
    data() {
        return {
            form: {
                username: '',
                password: ''
            },
            errorMessage: ''
        };
    },
    methods: {
        async submitForm() {
            const response = await fetch(`${BASE_URL}/api/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                credentials: 'include',
                body: JSON.stringify(this.form)
            });

            if (response.ok) {
                const data = await response.json();
                this.$store.commit('setCredentials', { username: this.form.username, token: data.token });
                this.$router.push({ name: 'home' });
            } else {
                this.errorMessage = await response.text();
            }
        }
    }
};
</script>
