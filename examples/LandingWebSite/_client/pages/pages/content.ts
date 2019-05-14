import Page from "./page";
import { PageClientModel } from "../typings/website";
import { UIElement } from "brandup-ui";

class ContentPage extends Page<ContentPageModel>
{
    private __pageToolbar: UIElement;

    get typeName(): string { return "BrandUpPages.ContentPage" }

    protected renderWebsiteToolbar() {
        if (!this.model.editId)
            super.renderWebsiteToolbar();
    }
    protected onRenderContent() {
        if (this.app.navigation.enableAdministration) {
            import("../admin/page").then(d => {
                this.__pageToolbar = new d.PageToolbar(this);
            });
        }
    }

    destroy() {
        if (this.__pageToolbar) {
            this.__pageToolbar.destroy();
            this.__pageToolbar = null;
        }

        super.destroy();
    }
}

export interface ContentPageModel extends PageClientModel {
    id: string;
    parentPageId: string;
    editId: string;
    status: "Draft" | "Published";
}

export default ContentPage;