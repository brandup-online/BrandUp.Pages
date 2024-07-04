import { Middleware, NavigateContext } from "brandup-ui-app";
import { Page, PageModel } from "brandup-ui-website";

const UI = () => import("./ui");

export class PagesMiddleware extends Middleware {
    private __isEditing: boolean = false;

    start(context, next) {
        next();

        this._showUI(context.items);
    }

    navigate(context: NavigateContext, next: () => void) {
        next();

        this._showUI(context.items);
    }

    private _showUI(items: { [key: string]: any }) {
        if (document.body.dataset.pagesAdmin) {
            const page = items["page"] as Page<PageModel>;
            if (page) {
                UI().then(t => {
                    this.__isEditing = t.default(page);
                });
            }
        }
    }
}