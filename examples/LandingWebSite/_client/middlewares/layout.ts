import { MiddlewareNext, Middleware, NavigateContext, StartContext } from "@brandup/ui-app";
import { request } from "@brandup/ui-ajax";

export class LayoutMiddleware implements Middleware {
    name = "layout-middleware";

    async start(_context: StartContext, next: MiddlewareNext) {
        await next();

        _context.app.registerCommand("signin", () => {
            request({
                url: _context.app.buildUrl("signin"),
            }, _context.abort).then(() => _context.app.reload());
        });

        _context.app.registerCommand("signout", async () => {
            request({
                url: _context.app.buildUrl("signout"),
            }, _context.abort).then(() => _context.app.reload());
        });

        _context.app.registerCommand("toggle-app-menu", () => {
            document.body.classList.toggle("website-state-show-appmenu");
        });
    }

    async navigate(context: NavigateContext, next: MiddlewareNext) {
        await next();

        document.body.classList.remove("website-state-show-appmenu");
    }
}