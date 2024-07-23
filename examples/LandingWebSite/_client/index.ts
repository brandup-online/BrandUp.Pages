import { WEBSITE } from "@brandup/ui-website";
import { PagesMiddleware } from "@brandup/ui-pages";
import { LayoutMiddleware } from "./middlewares/layout";
import "./styles.less";

WEBSITE.run({
        pageTypes: {
            "content": { factory: () => import("@brandup/ui-pages/source/pages/content") },
            "about": { factory: () => import("./pages/about/index") }
        },
        scripts: {
            "BB1": { factory: () => import("./contents/BB1") }
        }
    }, (builder) => {
            builder.useMiddleware(new LayoutMiddleware());
            builder.useMiddleware(new PagesMiddleware());
        }
    );