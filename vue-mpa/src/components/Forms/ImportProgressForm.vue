<template>
    <div style="width: 50%; margin-top: 10%; margin-left: 25%; margin-right: 25%;">
        <form @submit.prevent="submitForm">
            <!--file upload form-->
            <label class="form-label" for="file">Select file to upload:</label>
            <input class="form-control" id="file" name="file" required="" type="file" v-on:change="form.file = $event.target.files[0]">
            
            <p style="color: red">{{ errorMessage }}</p>

            <button class="form-button" type="submit">Upload</button>
        </form>    
    </div>
</template>


<script>
import { BASE_URL } from '@/util/util';

export default {
    computed: {
        token(){
            return this.$store.state.token;
        }
    },
    data(){
        return {
            form: {
                file: null
            },
            errorMessage: ""
        }
    },
    methods: {
        async submitForm() {
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

            if(response.ok){
                window.location.href = "/progress";
            } else {
                this.errorMessage = await response.text();
            }
        }
    }
}    
</script>

<style>
/* Full-width input fields */
/* For file form */
input[type=file] {
    width: 100%;
    padding: 12px 20px;
    margin: 8px 0;
    display: inline-block;
    border: 1px solid #ccc;
    box-sizing: border-box;
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