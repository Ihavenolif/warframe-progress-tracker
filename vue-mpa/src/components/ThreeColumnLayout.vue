<template>
    <div class="three-column-layout">
        <div class="three-column-layout__side">
        </div>
        <main class="three-column-layout__main">
            <slot>Three column layout placeholder</slot>
        </main>
        <div class="three-column-layout__side">
        </div>
    </div>
</template>


<script>
export default {
    name: "ThreeColumnLayout"
}
</script>
