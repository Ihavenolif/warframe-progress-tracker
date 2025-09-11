import { createApp } from 'vue'
import ClansIndex from '@/routes/Clans/Index.vue'
import { store } from '@/store';

import "@/assets/styles/global.css"
import "@/assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(ClansIndex)
    .use(store)
    .mount('#app')