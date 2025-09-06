import { createApp } from 'vue'
import Progress from '@/routes/Progress/Index.vue'
import { store } from '@/store';

import "@/assets/styles/global.css"
import "@/assets/styles/three-column-layout.css"
import "@/assets/styles/collapsible.css"
import "@/assets/styles/form.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(Progress)
    .use(store)
    .mount('#app')