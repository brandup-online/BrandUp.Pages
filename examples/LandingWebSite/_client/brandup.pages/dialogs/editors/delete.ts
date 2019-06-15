import { DialogOptions } from "../dialog";
import { DeleteDialog } from "../dialog-delete";

class PageEditorDeleteDialog extends DeleteDialog<PageCollectionModel> {
    readonly editorId: string;

    constructor(editorId: string, options?: DialogOptions) {
        super(options);

        this.editorId = editorId;
    }

    get typeName(): string { return "BrandUpPages.PageEditorDeleteDialog"; }

    protected _onRenderContent() {
        this.setHeader("Удаление редактора");

        super._onRenderContent();
    }
    protected _getText(): string {
        return "Подтвердите удаление редактора страниц.";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/editor/${this.editorId}`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
    }
}

export var deletePageEditor = (editorId: string) => {
    let dialog = new PageEditorDeleteDialog(editorId);
    return dialog.open();
};