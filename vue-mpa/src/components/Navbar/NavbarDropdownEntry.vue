<template>
    <a :href="href" :class="{ 'active-inverted': isActive }">
        <slot></slot>
    </a>
</template>

<script>
export default {
    name: 'NavbarDropdownEntry',
    props: {
        href: {
            type: String,
            required: true
        }
    },
    computed: {
        isActive() {
            return window.location.pathname == this.href || window.location.pathname == (this.href + "/");
        }
    }
}
</script>