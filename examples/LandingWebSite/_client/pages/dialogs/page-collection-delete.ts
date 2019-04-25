import { DialogOptions } from "./dialog";
import { DeleteDialog } from "./dialog-delete";

class PageCollectionDeleteDialog extends DeleteDialog<PageCollectionModel> {
    readonly collectionId: string;

    constructor(collectionId: string, options?: DialogOptions) {
        super(options);

        this.collectionId = collectionId;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionDeleteDialog"; }

    protected _onRenderContent() {
        this.setHeader("Удаление коллекции страниц");

        super._onRenderContent();
    }
    protected _getText(): string {
        return "Подтвердите удаление коллекции страниц.";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/collection/${this.collectionId}`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
    }
}

export var deletePageCollection = (collectionId: string) => {
    let dialog = new PageCollectionDeleteDialog(collectionId);
    return dialog.open();
};