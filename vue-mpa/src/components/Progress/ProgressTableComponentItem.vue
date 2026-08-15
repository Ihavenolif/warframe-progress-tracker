<template>
    <div @mousemove="updatePosition" @mouseleave="tooltipVisible = false">
        <img :src="item['imgSrc']" :alt="item['name']" height="34px" width="34px" @mouseover="tooltipVisible = true">

        <div v-if="tooltipVisible" class="progress-tooltip" :style="tooltipProperties" ref="tooltip">
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
            tooltipTop: "0px",
            tooltipLeft: "0px"
        }
    },
    computed: {
        tooltipProperties() {
            return {
                '--tooltip-top': this.tooltipTop,
                '--tooltip-left': this.tooltipLeft
            };
        }
    },
    methods: {
        updatePosition(event) {
            const tooltipHeight = this.$refs.tooltip?.offsetHeight || 20;

            this.tooltipTop = `${event.clientY - tooltipHeight}px`;
            this.tooltipLeft = `${event.clientX}px`;
        }
    }
}
</script>
