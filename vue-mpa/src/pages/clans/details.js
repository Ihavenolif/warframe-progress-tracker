import { createApp } from 'vue'
import ClansDetails from '@/routes/Clans/Details.vue'
import { store } from '@/store';

import "@/assets/styles/global.css"
import "@/assets/styles/three-column-layout.css"
import 'font-awesome/css/font-awesome.min.css'

import { library } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';

// Import the icons you need
import { faCrown, faCaretDown } from '@fortawesome/free-solid-svg-icons';

// Add them to the library
library.add(faCrown, faCaretDown);

const app = createApp(ClansDetails);

app.component('font-awesome-icon', FontAwesomeIcon);
app.use(store);
app.mount('#app');