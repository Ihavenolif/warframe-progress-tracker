import { createApp } from 'vue'
import { store } from '../../store';

import "@/assets/styles/global.css"
import "@/assets/styles/three-column-layout.css"
import Import from '@/routes/Progress/Import.vue';

createApp(Import)
    .use(store)
    .mount('#app')