import Page from "./page";
import { PageClientModel } from "../typings/website";

class ContentPage extends Page<ContentPageModel>
{
    get typeName(): string { return "BrandUpPages.ContentPage" }

    protected renderWebsiteToolbar() {
        if (!this.model.editId)
            super.renderWebsiteToolbar();
    }
    protected onRenderContent() {
        if (this.app.navigation.enableAdministration) {
            import("../admin/page").then(d => { new d.PageToolbar(this) });
        }
    }
}

export interface ContentPageModel extends PageClientModel {
    id: string;
    parentPageId: string;
    editId: string;
    status: "Draft" | "Published";
}

export default ContentPage;