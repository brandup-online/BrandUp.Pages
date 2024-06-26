import { Middleware, NavigateContext } from "brandup-ui-app";
export class LayoutMiddleware extends Middleware {
    start(context, next) {
        next();

        this.app.registerCommand("toggle-app-menu", () => {
            document.body.classList.toggle("website-state-show-appmenu");
        });
    }

    navigate(context: NavigateContext, next) {
        next();

        document.body.classList.remove("website-state-show-appmenu");
    }
}