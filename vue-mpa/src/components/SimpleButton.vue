<template>
    <button class="simple-btn">
        <slot></slot>
    </button>
</template>

<script>
export default {
    name: "SimpleButton"
}
</script>

<style>
.simple-btn {
    background-color: #007bff;
    color: white;
    border: none;
    padding: 10px 20px;
    text-align: center;
    text-decoration: none;
    display: inline-block;
    font-size: 16px;
    margin: 4px 2px;
    cursor: pointer;
}
</style>