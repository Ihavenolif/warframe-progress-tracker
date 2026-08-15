<template>
    <div class="auth-form-shell">
        <form @submit.prevent="submitForm">
            <label class="form-label" for="username">Username</label>
            <input class="form-control" id="username" name="username" required="" type="text" value=""
                v-model="form.username">
            <label class="form-label" for="password">Password</label>
            <input class="form-control" id="password" name="password" required="" type="password" value=""
                v-model="form.password">
            <label class="form-label" for="password">Repeat password</label>
            <input class="form-control" id="password-repeat" name="password-repeat" required="" type="password" value=""
                v-model="form.repeatPassword">

            <p class="text-danger">{{ errorMessage }}</p>

            <button class="btn btn-primary btn-block" type="submit">Register</button>
        </form>
    </div>
</template>

<script>
import { BASE_URL } from "@/util/util.js"

export default {
    data() {
        return {
            form: {
                username: '',
                password: '',
                repeatPassword: ''
            },
            errorMessage: ""
        }
    },
    methods: {
        async submitForm() {
            if (this.form.password !== this.form.repeatPassword) {
                this.errorMessage = "Passwords do not match!";
                return;
            }

            const url = new URL(`${BASE_URL}/api/auth/register`);
            url.searchParams.append('username', this.form.username);
            url.searchParams.append('password', this.form.password);

            const response = await fetch(url, {
                method: 'POST'
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
}
</script>
