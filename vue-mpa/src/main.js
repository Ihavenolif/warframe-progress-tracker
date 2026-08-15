import { createApp } from 'vue'
import { library } from '@fortawesome/fontawesome-svg-core'
import { faCaretDown, faCrown } from '@fortawesome/free-solid-svg-icons'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import AppShell from './AppShell.vue'
import router from './router'
import { store } from './store'

import './assets/styles/index.css'
import 'font-awesome/css/font-awesome.min.css'

library.add(faCaretDown, faCrown)

createApp(AppShell)
    .component('font-awesome-icon', FontAwesomeIcon)
    .use(store)
    .use(router)
    .mount('#app')
