const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
    configureWebpack: {
        devtool: 'source-map'
    },
    transpileDependencies: true,
    pages: {
        "index": {
            entry: 'src/pages/main.js',
            template: 'public/index.html',
            filename: 'index.html'
        },
        "about": {
            entry: 'src/pages/about.js',
            template: 'public/about.html',
            filename: 'about.html'
        },
        "login": {
            entry: 'src/pages/login.js',
            template: 'public/index.html',
            filename: 'login.html'
        },
        "register": {
            entry: 'src/pages/register.js',
            template: 'public/index.html',
            filename: 'register.html'
        },
        progressIndex: {
            entry: 'src/pages/progress/index.js',
            template: 'public/progress/index.html',
            filename: 'progress/index.html'
        },
        progressImport: {
            entry: 'src/pages/progress/import.js',
            template: 'public/progress/import.html',
            filename: 'progress/import.html'
        },
        "logout": {
            entry: 'src/pages/logout.js',
            template: 'public/index.html',
            filename: 'logout.html'
        },
        "userSettings": {
            entry: 'src/pages/userSettings.js',
            template: 'public/userSettings.html',
            filename: 'settings.html'
        },
        "clans/index": {
            entry: 'src/pages/clans/index.js',
            template: 'public/clans/index.html',
            filename: 'clans/index.html'
        },
        "clans/details": {
            entry: 'src/pages/clans/details.js',
            template: 'public/clans/details.html',
            filename: 'clans/details.html'
        },
        "clans/progress": {
            entry: 'src/pages/clans/progress.js',
            template: 'public/clans/progress.html',
            filename: 'clans/progress.html'
        },
        "adminPanel": {
            entry: 'src/pages/adminPanel.js',
            template: 'public/adminPanel.html',
            filename: 'admin.html'
        },
    },
    devServer: {
        host: 'www.localhost.me',
        port: 8080,
        https: {
            key: "../https-setup/localhost-me.key",
            cert: "../https-setup/localhost-me.crt",
        },
        headers: {
            'Access-Control-Allow-Origin': 'https://www.localhost.me:8080',
            'Access-Control-Allow-Credentials': 'true'
        },
        proxy: {
            '/api': {
                target: 'https://api.localhost.me:5224',
                changeOrigin: true,
                secure: false,  // allow self-signed cert in dev
                cookieDomainRewrite: 'www.localhost.me', // optional
            },
            '/images': {
                target: 'https://www.localhost.me:18443',
                changeOrigin: true,
                secure: false,  // allow self-signed cert in dev
            }
        },
        historyApiFallback: {
            rewrites: [
                {
                    from: /^\/clans\/.*\/progress$/,
                    to: "/clans/progress.html"
                },
                {
                    from: /^\/clans\/.*\/details$/,
                    to: "/clans/details.html"
                },
                {
                    from: /^\/.*$/,  // match any path
                    to(context) {
                        const url = context.parsedUrl.pathname;

                        // If path ends with '/', serve index.html inside that folder
                        if (url.endsWith('/')) {
                            return url + 'index.html';
                        }

                        // Otherwise, try serving url + '.html'
                        return url + '.html';
                    },
                },
            ],
        }

    }
})