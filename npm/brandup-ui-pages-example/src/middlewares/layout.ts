import { Middleware, MiddlewareNext, NavigateContext, StartContext } from "@brandup/ui-app";
export class LayoutMiddleware implements Middleware {
    readonly name: string = "layout";

    async start(context: StartContext, next: MiddlewareNext) {
        await next();

        context.app.registerCommand("toggle-app-menu", () => {
            document.body.classList.toggle("website-state-show-appmenu");
        });
    }

    async navigate(context: NavigateContext, next: MiddlewareNext) {
        await next();

        document.body.classList.remove("website-state-show-appmenu");
    }
}