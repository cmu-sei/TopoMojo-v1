const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const OptimizeCssAssetsPlugin = require('optimize-css-assets-webpack-plugin');
const UglifyJSPlugin = require('uglifyjs-webpack-plugin');
const merge = require('webpack-merge');

module.exports = (env) => {
    const extractCSS = new ExtractTextPlugin('[name].css');
    const isDevBuild = !(env && env.prod);
    const sharedConfig = {
        stats: { modules: true },
        module: {
            rules: [
                // { test: /\.woff2$/, use: 'file-loader' }
                { test: /\.(png|eot|[ot]tf|woff|woff2|svg)(\?|$)/, use: 'file-loader' }

            ]
        },
        entry: {
            vendor: [
                '@angular/common',
                '@angular/compiler',
                '@angular/core',
                '@angular/platform-browser',
                '@angular/platform-browser-dynamic',
                '@angular/router',
                '@aspnet/signalr-client',
                'bootstrap',
                'bootstrap/dist/css/bootstrap.css',
                'font-awesome-webpack2',
                'font-awesome/css/font-awesome.css',
                'es6-shim',
                'event-source-polyfill',
                'jquery',
                'oidc-client',
                'showdown',
                'zone.js',
            ]
        },
        output: {
            publicPath: '/dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        plugins: [
            new webpack.ProvidePlugin({ jQuery: 'jquery', $: 'jquery', "jquery.js": 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.optimize.ModuleConcatenationPlugin(),
            new webpack.ContextReplacementPlugin(
                /\@angular(\\|\/)core(\\|\/)esm5/,
                path.join(__dirname, './client')
            ),
        ]
    };

    const clientBundleConfig = merge(sharedConfig, {
        output: { path: path.join(__dirname, 'wwwroot', 'dist') },
        module: {
            rules: [
                { test: /\.css(\?|$)/, use: extractCSS.extract({ use: 'css-loader' }) }
            ]
        },
        plugins: [
            extractCSS,
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            })
        ].concat(isDevBuild ? [] : [
            new UglifyJSPlugin({
                parallel: true,
            })
        ])
    });

    return [clientBundleConfig];
}
