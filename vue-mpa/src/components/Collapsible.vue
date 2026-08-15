<template>
    <button type="button" class="collapsible" :class="{ 'collapsible-active': expanded }" @click="expanded = !expanded"><strong>{{ title }}</strong></button>
    <div class="collapsible-content" v-show="expanded">
        <slot>Collapsible placeholder slot</slot>
    </div>
</template>

<script>
export default {
    name: "CollapsibleContainer",
    props: {
        title: {
            type: String,
            required: true
        }
    },
    data() {
        return {
            expanded: false
        }
    }
}
</script>
