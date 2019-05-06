import { DialogOptions } from "./dialog";
import { ListDialog } from "./dialog-list";
import { DOM } from "brandup-ui";
import { createPage } from "./page-create";
import { deletePage } from "./page-delete";

export class PageListDialog extends ListDialog<PageModel> {
    readonly collectionId: string;

    constructor(collectionId: string, options?: DialogOptions) {
        super(options);

        this.collectionId = collectionId;
    }

    get typeName(): string { return "BrandUpPages.PageListDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();

        this.setHeader("Страницы коллекции");
        this.setNotes("Просмотр и управление страницами.");

        this.registerCommand("item-create", () => {
            createPage(this.collectionId).then((createdItem: PageModel) => {
                this.loadItems();
            });
        });
        this.registerItemCommand("item-open", (itemId: string, model: PageModel) => {
            location.href = model.url;
        });
        this.registerItemCommand("item-update", (itemId: string) => { });
        this.registerItemCommand("item-delete", (itemId: string) => {
            deletePage(itemId).then((deletedItem: PageModel) => {
                this.loadItems();
            });
        });

        this.loadItems();
    }

    protected _getItemsUrl(): string {
        return `/brandup.pages/page`;
    }
    protected _onSetUrlParams(urlParams: { [key: string]: string; }) {
        urlParams["collectionId"] = this.collectionId;
    }
    protected _getItemId(item: PageModel): string {
        return item.id;
    }
    protected _renderItemContent(item: PageModel, contentElem: HTMLElement) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("a", { href: "", "data-command": "item-open" }, item.title)));
        contentElem.appendChild(DOM.tag("div", { class: `status ${item.status.toLowerCase()}` }, item.status));
    }
    protected _renderItemMenu(item: PageModel, menuElem: HTMLElement) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-open" }, "Open")]));
        menuElem.appendChild(DOM.tag("li", { class: "split" }));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-update" }, "Edit")]));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Страниц не создано.";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Новая страница"));
    }
}

export var listPage = (pageId: string) => {
    let dialog = new PageListDialog(pageId);
    return dialog.open();
};