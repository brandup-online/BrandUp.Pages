import Page from "./page";
import { PageClientModel } from "../typings/website";

class ContentPage extends Page<ContentPageModel>
{
    get typeName(): string { return "BrandUpPages.ContentPage" }

    protected onRenderContent() {
        import("../root").then(d => {
            d.default.load(this);
        });
    }
}

export interface ContentPageModel extends PageClientModel {
    id: string;
    parentPageId: string;
    editId: string;
    status: "Draft" | "Published";
}

export default ContentPage;