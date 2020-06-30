import { Page, PageModel } from "brandup-ui-website";

class ContentPage extends Page<ContentPageModel> {
    get typeName(): string { return "BrandUpPages.ContentPage" }

    protected onRenderContent() {
        super.onRenderContent();

        if (this.nav.enableAdministration) {
            if (!this.model.editId) {
                import("../admin/website").then(d => {
                    this.attachDestroyElement(new d.WebSiteToolbar(this));
                });
            }

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