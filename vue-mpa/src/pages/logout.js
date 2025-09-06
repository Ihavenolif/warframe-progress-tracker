import { createApp } from 'vue'
import Logout from '@/routes/Logout.vue';
import { store } from '../store';

import "../assets/styles/global.css"
import "../assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(Logout)
    .use(store)
    .mount('#app')