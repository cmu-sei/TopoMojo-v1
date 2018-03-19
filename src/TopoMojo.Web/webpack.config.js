const path = require('path');
const webpack = require('webpack');
const merge = require('webpack-merge');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const AngularCompilerPlugin = require('@ngtools/webpack').AngularCompilerPlugin;
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

module.exports = (env) => {
    // Configuration in common to both client-side and server-side bundles
    const isDevBuild = !(env && env.prod);
    const sharedConfig = {
        mode: isDevBuild ? 'development' : 'production',
        stats: { modules: false },
        context: __dirname,
        resolve: { extensions: [ '.js', '.ts' ] },
        output: {
            filename: '[name].js',
            publicPath: '/dist/' // Webpack dev middleware, if enabled, handles requests for this URL prefix
        },
        module: {
            rules: [
                { test: /\.ts$/, use: isDevBuild ? ['awesome-typescript-loader?silent=true', 'angular2-template-loader', 'angular2-router-loader'] : '@ngtools/webpack' },
                { test: /\.html$/, use: 'html-loader?minimize=false' },
                { test: /\.css$/, use: [ 'to-string-loader', isDevBuild ? 'css-loader' : 'css-loader?minimize' ] },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
            ]
        },
        plugins: [
            new CheckerPlugin()
        ]
    };

    // Configuration for client-side bundle suitable for running in browsers
    const clientBundleOutputDir = './wwwroot/dist';
    const clientBundleConfig = merge(sharedConfig, {
        entry: { 'main-client': './ClientApp/boot-client.ts' },
        output: { path: path.join(__dirname, clientBundleOutputDir) },
        plugins: [
            new webpack.ProvidePlugin({ $: 'jquery', jQuery: 'jquery' }), // Maps these identifiers to the jQuery package (because Bootstrap expects it to be a global variable)
            new webpack.DllReferencePlugin({
                context: __dirname,
                manifest: require('./wwwroot/dist/vendor-manifest.json')
            }),
            new BundleAnalyzerPlugin({
                analyzerMode: 'static',
                reportFilename: 'main-report.html',
                openAnalyzer: false
            })
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(clientBundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [
            // Plugins that apply in production builds only
            new AngularCompilerPlugin({
                mainPath: path.join(__dirname, 'ClientApp', 'boot.client.ts'),
                tsConfigPath: './tsconfig.json',
                entryModule: path.join(__dirname, 'ClientApp', 'app','app.module#AppModule'),
                exclude: ['./**/*.server.ts']
            }),
            // new webpack.optimize.UglifyJsPlugin({
            //     output: {
            //         ascii_only: true,
            //     }
            // })
        ]),
        devtool: isDevBuild ? 'cheap-eval-source-map' : false,
        node: {
          fs: "empty"
        }
    });

    return [clientBundleConfig];
};
