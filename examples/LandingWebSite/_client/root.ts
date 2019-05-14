import { AppClientModel } from "./pages/typings/website";
import { Application } from "./pages/app";
import "./styles.less";

export interface LandingWebSiteModel extends AppClientModel {
}

Application.setup<LandingWebSiteModel>({
    configure: (builder) => {
        builder.addPageType("content-page", () => import("./pages/pages/page"));
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
});