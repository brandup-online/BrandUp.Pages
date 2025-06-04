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
    })
    .then(context => {
        context.app.registerCommand("signin", () => {
            ajaxRequest({
                url: context.app.buildUrl("signin"),
                success: () => {
                    context.app.reload();
                }
            })
        });
        context.app.registerCommand("signout", () => {
            ajaxRequest({
                url: context.app.buildUrl("signout"),
                success: () => {
                    context.app.reload();
                }
            })
        });
    });