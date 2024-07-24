const config = {
	rootDir: __dirname,
	setupFilesAfterEnv: ['<rootDir>/src/npm/__mocks__/setup.jest.ts'],
	testMatch: ["**/test/**/*.test.ts"],
	testEnvironment: "jsdom",

	verbose: true,
	transform: {
		"^.+\\.[jt]sx?$": "babel-jest",
		".+\\.(css|styl|less|sass|scss|png|jpg|ttf|woff|woff2)$": "jest-transform-stub",
	},
	moduleFileExtensions: ["js", "ts"],
	//moduleDirectories: ["node_modules", "bower_components", "shared"],
	//transformIgnorePatterns: ["/node_modules/(?!(brandup-ui?([a-z,-])*)/)"]
	testEnvironmentOptions: {
		customExportConditions: [''],
	  }
};

module.exports = config;