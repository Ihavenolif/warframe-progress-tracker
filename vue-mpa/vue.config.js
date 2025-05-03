const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
    transpileDependencies: true,
    pages: {
        index: {
            entry: 'src/pages/main.js',
            template: 'public/index.html',
            filename: 'index.html'
        },
        about: {
            entry: 'src/pages/about.js',
            template: 'public/about.html',
            filename: 'about.html'
        },
        login: {
            entry: 'src/pages/login.js',
            template: 'public/login.html',
            filename: 'login.html'
        },
        progress: {
            entry: 'src/pages/progress.js',
            template: 'public/progress.html',
            filename: 'progress.html'
        }
    }
})
