import { Middleware, NavigateContext, NavigatingContext, StartContext } from "brandup-ui-app";
import { Page, PageModel } from "brandup-ui-website";
import ContentPage from "./pages/content";
import "./styles.less";

export class ContentMiddleware extends Middleware {
    start(context: StartContext, next) {
        next();

        this._showUI(context.items);
    }
    navigating(context: NavigatingContext, next) {
        //if (context.items["nav"].enableAdministration) {
        //    const page = context.items["page"] as Page<PageModel>;
        //    if (page.model.editId)
        //        context.isCancel = true;
        //}

        next();
    }
    navigate(context: NavigateContext, next) {
        next();

        this._showUI(context.items);
    }

    private _showUI(items: { [key: string]: any }) {
        if (items["nav"].enableAdministration) {
            const page = items["page"] as Page<PageModel>;
            if (!page.model.editId) {
                import("./admin/website").then(d => {
                    page.attachDestroyElement(new d.WebSiteToolbar(page));
                });
            }

            if (page instanceof ContentPage) {
                import("./admin/page").then(d => {
                    page.attachDestroyElement(new d.PageToolbar(page as ContentPage));
                });
            }
        }
    }
}