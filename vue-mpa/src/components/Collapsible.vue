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

<style>
/* Style the button that is used to open and close the collapsible content */
.collapsible {
    background-color: #eee;
    color: #444;
    cursor: pointer;
    padding: 18px;
    width: 100%;
    border: none;
    text-align: left;
    outline: none;
    font-size: 15px;
}

/* Add a background color to the button if it is clicked on (add the .active class with JS), and when you move the mouse over it (hover) */
.collapsible-active,
.collapsible:hover {
    background-color: #ccc;
}

/* Style the collapsible content. Note: hidden by default */
.collapsible-content {
    padding: 18px;
    overflow: hidden;
    background-color: #f5f5f5;
    padding: 18px;
    border: 1px solid #ddd;
    transition: max-height 0.4s ease-out;
    /* Add transition for smooth opening/closing */
}

.collapsible:after {
    content: '\02795';
    /* Unicode character for "plus" sign (+) */
    font-size: 13px;
    color: white;
    float: right;
    margin-left: 5px;
}

.collapsible-active:after {
    content: "\2796";
    /* Unicode character for "minus" sign (-) */
}
</style>
