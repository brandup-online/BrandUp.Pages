const plugins = [
	[
		'@babel/plugin-transform-runtime', {
			absoluteRuntime: false,
			corejs: false,
			helpers: true,
			useESModules: true
		}
	]
]; // '@babel/plugin-transform-runtime'

const isModern = process.env.BROWSERS_ENV === 'modern';

module.exports = {
  presets: [
    [
		"@babel/preset-env", {
			useBuiltIns: "usage",
			corejs: "3.37.1",
			targets: isModern ? { esmodules: true } : undefined,
			debug: false
    	}
	],
    "@babel/preset-typescript"
  ],
  plugins: plugins
};