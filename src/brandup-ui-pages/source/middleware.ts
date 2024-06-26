import { Middleware, NavigateContext } from "brandup-ui-app";
import { Page, PageModel } from "brandup-ui-website";

const UI = () => import("./ui");

export class PagesMiddleware extends Middleware {
    start(context, next) {
        next();

        this._showUI(context.items);
    }

    navigate(context: NavigateContext, next) {
        next();
        
        this._showUI(context.items);
    }

    private _showUI(items: { [key: string]: any }) {
        if (items["nav"].enableAdministration) {
            const page = items["page"] as Page<PageModel>;
            if (page) {
                UI().then(t => {
                    t.default(page);
                });
            }
        }
    }
}