<template>
    <div class="spinner"></div>
</template>

<style>
.spinner {
    border: 8px solid #f3f3f3;
    /* Light grey */
    border-top: 8px solid var(--accent);
    /* Blue */
    border-radius: 50%;
    width: 60px;
    height: 60px;
    animation: spin 1s linear infinite;
    margin: 50px auto;
    /* Center horizontally */
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }

    100% {
        transform: rotate(360deg);
    }
}
</style>

<script>
export default {
    name: "LoadingIndicator"
}
</script>