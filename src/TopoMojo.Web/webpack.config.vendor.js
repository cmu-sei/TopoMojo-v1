const path = require('path');
const webpack = require('webpack');
// const ExtractTextPlugin = require('extract-text-webpack-plugin');
const merge = require('webpack-merge');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

const treeShakableModules = [
    '@angular/animations',
    '@angular/animations/browser',
    '@angular/common',
    '@angular/common/http',
    '@angular/compiler',
    '@angular/core',
    '@angular/forms',
    '@angular/platform-browser',
    '@angular/platform-browser/animations',
    '@angular/platform-browser-dynamic',
    '@angular/router',
    '@ng-bootstrap/ng-bootstrap',
    '@ngx-translate/core',
    '@ngx-translate/http-loader',
    'zone.js',
];
const nonTreeShakableModules = [
    '@aspnet/signalr',
    'babel-polyfill',
    'bootstrap/dist/css/bootstrap.css',
    'core-js',
    'font-awesome/css/font-awesome.css',
    'es6-promise',
    'es6-shim',
    'event-source-polyfill',
    'jquery',
    'oidc-client',
    'reflect-metadata',
    'showdown',
];
const allModules = treeShakableModules.concat(nonTreeShakableModules);

module.exports = (env) => {
    console.log(`env = ${JSON.stringify(env)}`)

    // const extractCSS = new ExtractTextPlugin('[name].css');
    const isDevBuild = !(env && env.prod);
    const sharedConfig = {
        mode: isDevBuild ? 'development' : 'production',
        stats: { modules: false },
        resolve: { extensions: [ '.js' ] },
        module: {
            rules: [
                { test: /\.woff2(\?|$)/, use: 'url-loader' },
                { test: /\.(png|woff|eot|ttf|svg)(\?|$)/, use: 'file-loader' }
            ]
        },
        output: {
            publicPath: '/dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        output: {
            publicPath: '/dist/',
            filename: '[name].js',
            library: '[name]_[hash]'
        },
        plugins: [
            new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.ContextReplacementPlugin(/\@angular\b.*\b(bundles|linker)/, path.join(__dirname, './ClientApp')), // Workaround for https://github.com/angular/angular/issues/11580
            new webpack.ContextReplacementPlugin(/(.+)?angular(\\|\/)core(.+)?/, path.join(__dirname, './ClientApp')), // Workaround for https://github.com/angular/angular/issues/14898
            new webpack.IgnorePlugin(/^vertx$/) // Workaround for https://github.com/stefanpenner/es6-promise/issues/100
        ]
    };

    const clientBundleConfig = merge(sharedConfig, {
        entry: {
            // To keep development builds fast, include all vendor dependencies in the vendor bundle.
            // But for production builds, leave the tree-shakable ones out so the AOT compiler can produce a smaller bundle.
            vendor: isDevBuild ? allModules : nonTreeShakableModules
        },
        output: { path: path.join(__dirname, 'wwwroot', 'dist') },
        module: {
            rules: [
                { test: /\.css(\?|$)/, use: [
                    MiniCssExtractPlugin.loader,
                    "css-loader?minimize"
                  ]
                }
            ]
        },
        plugins: [
            //extractCSS,
            new MiniCssExtractPlugin({
                // Options similar to the same options in webpackOptions.output
                // both options are optional
                filename: "[name].css",
                // chunkFilename: "[id].css"
            }),
            new webpack.DllPlugin({
                path: path.join(__dirname, 'wwwroot', 'dist', '[name]-manifest.json'),
                name: '[name]_[hash]'
            }),
            new BundleAnalyzerPlugin({
                analyzerMode: 'static',
                reportFilename: 'vendor-report.html',
                openAnalyzer: false
            })
        ].concat(isDevBuild ? [] : [
            //new webpack.optimize.UglifyJsPlugin()
        ]),
        node: {
            fs: "empty"
        }
    });

    return [clientBundleConfig];
}
