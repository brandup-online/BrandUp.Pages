const plugins = [
	// В Babel 8 опции helpers/useESModules удалены (helpers включены по умолчанию,
	// @babel/runtime сам выбирает CJS/ESM через "exports"), поэтому плагин без опций.
	'@babel/plugin-transform-runtime',
	[
		// В Babel 8 preset-env больше не инжектит полифилы (useBuiltIns/corejs удалены).
		// usage-global воспроизводит прежнее поведение useBuiltIns: "usage" c core-js 3.
		'polyfill-corejs3', {
			method: 'usage-global',
			version: '3.37.1'
		}
	]
]; // '@babel/plugin-transform-runtime' + 'polyfill-corejs3'

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