import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";

class ContentPage extends Page<WebsiteApplication, ContentPageModel> {
    get typeName(): string { return "BrandUp.ContentPage" }
}

export interface ContentPageModel extends PageModel {
    id: string;
    parentPageId: string;
    status: "Draft" | "Published";
}

export default ContentPage;