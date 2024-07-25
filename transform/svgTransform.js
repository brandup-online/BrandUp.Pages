module.exports = {
    process() {
      return {
        code: `module.exports = "icon";`,
      };
    },
    getCacheKey() {
      // The output is always the same.
      return "svgTransform";
    },
};