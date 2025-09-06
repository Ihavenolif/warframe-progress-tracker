import { createApp } from 'vue'
import Register from '@/routes/Register.vue';
import { store } from '../store';

import "../assets/styles/global.css"
import "../assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(Register)
    .use(store)
    .mount('#app')