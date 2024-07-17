import { WEBSITE } from "brandup-ui-website";
import { PagesMiddleware } from "brandup-ui-pages";
import { LayoutMiddleware } from "./middlewares/layout";
import "./styles.less";

WEBSITE.run({
        pageTypes: {
            "content": () => import("brandup-ui-pages/source/pages/content"),
            "about": () => import("./pages/about/index")
        },
        scripts: {
            "BB1": () => import("./contents/BB1")
        }
    }, (builder) => {
            builder.useMiddleware(new LayoutMiddleware());
            builder.useMiddleware(new PagesMiddleware());
        }
    );