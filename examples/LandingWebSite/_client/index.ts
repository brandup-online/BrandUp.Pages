import { host } from "brandup-ui-website";
import { ajaxRequest } from "brandup-ui-ajax";
import { PagesMiddleware } from "brandup-pages-ui";
import "./styles.less";

host.start({
    pageTypes: {
        "content": () => import("brandup-pages-ui/source/pages/content"),
        "about": () => import("./pages/about/index")
    },
    scripts: {
        "BB1": () => import("./contents/BB1")
    }
}, (builder) => {
        builder.useMiddleware(new PagesMiddleware());
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

        app.registerCommand("toggle-app-menu", () => {
            document.body.classList.toggle("website-state-show-appmenu");
        });

        window.addEventListener("pageNavigated", () => {
            document.body.classList.remove("website-state-show-appmenu");
        });
});