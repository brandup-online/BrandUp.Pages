import { Application, ApplicationModel, Middleware, NavigateContext } from "brandup-ui-app";
import { ajaxRequest } from "brandup-ui-ajax";

export class LayoutMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> {
    start(_context, next: VoidFunction) {
        next();

        this.app.registerCommand("signin", () => {
            ajaxRequest({
                url: this.app.uri("signin"),
                success: () => {
                    this.app.reload();
                }
            })
        });

        this.app.registerCommand("signout", () => {
            ajaxRequest({
                url: this.app.uri("signout"),
                success: () => {
                    this.app.reload();
                }
            })
        });

        this.app.registerCommand("toggle-app-menu", () => {
            document.body.classList.toggle("website-state-show-appmenu");
        });
    }

    navigate(context: NavigateContext, next) {
        next();

        document.body.classList.remove("website-state-show-appmenu");
    }
}