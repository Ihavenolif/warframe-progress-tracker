import { createApp } from 'vue'
import UserSettings from '@/routes/UserSettings.vue';
import { store } from '../store';

import "../assets/styles/global.css"
import "../assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(UserSettings)
    .use(store)
    .mount('#app')