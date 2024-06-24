import { Page, PageModel } from "brandup-ui-website";

class ContentPage extends Page<ContentPageModel> {
    get typeName(): string { return "BrandUp.ContentPage" }
}

export interface ContentPageModel extends PageModel {
    id: string;
    parentPageId: string;
    status: "Draft" | "Published";
}

export default ContentPage;