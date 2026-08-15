<template>
    <div @mousemove="updatePosition" @mouseleave="tooltipVisible = false">
        <img :src="item['imgSrc']" :alt="item['name']" height="34px" width="34px" @mouseenter="showTooltip">

        <Teleport to="body">
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
        </Teleport>
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
        showTooltip(event) {
            this.tooltipVisible = true;
            this.$nextTick(() => this.updatePosition(event));
        },
        updatePosition(event) {
            if (!this.tooltipVisible) return;

            const gap = 12;
            const tooltipWidth = this.$refs.tooltip?.offsetWidth || 160;
            const tooltipHeight = this.$refs.tooltip?.offsetHeight || 20;
            const maxLeft = Math.max(gap, window.innerWidth - tooltipWidth - gap);
            const maxTop = Math.max(gap, window.innerHeight - tooltipHeight - gap);
            const preferredTop = event.clientY - tooltipHeight - gap;

            this.tooltipTop = `${Math.max(gap, Math.min(preferredTop >= gap ? preferredTop : event.clientY + gap, maxTop))}px`;
            this.tooltipLeft = `${Math.max(gap, Math.min(event.clientX + gap, maxLeft))}px`;
        }
    }
}
</script>
