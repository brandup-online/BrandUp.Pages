import { Middleware, MiddlewareNext, NavigateContext, StartContext } from "@brandup/ui-app";
import { Page, PageModel,  } from "@brandup/ui-website";

const UI = () => import("./ui");

export class PagesMiddleware implements Middleware {
    private __isEditing: boolean = false;

    name = "brandup-layout-middleware";

    start(context: StartContext, next: MiddlewareNext) {
        const middleware = context.app.middleware('website-pages');
        return this._showUI(middleware, next);
    }

    navigate(context: NavigateContext, next: MiddlewareNext) {
        return this._showUI(context.data, next);
    }

    private _showUI(data: { [key: string]: any }, next: MiddlewareNext) {
        if (document.body.dataset.pagesAdmin && data["page"]) {
            const page = data["page"] as Page<PageModel>;

            return UI().then( t => {
                this.__isEditing = t.default(page);

                return next();
            })
            .catch(error => console.log(error));
        }

        return next();
    }
}