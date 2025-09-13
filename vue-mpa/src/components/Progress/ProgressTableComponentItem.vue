<template>
    <div @mousemove="updatePosition" @mouseleave="tooltipVisible = false">
        <img :src="item['imgSrc']" :alt="item['name']" height="34px" width="34px" @mouseover="tooltipVisible = true">

        <div v-if="tooltipVisible" class="tooltip" :style="tooltipStyle" ref="tooltip">
            {{ item['name'] }}<br>

            <span v-if="!item['name'].includes('Blueprint')">
                <br>
                <br>
                {{ item['countOwned'] }} / {{ item['countRequired'] }}
            </span>

            <span v-if="item['blueprintOwned']">
                <br>
                <br>
                Blueprint owned
            </span>

        </div>
    </div>
</template>

<script>
export default {
    name: "ProgressTableComponentItem",
    props: {
        item: {
            required: true
        }
    },
    data() {
        return {
            tooltipVisible: false,
            tooltipStyle: {
                position: "absolute",
                top: "0px",
                left: "0px"
            }
        }
    },
    methods: {
        updatePosition(event) {
            const tooltipHeight = this.$refs.tooltip?.offsetHeight || 20;

            this.tooltipStyle.top = `${event.clientY - tooltipHeight}px`;
            this.tooltipStyle.left = `${event.clientX}px`;
        }
    }
}
</script>

<style>
.tooltip-container {
    position: relative;
    margin: 100px;


}

.tooltip {
    position: absolute;
    background: rgba(255, 255, 255, 0.6);
    border: 1px solid rgba(30, 30, 30, 0.15);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
    border-radius: 8px;
    color: #333;
    padding: 12px 12px;
    white-space: nowrap;
    z-index: 10;

    backdrop-filter: blur(6px);
    -webkit-backdrop-filter: blur(6px);

    transition: opacity 0.2s ease, transform 0.2s ease;
}
</style>