<template>
    <img :src="item['imgSrc']" :alt="item['name']" height="34px" width="34px">
</template>

<script>
export default {
    name: "ProgressTableComponentItem",
    props: {
        item: {
            required: true
        }
    }
}
</script>