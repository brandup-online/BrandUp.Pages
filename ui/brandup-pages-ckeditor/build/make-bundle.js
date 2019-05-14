const { promisify } = require("util");
const fs = require("fs");
const path = require("path");
const rollup = require("rollup");
const rollupTypescript = require("rollup-plugin-typescript2");
const rollupLess = require("rollup-plugin-less");
const rollupPostCss = require("rollup-plugin-postcss");
const rollupResolve = require("rollup-plugin-node-resolve");
const readFile = promisify(fs.readFile);
const writeFile = promisify(fs.writeFile);
const pkg = require("../package.json");

const packageName = pkg.name;
const srcPath = path.join(__dirname, "..", "src");
const compiledPath = path.join(__dirname, "..", "src");
const distNpmPath = path.join(__dirname, "..", "dist");

async function build() {
    let bundle = await rollup.rollup({
        input: path.join(compiledPath, "editor.js"),
        output: [
            {
                file: pkg.main,
                format: 'es'
            }
        ],
        external: [
            ...Object.keys(pkg.dependencies || {}),
            ...Object.keys(pkg.peerDependencies || {})
        ],
        plugins: [
            //rollupPostCss({
            //    extract: path.join(distNpmPath, `${packageName}.css`),
            //    extensions: [ '.less' ],
            //    inject: false,
            //    modules: false
            //}),
            rollupResolve({
                browser: true,
                mainFields: [ 'main' ]
            })
        ]
    });

    let { code } = await bundle.generate({
        format: "es",
        sourcemap: false
    });

    fs.mkdir(distNpmPath, () => { });

    await writeFile(path.join(distNpmPath, `${packageName}.js`), code);
}

build().then(() => {
    console.log("done");
}, err => console.log(err.message, err.stack));