import { WEBSITE } from "@brandup/ui-website";
import { ajaxRequest } from "@brandup/ui-ajax";
import { PagesMiddleware } from "brandup-ui-pages";
import { LayoutMiddleware } from "./middlewares/layout";
import "./styles.less";

WEBSITE.run({
    pages: {
        "content": { factory: () => import("brandup-ui-pages/source/pages/content"), preload: true },
        "about": { factory: () => import("./pages/about/index"), preload: true }
    },
    components: {
        "BB1": { factory: () => import("./contents/BB1"), preload: true }
    }
}, (builder) => {
        builder.useMiddleware(() => new LayoutMiddleware());
        builder.useMiddleware(() => new PagesMiddleware());
    }, (app) => {
        app.registerCommand("signin", () => {
            ajaxRequest({
                url: app.uri("signin"),
                success: () => {
                    app.reload();
                }
            })
        });

        app.registerCommand("signout", () => {
            ajaxRequest({
                url: app.uri("signout"),
                success: () => {
                    app.reload();
                }
            })
        });
});