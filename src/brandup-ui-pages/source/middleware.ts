import { Application, ApplicationModel, Middleware, NavigateContext, StartContext } from "brandup-ui-app";
import { Page, PageModel } from "brandup-ui-website";

const UI = () => import("./ui");

export class PagesMiddleware extends Middleware<Application<ApplicationModel>, ApplicationModel> {
    private __isEditing: boolean = false;

    start(context: StartContext, next: VoidFunction, _end: () => void, error: (reason: any) => void) {
        this._showUI(context.data, next, error);
    }

    navigate(context: NavigateContext, next: VoidFunction, _end: () => void, error: (reason: any) => void) {
        this._showUI(context.data, next, error);
    }

    private _showUI(data: { [key: string]: any }, next: VoidFunction, error: (reason: any) => void) {
        if (document.body.dataset.pagesAdmin && data["page"]) {
            const page = data["page"] as Page<PageModel>;

            UI()
                .then(t => {
                    this.__isEditing = t.default(page);

                    next();
                })
                .catch(error);

            return;

        }

        next();
    }
}