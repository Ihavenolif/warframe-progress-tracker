<template>
    <NavbarElement></NavbarElement>
    <main class="home-page">
        <section class="home-hero">
            <div class="home-hero__copy">
                <p class="home-eyebrow">Orbitr // Warframe progress tracker</p>
                <h1>Know what remains.</h1>
                <p class="home-hero__lede">
                    Turn your Arsenal into a clear route forward. Track mastery,
                    account for every relic, and compare progress with your clan.
                </p>
                <div class="home-hero__actions">
                    <template v-if="isLoggedIn">
                        <RouterLink class="btn btn-primary" to="/dashboard">Open dashboard</RouterLink>
                        <RouterLink class="btn home-btn-ghost" to="/progress/import">Import progress</RouterLink>
                    </template>
                    <template v-else>
                        <RouterLink class="btn btn-primary" to="/register">Start tracking</RouterLink>
                        <RouterLink class="btn home-btn-ghost" to="/login">Log in</RouterLink>
                    </template>
                </div>
            </div>

            <div class="home-sigil" aria-hidden="true">
                <span class="home-sigil__label home-sigil__label--top">Track</span>
                <span class="home-sigil__label home-sigil__label--right">Plan</span>
                <span class="home-sigil__label home-sigil__label--bottom">Complete</span>
                <svg viewBox="0 0 320 320">
                    <circle class="home-sigil__orbit" cx="160" cy="160" r="128" />
                    <circle class="home-sigil__orbit home-sigil__orbit--inner" cx="160" cy="160" r="91" />
                    <path class="home-sigil__ticks" d="M160 12v22M160 286v22M12 160h22M286 160h22" />
                    <path class="home-sigil__mark" d="M160 73 176 111 222 91 201 140 239 129 207 178 177 194 160 244 143 194 113 178 81 129 119 140 98 91 144 111Zm0 38-14 44 14 36 14-36Z" />
                    <circle class="home-sigil__core" cx="160" cy="160" r="8" />
                </svg>
            </div>
        </section>

        <section class="home-capabilities" aria-labelledby="capabilities-title">
            <div class="home-capabilities__intro">
                <p class="home-eyebrow">One account. Full picture.</p>
                <h2 id="capabilities-title">A working ledger for the Origin System.</h2>
            </div>

            <RouterLink class="home-capability" to="/progress">
                <span class="home-capability__number">01</span>
                <span class="home-capability__body">
                    <strong>Mastery inventory</strong>
                    <span>Filter every item by type and completion state. Find the mastery XP still on the table.</span>
                </span>
                <span class="home-capability__arrow">&rarr;</span>
            </RouterLink>
            <RouterLink class="home-capability" to="/relics">
                <span class="home-capability__number">02</span>
                <span class="home-capability__body">
                    <strong>Relic archive</strong>
                    <span>See owned refinements and reward tables without losing your place in the hunt.</span>
                </span>
                <span class="home-capability__arrow">&rarr;</span>
            </RouterLink>
            <RouterLink class="home-capability" to="/clans">
                <span class="home-capability__number">03</span>
                <span class="home-capability__body">
                    <strong>Clan comparison</strong>
                    <span>Put member progress side by side and reveal the next shared objective.</span>
                </span>
                <span class="home-capability__arrow">&rarr;</span>
            </RouterLink>
        </section>
    </main>
</template>

<script>
import NavbarElement from '@/components/Navbar/NavbarElement.vue';

export default {
    name: 'AppPage',
    components: {
        NavbarElement
    },
    computed: {
        isLoggedIn() {
            return Boolean(this.$store.state.username);
        }
    }
};
</script>
