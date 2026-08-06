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
                target: 'http://localhost:5224',
                changeOrigin: true,
            },
            '/images': {
                target: 'http://localhost:18080',
                changeOrigin: true,
            }
        },
        historyApiFallback: true

    }
})
