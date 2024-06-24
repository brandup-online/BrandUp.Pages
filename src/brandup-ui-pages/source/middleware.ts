import { Middleware, NavigateContext } from "brandup-ui-app";
import { Page, PageModel } from "brandup-ui-website";
import { DOM } from "brandup-ui-dom";

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
            const editingContentElem = DOM.queryElement(document.body, "[data-content-edit-id]");
            if (!editingContentElem) {
                import("./admin/page-toolbar").then(d => {
                    page.attachDestroyElement(new d.PageToolbar(page));
                });
            }
            else {
                import("./admin/editor-toolbar").then(d => {
                    page.attachDestroyElement(new d.EditorToolbar(page, editingContentElem));
                });
            }
        }
    }
}