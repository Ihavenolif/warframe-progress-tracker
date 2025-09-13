<template>
    <div class="progress-container">
        <div class="progress-bar" id="progressBar" :style="{ width: `${progressPercent}%` }">
            <slot></slot>
        </div>
    </div>
</template>

<script>
export default {
    props: {
        progressPercent: {
            type: Number,
            required: true
        }
    },
    name: "ProgressBarElement"
}
</script>

<style>
.progress-container {
    width: 100%;
    background-color: rgb(238, 238, 119);
    ;
    overflow: hidden;
    padding: 0px;
    margin: 0px;
}

.progress-bar {
    height: 100%;
    width: 0;
    background-color: rgb(92, 233, 92);
    padding: 0px;
    padding-left: 10px;
    margin: 0px;
    line-height: 38px !important;
    text-overflow: ellipsis;
    white-space: nowrap;
}
</style>