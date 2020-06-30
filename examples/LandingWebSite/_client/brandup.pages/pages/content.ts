import { Page, PageModel } from "brandup-ui-website";

class ContentPage extends Page<ContentPageModel> {
    get typeName(): string { return "BrandUpPages.ContentPage" }

    protected renderWebsiteToolbar() {
        //if (!this.model.editId)
        //    super.renderWebsiteToolbar();
    }
    protected onRenderContent() {
        if (this.nav.enableAdministration) {
            import("../admin/page").then(d => { new d.PageToolbar(this) });
        }
    }
}

export interface ContentPageModel extends PageModel {
    id: string;
    parentPageId: string;
    editId: string;
    status: "Draft" | "Published";
}

export default ContentPage;