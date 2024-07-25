const config = {
	rootDir: __dirname,
	setupFilesAfterEnv: ['<rootDir>/src/npm/brandup-ui-pages/mocks/setup.jest.ts'],
	testMatch: ["**/test/**/*.test.ts"],
	testEnvironment: "jsdom",

	verbose: true,
	transform: {
		"^.+\\.[jt]sx?$": "babel-jest",
		".+\\.(css|styl|less|sass|scss|png|jpg|ttf|woff|woff2)$": "jest-transform-stub",
		"^.+\\.svg$": "<rootDir>/transform/svgTransform.js"
	},
	moduleFileExtensions: ["js", "ts"],
	//moduleDirectories: ["node_modules", "bower_components", "shared"],
	//transformIgnorePatterns: ["/node_modules/(?!(brandup-ui?([a-z,-])*)/)"]
	testEnvironmentOptions: {
		customExportConditions: [''],
	  }
};

module.exports = config;