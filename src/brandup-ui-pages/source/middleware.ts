﻿import { Middleware } from "brandup-ui-app";
import { Page, PageModel } from "brandup-ui-website";
import ContentPage from "./pages/content";
import "./styles.less";

export class PagesMiddleware extends Middleware {
    start(context, next) {
        next();

        this._showUI(context.items);
    }

    navigate(context, next) {
        next();

        this._showUI(context.items);
    }

    private _showUI(items: { [key: string]: any }) {
        if (items["nav"].enableAdministration) {
            const page = items["page"] as Page<PageModel>;
            if (!page.model.editId) {
                import("./admin/widget").then(d => {
                    page.attachDestroyElement(new d.EditorWidget(page));
                });
            }
            else {
                import("./admin/toolbar").then(d => {
                    page.attachDestroyElement(new d.EditorToolbar(page as ContentPage));
                });
            }
        }
    }
}