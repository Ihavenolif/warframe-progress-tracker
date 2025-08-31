<template>
    <div style="width: 50%; margin-top: 10%; margin-left: 25%; margin-right: 25%;">
        <form @submit.prevent="submitForm">
            <label class="form-label" for="username">Username</label>
            <input class="form-control" id="username" name="username" required="" type="text" value=""
                v-model="form.username">
            <label class="form-label" for="password">Password</label>
            <input class="form-control" id="password" name="password" required="" type="password" value=""
                v-model="form.password">

            <p style="color: red">{{ errorMessage }}</p>

            <button class="form-button" type="submit">Login</button>
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
                password: ''
            },
            errorMessage: "1234"
        };
    },
    methods: {
        async submitForm() {
            console.log(BASE_URL);

            const url = new URL(`${BASE_URL}/api/auth/login`);
            url.searchParams.append('username', this.form.username);
            url.searchParams.append('password', this.form.password);

            console.log(url);

            const response = await fetch(url, {
                method: 'POST'
            });

            if (response.ok) {
                const data = await response.json();
                this.$store.commit('setCredentials', { username: this.form.username, token: data.token });
                //window.location.href = "/";
            } else {
                this.errorMessage = await response.text();
            }
        }
    }
}
</script>

<style>
/* Full-width input fields */
input[type=text],
input[type=password] {
    width: 100%;
    padding: 12px 20px;
    margin: 8px 0;
    display: inline-block;
    border: 1px solid #ccc;
    box-sizing: border-box;
}

input[type="file"] {
    display: none;
}

.custom-file-upload {
    width: 100%;
    padding: 12px 20px;
    margin: 8px 0;
    display: inline-block;
    border: 1px solid #ccc;
    box-sizing: border-box;
    cursor: pointer;
    line-height: 30px;
}

label {
    margin-top: 8px;
    line-height: 20%;
}

select {
    width: 100%;
    padding: 12px 20px;
    margin: 8px 0;
    display: inline-block;
    border: 1px solid #ccc;
    box-sizing: border-box;
}

option {
    padding: 12px 20px;
    margin: 8px 0;
    border: 1px solid #f00;
}

/* Set a style for all buttons */
.form-button {
    font-family: Arial;
    font-size: 105%;
    background-color: var(--accent);
    color: white;
    padding: 14px 20px;
    margin: 8px 0;
    border: none;
    cursor: pointer;
    width: 100%;
}
</style>