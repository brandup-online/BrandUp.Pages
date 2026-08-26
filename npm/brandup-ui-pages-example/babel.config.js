const plugins = [
	[
		'@babel/plugin-transform-runtime', {
			absoluteRuntime: false
		}
	], // '@babel/plugin-transform-runtime'
	[
		// Babel 8 dropped useBuiltIns/corejs from preset-env; core-js polyfills are injected by this plugin now.
		'polyfill-corejs3', {
			method: 'usage-global',
			version: '3.50'
		}
	] // 'babel-plugin-polyfill-corejs3'
];

module.exports = {
	presets: [
		[
			"@babel/preset-env", {
				// targets берётся из .browserslistrc (единый источник истины)
				debug: false
			}
		],
		"@babel/preset-typescript"
	],
	plugins: plugins
};