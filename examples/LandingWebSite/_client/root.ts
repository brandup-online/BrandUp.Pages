import { AppClientModel } from "./brandup.pages/typings/website";
import { Application } from "./brandup.pages/app";
import "./styles.less";

export interface LandingWebSiteModel extends AppClientModel {
}

Application.setup<LandingWebSiteModel>({
    configure: (builder) => {
        builder.addScript("BB1", () => import("./contents/BB1"));
    }
}, (app) => {
    app.registerCommand("signin", () => {
        app.request({
            url: app.uri("signin"),
            success: () => {
                app.reload();
            }
        })
    });

    app.registerCommand("signout", () => {
        app.request({
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