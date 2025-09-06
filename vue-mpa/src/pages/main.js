import { createApp } from 'vue'
import App from '../routes/App.vue'
import { store } from '../store';

import "../assets/styles/global.css"
import 'font-awesome/css/font-awesome.min.css'

createApp(App)
    .use(store)
    .mount('#app')