import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/',
            name: 'home',
            alias: '/index.html',
            component: () => import('@/routes/App.vue')
        },
        {
            path: '/about',
            name: 'about',
            alias: '/about.html',
            component: () => import('@/routes/About.vue')
        },
        {
            path: '/login',
            name: 'login',
            alias: '/login.html',
            component: () => import('@/routes/Login.vue')
        },
        {
            path: '/register',
            name: 'register',
            alias: '/register.html',
            component: () => import('@/routes/Register.vue')
        },
        {
            path: '/logout',
            name: 'logout',
            alias: '/logout.html',
            component: () => import('@/routes/Logout.vue')
        },
        {
            path: '/settings',
            name: 'settings',
            alias: '/settings.html',
            component: () => import('@/routes/UserSettings.vue')
        },
        {
            path: '/dashboard',
            name: 'dashboard',
            alias: '/dashboard.html',
            component: () => import('@/routes/Dashboard.vue')
        },
        {
            path: '/progress',
            name: 'progress',
            alias: ['/progress/index', '/progress/index.html'],
            component: () => import('@/routes/Progress/Index.vue')
        },
        {
            path: '/progress/import',
            name: 'progress-import',
            alias: '/progress/import.html',
            component: () => import('@/routes/Progress/Import.vue')
        },
        {
            path: '/clans',
            name: 'clans',
            alias: ['/clans/index', '/clans/index.html'],
            component: () => import('@/routes/Clans/Index.vue')
        },
        {
            path: '/clans/:clanName/details',
            name: 'clan-details',
            component: () => import('@/routes/Clans/Details.vue'),
            props: true
        },
        {
            path: '/clans/:clanName/progress',
            name: 'clan-progress',
            component: () => import('@/routes/Clans/Progress.vue'),
            props: true
        },
        {
            path: '/admin',
            name: 'admin',
            alias: '/admin.html',
            component: () => import('@/routes/AdminPanel.vue')
        },
        {
            path: '/:pathMatch(.*)*',
            redirect: { name: 'home' }
        }
    ],
    scrollBehavior() {
        return { top: 0 }
    }
})

export default router
