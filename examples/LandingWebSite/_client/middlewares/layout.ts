import { Middleware, MiddlewareMethod, NavigateContext } from "@brandup/ui-app";
export class LayoutMiddleware implements Middleware {
    readonly name: string = "layout";

    async start(context, next) {
        await next();

        context.app.registerCommand("toggle-app-menu", () => {
            document.body.classList.toggle("website-state-show-appmenu");
        });
    }

    async navigate(context: NavigateContext, next) {
        await next();

        document.body.classList.remove("website-state-show-appmenu");
    }
}