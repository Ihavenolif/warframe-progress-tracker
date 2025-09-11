import { createApp } from 'vue'
import AdminPanel from '../routes/AdminPanel.vue'
import { store } from '../store';

import "../assets/styles/global.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(AdminPanel)
    .use(store)
    .mount('#app')