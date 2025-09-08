import { createApp } from 'vue'
import ClansDetails from '@/routes/Clans/Details.vue'
import { store } from '@/store';

import "@/assets/styles/global.css"
import "@/assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(ClansDetails)
    .use(store)
    .mount('#app')