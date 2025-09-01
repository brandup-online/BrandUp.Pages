import { WEBSITE } from "@brandup/ui-website";
import { ajaxRequest } from "@brandup/ui-ajax";
import { PagesMiddleware } from "brandup-ui-pages";
import "./styles.less";

WEBSITE.run(
    {
        pages: {
            "content": { factory: () => import("brandup-ui-pages/source/pages/content") },
            "about": { factory: () => import("./pages/about/index") }
        },
        components: {
            "BB1": { factory: () => import("./contents/BB1") }
        }
    },
    (builder) => builder.useMiddleware(() => new PagesMiddleware()))
    .then((context) => {
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

        context.app.registerCommand("toggle-app-menu", () => {
            document.body.classList.toggle("website-state-show-appmenu");
        });

        window.addEventListener("pageNavigated", () => {
            document.body.classList.remove("website-state-show-appmenu");
        });
    });