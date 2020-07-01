import { host } from "brandup-ui-website";
import "./styles.less";
import { ajaxRequest } from "brandup-ui";
import { ContentMiddleware } from "./brandup.pages/middleware";

host.start({
    pageTypes: {
        "content": () => import("./brandup.pages/pages/content"),
        "about": () => import("./pages/about/index")
    },
    scripts: {
        "BB1": () => import("./contents/BB1")
    }
}, (builder) => {
        builder.useMiddleware(new ContentMiddleware());
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