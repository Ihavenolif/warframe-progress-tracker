const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
    configureWebpack: {
        devtool: 'source-map'
    },
    transpileDependencies: true,
    devServer: {
        host: 'localhost',
        port: 8080,
        proxy: {
            '/api': {
                target: process.env.BACKEND_URL || 'http://localhost:5224',
                changeOrigin: true,
            },
            '/images': {
                target: process.env.IMGPROXY_URL || 'http://localhost:20080',
                changeOrigin: true,
                pathRewrite: {
                    '^/images/': '/insecure/resize:fit:34:34:0/gravity:sm/plain/'
                },
            }
        },
        historyApiFallback: true

    }
})
