import { WEBSITE } from "brandup-ui-website";
import { ajaxRequest } from "brandup-ui-ajax";
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
    })
.then((app) => {
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