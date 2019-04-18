﻿"use strict";

const path = require('path');
const webpack = require('webpack');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const bundleOutputDir = './wwwroot/dist';

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        entry: {
            app: path.resolve(__dirname, '_client', 'root.ts')
        },
        resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx'] },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/',
            libraryTarget: 'umd'
        },
        module: {
            rules: [
                { test: /\.tsx?$/, include: /_client/, use: 'awesome-typescript-loader?silent=true' },
                {
                    test: /\.(le|c)ss$/,
                    use: [
                        //{
                        //    loader: 'style-loader',
                        //    options: { }
                        //},
                        {
                            loader: MiniCssExtractPlugin.loader
                        },
                        {
                            loader: 'css-loader',
                            options: {
                                minimize: !isDevBuild
                            }
                        }, {
                            loader: 'less-loader', options: {
                                strictMath: false,
                                noIeCompat: true,
                                minimize: !isDevBuild
                            }
                        }
                    ]
                },
                { test: /\.(png|jpg|jpeg|gif|svg)$/, use: 'url-loader?limit=25000' }
            ]
        },
        optimization: {
            minimize: !isDevBuild,
            namedModules: true
        },
        plugins: [
            new CheckerPlugin(),
            new MiniCssExtractPlugin()
        ].concat(isDevBuild ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
                filename: '[file].map', // Remove this line if you prefer inline source maps
                moduleFilenameTemplate: path.relative(bundleOutputDir, '[resourcePath]') // Point sourcemap entries to the original file locations on disk
            })
        ] : [])
    }];
};