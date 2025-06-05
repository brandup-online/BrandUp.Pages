import { WEBSITE } from "@brandup/ui-website";
import { request } from "@brandup/ui-ajax";
import { pagesMiddlewareFactory } from "brandup-ui-pages";
import { LayoutMiddleware } from "./middlewares/layout";
import { uiKitMiddlewareFactory } from "@brandup/ui-kit";
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
    builder.useMiddleware(uiKitMiddlewareFactory);
    builder.useMiddleware(() => new LayoutMiddleware());
    builder.useMiddleware(pagesMiddlewareFactory);
})
    .then(context => {
        context.app.registerCommand("signin", async () => {
            await request({ url: context.app.buildUrl("signin") });
            context.app.reload();
        });
        context.app.registerCommand("signout", async () => {
            await request({ url: context.app.buildUrl("signout") });
            context.app.reload();
        });
    });