import { Middleware, NavigateContext, NavigatingContext } from "brandup-ui-app";
import { Page, PageModel } from "brandup-ui-website";
import ContentPage from "./pages/content";
import "./styles.less";

export class ContentMiddleware extends Middleware {
    navigating(context: NavigatingContext, next) {
        next();

        //if (context.items["nav"].enableAdministration) {
            //const page = context.items["page"] as Page<PageModel>;
            //if (page.model.editId)
            //    context.isCancel = true;
        //}
    }
    navigate(context: NavigateContext, next) {
        next();

        console.log(context.items);

        if (context.items["nav"].enableAdministration) {
            const page = context.items["page"] as Page<PageModel>;
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