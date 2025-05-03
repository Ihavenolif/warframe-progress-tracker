import { createApp } from 'vue'
import App from '../routes/App.vue'
import { store } from '../store';

import "../assets/styles/global.css"

createApp(App)
    .use(store)
    .mount('#app')