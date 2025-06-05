import { Middleware, MiddlewareNext, NavigateContext, StartContext } from "@brandup/ui-app";
import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";

const UI = () => import("./ui");

export class PagesMiddleware implements Middleware {
    readonly name: string = "pages";
    private __isEditing: boolean = false;

    get isEditing(): boolean { return this.__isEditing; }

    async start(_context: StartContext, next: MiddlewareNext) {
        await next();
    }

    async navigate(context: NavigateContext, next: MiddlewareNext) {
        await next();

        this._showUI(context.data);
    }

    private _showUI(data: { [key: string]: any }) {
        if (document.body.dataset.pagesAdmin) {
            const page = data.page as Page<WebsiteApplication, PageModel>;
            if (page) {
                UI().then(t => {
                    this.__isEditing = t.default(page);
                });
            }
        }
    }
}