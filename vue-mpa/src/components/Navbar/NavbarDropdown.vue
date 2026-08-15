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
        }" :style="dropdownProperties">
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
            dropdownOffset: '0px'
        };
    },
    computed: {
        dropdownProperties() {
            return this.isRightAligned
                ? { '--dropdown-right': this.dropdownOffset }
                : { '--dropdown-left': this.dropdownOffset };
        }
    },
    mounted() {
        const rect = this.$refs.dropdown.getBoundingClientRect();
        if (this.isRightAligned) {
            this.dropdownOffset = window.innerWidth - rect.right + "px";
        } else {
            this.dropdownOffset = rect.left + "px";
        }
    },
    methods: {

    }
}
</script>
