import { DialogOptions } from "./dialog";
import { ajaxRequest, DOM } from "brandup-ui";
import { createPageCollection } from "./page-collection-create";
import { deletePageCollection } from "./page-collection-delete";
import { updatePageCollection } from "./page-collection-update";
import { ListDialog } from "./dialog-list";
import { listPage } from "./page-list";

export class PageCollectionListDialog extends ListDialog<PageCollectionModel> {
    readonly pageId: string;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionListDialog"; }

    protected _onRenderContent() {
        this.setHeader("Коллекции страниц");
        this.setNotes("Просмотр и управление коллекциями страниц.");

        super._onRenderContent();
        
        this.registerCommand("item-create", () => {
            createPageCollection(this.pageId).then((pageCollection: PageCollectionModel) => {
                this.loadItems();
            });
        });
        this.registerItemCommand("item-update", (itemId: string, el: HTMLElement) => {
            updatePageCollection(itemId).then((pageCollection: PageCollectionModel) => {
                this.loadItems();
            });
        });
        this.registerItemCommand("item-delete", (itemId: string, el: HTMLElement) => {
            deletePageCollection(itemId).then((id: string) => {
                this.loadItems();
            });
        });
        this.registerItemCommand("page-list", (itemId: string) => {
            listPage(itemId);
        });
        
        this.loadItems();
    }
    
    protected _getItemsUrl(): string {
        return `/brandup.pages/collection`;
    }
    protected _onSetUrlParams(urlParams: { [key: string]: string; }) {
        if (this.pageId)
            urlParams["pageId"] = this.pageId;
    }
    protected _getItemId(item: PageCollectionModel): string {
        return item.id;
    }
    protected _renderItemContent(item: PageCollectionModel, contentElem: HTMLElement) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("a", { href: "", "data-command": "page-list" }, item.title)));
    }
    protected _renderItemMenu(item: PageCollectionModel, menuElem: HTMLElement) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "page-list" }, "View pages")]));
        menuElem.appendChild(DOM.tag("li", { class: "split" }));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-update" }, "Edit")]));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Коллекций не создано.";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Создать коллекцию страниц"));
    }
}

export var listPageCollection = (pageId: string) => {
    let dialog = new PageCollectionListDialog(pageId);
    return dialog.open();
};