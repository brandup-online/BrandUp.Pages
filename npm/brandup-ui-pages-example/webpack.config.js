"use strict";

const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const CleanCSSPlugin = require("less-plugin-clean-css");
const TerserPlugin = require("terser-webpack-plugin");
const bundleOutputDir = '../../src/LandingWebSite/wwwroot/dist';

const lessLoaderOptions = { webpackImporter: true, lessOptions: { math: 'always', plugins: [new CleanCSSPlugin({ advanced: false })] } };
var splitChunks = {
    cacheGroups: {
        vendors: {
            test: /[\\/]node_modules[\\/]/,
            reuseExistingChunk: true,
            enforce: true
        },
        styles: {
            test: /\.(css|scss|less)$/, // нужно чтобы import`ы на одинаковые файла less не дублировались на выходе
            reuseExistingChunk: true,
            enforce: true
        },
        images: {
            test: /\.(svg|jpg|png)$/,
            reuseExistingChunk: true,
            enforce: true
        }
    }
};

module.exports = (env) => {
    const isDevBuild = process.env.NODE_ENV !== "production";

    console.log(`NODE_ENV: "${process.env.NODE_ENV}"`);
    console.log(`isDevBuild: ${isDevBuild}`);

    return [{
        entry: {
            app: path.resolve(__dirname, 'source', 'index.ts')
        },
        resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx', '.less'] },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            chunkFilename: isDevBuild ? '[name].js' : '[name].[contenthash].js',
            iife: true,
            clean: true,
            publicPath: 'dist/'
        },
        module: {
            rules: [
                {
                    test: /\.(?:ts|js|mjs|cjs)$/,
                    exclude: {
                        and: [/node_modules/],
                        not: [/@brandup/]
                    },
                    use: {
                        loader: 'babel-loader'
                    }
                },
                {
                    test: /\.(le|c)ss$/,
                    use: [
                        { loader: MiniCssExtractPlugin.loader },
                        { loader: 'css-loader', options: { importLoaders: 2 } },
                        { loader: 'less-loader', options: lessLoaderOptions }
                    ]
                },
                {
                    test: /\.svg$/,
                    use: [
                        { loader: "raw-loader" },
                        {
                            loader: "svgo-loader",
                            options: {
                                configFile: __dirname + "/svgo.config.mjs",
                                floatPrecision: 2,
                            }
                        }
                    ]
                },
                {
                    test: /\.(png|jpg|jpeg|gif)$/,
                    use: 'url-loader?limit=25000'
                }
            ]
        },
        optimization: {
            splitChunks: splitChunks,
            minimize: !isDevBuild,
            minimizer: [
                new TerserPlugin({
                    terserOptions: {
                        compress: true,
                        keep_classnames: true,
                        keep_fnames: true,
                        format: {
                            comments: false
                        },
                    },
                    extractComments: false
                })
            ],
            removeAvailableModules: true,
            removeEmptyChunks: true,
            providedExports: false,
            usedExports: false
        },
        plugins: [
            new MiniCssExtractPlugin({
                filename: '[name].css',
                chunkFilename: isDevBuild ? '[id].css' : '[id].[contenthash].css',
                ignoreOrder: true
            })
        ]
    }];
};