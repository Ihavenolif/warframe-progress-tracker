<template>
    <div class="progress-container mastery-state-1">
        <div class="progress-bar mastery-state-0" :style="{ '--progress-percent': `${progressPercent}%` }">
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
