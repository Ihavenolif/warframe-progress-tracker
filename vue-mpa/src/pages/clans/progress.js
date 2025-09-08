import { createApp } from 'vue'
import ClansProgress from '@/routes/Clans/Progress.vue'
import { store } from '@/store';

import "@/assets/styles/global.css"
import "@/assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(ClansProgress)
    .use(store)
    .mount('#app')