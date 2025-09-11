import { createApp } from 'vue'
import Login from '../routes/Login.vue'
import { store } from '../store';

import "../assets/styles/global.css"
import "../assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(Login)
    .use(store)
    .mount('#app')