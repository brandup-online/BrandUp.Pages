"use strict";

const path = require('path');
const webpack = require('webpack');
const CKEditorWebpackPlugin = require('@ckeditor/ckeditor5-dev-webpack-plugin');
const { styles } = require('@ckeditor/ckeditor5-dev-utils');
const bundleOutputDir = './dist/';

module.exports = (env) => {
    const isDevBuild = !(env && env.prod);

    return [{
        entry: {
            app: path.resolve(__dirname, 'editor.js')
        },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: 'dist/',
            libraryTarget: 'umd'
        },
        module: {
            rules: [
                {
                    test: /\.svg$/,
                    use: ['raw-loader']
                },
                {
                    test: /\.css$/,
                    use: [
                        {
                            loader: 'style-loader',
                            options: {
                                singleton: true
                            }
                        },
                        {
                            loader: 'postcss-loader',
                            options: styles.getPostCssConfig({
                                themeImporter: {
                                    themePath: require.resolve('@ckeditor/ckeditor5-theme-lark')
                                },
                                minify: true
                            })
                        },
                    ]
                }
            ]
        },
        optimization: {
            minimize: !isDevBuild
        },
        plugins: [
            new CKEditorWebpackPlugin({
                language: 'ru'
            })
        ]
    }];
};