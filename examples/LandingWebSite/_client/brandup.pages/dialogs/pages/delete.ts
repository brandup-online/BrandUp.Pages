import { DeleteDialog } from "../dialog-delete";
import { DialogOptions } from "../dialog";

export class PageDeleteDialog extends DeleteDialog<PageModel> {
    readonly pageId: string;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageDeleteDialog"; }

    protected _onRenderContent() {
        this.setHeader("Удаление страницы");

        super._onRenderContent();
    }
    protected _getText(): string {
        return "Подтвердите удаление страницы.";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/page/${this.pageId}`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
    }
}

export var deletePage = (pageId: string) => {
    let dialog = new PageDeleteDialog(pageId);
    return dialog.open();
};