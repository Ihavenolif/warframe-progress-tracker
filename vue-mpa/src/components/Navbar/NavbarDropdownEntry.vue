<template>
    <RouterLink :to="href" :class="{ 'active-inverted': isActive }">
        <slot></slot>
    </RouterLink>
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
            return this.$route.path == this.href || this.$route.path == (this.href + "/");
        }
    }
}
</script>
