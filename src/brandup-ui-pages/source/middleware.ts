import { Middleware, MiddlewareMethod, MiddlewareNext, NavigateContext } from "@brandup/ui-app";
import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";

const UI = () => import("./ui");

export class PagesMiddleware implements Middleware {
    readonly name: string = "pages";
    private __isEditing: boolean = false;

    async start(context, next: MiddlewareNext) {
        await next();

        this._showUI(context.items);
    }

    async navigate(context: NavigateContext, next: MiddlewareNext) {
        await next();

        this._showUI(context.data);
    }

    private _showUI(items: { [key: string]: any }) {
        if (document.body.dataset.pagesAdmin) {
            const page = items["page"] as Page<WebsiteApplication, PageModel>;
            if (page) {
                UI().then(t => {
                    this.__isEditing = t.default(page);
                });
            }
        }
    }
}