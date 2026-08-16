<template>
    <div class="auth-form-shell">
        <form @submit.prevent="submitForm">
            <!--file upload form-->
            <label class="form-label" for="file">Select file to upload:</label>
            <input class="form-control" id="file" name="file" required="" type="file" accept=".json,application/json"
                v-on:change="form.file = $event.target.files[0]">

            <LoadingIndicator v-if="responseLoading" />

            <p v-if="errorMessage" class="text-danger" role="alert">{{ errorMessage }}</p>

            <button class="btn btn-primary btn-block" type="submit" :disabled="responseLoading">
                {{ responseLoading ? 'Uploading...' : 'Upload' }}
            </button>
        </form>

        <section v-if="receipt" class="import-summary" aria-live="polite">
            <p class="eyebrow">Import complete</p>
            <h2>{{ receipt.changed ? 'Progress updated' : 'Profile already current' }}</h2>
            <p>
                Processed <strong>{{ receipt.processedCount.toLocaleString() }}</strong> records.
                Mastery rank {{ receipt.resultingMasteryRank }},
                {{ receipt.resultingTotalMasteryXp.toLocaleString() }} total mastery XP.
            </p>
            <p v-if="receipt.skippedCount > 0" class="import-warning" role="alert">
                {{ receipt.skippedCount.toLocaleString() }} records skipped because catalog data did not recognize them.
            </p>
            <p v-if="!receipt.sourceVersion" class="import-warning">
                Parser version missing. Download latest parser before next import.
            </p>
            <RouterLink class="btn btn-secondary" to="/progress">View progress</RouterLink>
        </section>
    </div>
</template>


<script>
import { authFetch } from '@/util/util';
import LoadingIndicator from '../LoadingIndicator.vue';

export default {
    data() {
        return {
            form: {
                file: null
            },
            errorMessage: '',
            responseLoading: false,
            receipt: null
        };
    },
    methods: {
        async submitForm() {
            if (this.responseLoading) return;

            this.responseLoading = true;
            this.errorMessage = '';
            this.receipt = null;

            const formData = new FormData();
            formData.append('jsonFile', this.form.file);
            try {
                const response = await authFetch('/api/mastery/update', {
                    method: 'POST',
                    body: formData
                });

                if (!response) return;
                if (response.ok) {
                    this.receipt = await response.json();
                } else {
                    this.errorMessage = await response.text();
                }
            } catch {
                this.errorMessage = 'Import failed because server could not be reached.';
            } finally {
                this.responseLoading = false;
            }
        }
    },
    components: {
        LoadingIndicator
    }
};
</script>
