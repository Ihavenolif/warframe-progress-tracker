<template>
    <div :class="{
        'dropdown': true,
        'right-aligned': this.isRightAligned
    }" ref="dropdown">
        <button class="dropbtn">
            {{ title }}
            <i class="fa fa-caret-down"></i>
        </button>

        <div :class="{
            'dropdown-content': true,
            'right-aligned': this.isRightAligned
        }" ref="dropdownContent">
            <slot></slot>
        </div>
    </div>


</template>

<script>
export default {
    name: 'NavbarDropdown',
    components: {

    },
    props: {
        title: {
            type: String,
            required: true
        },
        isRightAligned: {
            type: Boolean,
            default: false
        }
    },
    data() {
        return {

        };
    },
    mounted() {
        const rect = this.$refs.dropdown.getBoundingClientRect();
        if (this.isRightAligned) {
            this.$refs.dropdownContent.style.right = window.innerWidth - rect.right + "px";
        } else {
            this.$refs.dropdownContent.style.left = rect.left + "px";
        }
    },
    methods: {

    }
}
</script>