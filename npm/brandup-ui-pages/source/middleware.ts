import { Middleware, MiddlewareNext, NavigateContext, StartContext } from "@brandup/ui-app";
import { Page } from "@brandup/ui-website";
import { ContentPage } from "./pages/content";

export class PagesMiddleware implements Middleware {
    name: string = "Pages";

    async start(_context: StartContext, next: MiddlewareNext) {
        await next();

        //this._showUI(context.data);
    }

    async navigate(context: NavigateContext, next: MiddlewareNext) {
        await next();

        this._showUI(context.data);
    }

    private _showUI(items: { [key: string]: any }) {
        if (items["page"] instanceof Page) {
            const page = items["page"];
            if (!page.model.enableAdministration)
                return;

            if (!page.model.editId) {
                import("./admin/website").then(d => {
                    const toolbar = new d.WebSiteToolbar(page);
                    page.onDestroy(() => toolbar.destroy());
                });
            }

            if (page instanceof ContentPage) {
                import("./admin/page").then(d => {
                    const toolbar = new d.PageToolbar(page);
                    page.onDestroy(() => toolbar.destroy());
                });
            }
        }
    }
}