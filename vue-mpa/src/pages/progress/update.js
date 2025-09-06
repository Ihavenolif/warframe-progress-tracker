import { createApp } from 'vue'
import Update from '../../routes/progress/Update.vue'
import { store } from '../../store';

import "@/assets/styles/global.css"
import "@/assets/styles/three-column-layout.css"
import "@fortawesome/fontawesome-free/css/all.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(Update)
    .use(store)
    .mount('#app')