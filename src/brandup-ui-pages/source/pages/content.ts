import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";
import "../styles.less";

export class ContentPage extends Page<WebsiteApplication, ContentPageModel> {
    override get typeName(): string { return "BrandUp.ContentPage" }
}

export interface ContentPageModel extends PageModel {
    id: string;
    parentPageId: string;
    editId: string;
    status: "Draft" | "Published";
}