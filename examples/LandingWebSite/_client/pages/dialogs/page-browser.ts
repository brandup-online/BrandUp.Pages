import { DialogOptions } from "./dialog";
import { ListDialog } from "./dialog-list";
import { DOM, ajaxRequest } from "brandup-ui";
import { createPage } from "./page-create";
import { deletePage } from "./page-delete";
import { listPageCollection } from "./page-collection-list";
import { createPageCollection } from "./page-collection-create";

export class PageBrowserDialog extends ListDialog<PageListModel, PageModel> {
    readonly pageId: string;
    private collectionId: string = null;
    private navElem: HTMLElement;
    private tabsElem: HTMLElement;
    private __createCollElem: HTMLElement;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageBrowserDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();

        this.registerCommand("item-create", () => {
            if (!this.collectionId)
                return;

            createPage(this.collectionId).then((createdItem: PageModel) => {
                this.loadItems();
            });
        });
        this.registerItemCommand("item-open", (itemId: string, model: PageModel) => {
            location.href = model.url;
        });
        this.registerItemCommand("item-delete", (itemId: string) => {
            deletePage(itemId).then((deletedItem: PageModel) => {
                this.loadItems();
            });
        });
        this.registerCommand("select-collection", (elem) => {
            let collectionId = elem.getAttribute("data-value");
            this.selectCollection(collectionId, true);
        });
        this.registerCommand("collection-sesttings", () => {
            listPageCollection(this.pageId).then(() => {
                this.refresh();
            });
        });
        this.registerCommand("create-collection", () => {
            createPageCollection(this.pageId).then(() => {
                this.refresh();
            });
        });
    }

    private selectCollection(collectionId: string, needLoad: boolean) {
        if (this.collectionId)
            DOM.removeClass(this.tabsElem, "a[data-value]", "selected");

        this.collectionId = collectionId;

        if (this.collectionId) {
            let tabItem = DOM.queryElement(this.tabsElem, `a[data-value="${this.collectionId}"]`);
            if (tabItem)
                tabItem.classList.add("selected");
            else
                this.collectionId = null;
        }

        if (needLoad)
            this.loadItems();
    }

    protected _allowLoadItems(): boolean {
        return this.collectionId ? true : false;
    }
    protected _buildUrl(): string {
        return `/brandup.pages/page/list`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
        if (this.pageId)
            urlParams["pageId"] = this.pageId;

        urlParams["collectionId"] = this.collectionId;
    }
    protected _buildList(model: PageListModel) {
        this.setHeader("Страницы");
        this.setNotes("Просмотр и управление страницами.");

        if (this.__createCollElem) {
            this.__createCollElem.remove();
            this.__createCollElem = null;
        }

        if (!this.tabsElem) {
            this.navElem = DOM.tag("ol", { class: "nav" })
            this.content.insertAdjacentElement("afterbegin", this.navElem);

            this.tabsElem = DOM.tag("ul", { class: "tabs" })
            this.navElem.insertAdjacentElement("afterend", this.tabsElem);
        }
        else {
            DOM.empty(this.tabsElem);
            DOM.empty(this.navElem);
        }

        this.navElem.appendChild(DOM.tag("li", null, DOM.tag("span", { class: "root" }, "root")));
        if (model.parents && model.parents.length) {
            for (let i = 0; i < model.parents.length; i++) {
                this.navElem.appendChild(DOM.tag("li", null, DOM.tag("span", {}, model.parents[i])));
            }
        }

        for (let i = 0; i < model.collections.length; i++) {
            let collection = model.collections[i];
            this.tabsElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-value": collection.id, "data-command": "select-collection" }, collection.title)));
        }

        let colId = this.collectionId;
        if (model.collections.length) {
            this.tabsElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "collection-sesttings" }, "настройки")));

            if (!colId)
                colId = model.collections[0].id;
        }
        else {
            colId = null;

            this.__itemsElem.insertAdjacentElement("beforebegin", this.__createCollElem = DOM.tag("div", { class: "empty" }, [
                DOM.tag("div", { class: "text" }, "Для страницы не созрано коллекций страниц."),
                DOM.tag("div", { class: "buttons" }, DOM.tag("button", { class: "brandup-pages-button", "data-command": "create-collection" }, "Создать коллекцию"))
            ]));
        }

        this.selectCollection(colId, false);
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
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Страниц не создано.";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Новая страница"));
    }
}

interface PageListModel {
    parents: Array<string>;
    collections: Array<PageCollectionModel>;
}

export var browserPage = (pageId: string) => {
    let dialog = new PageBrowserDialog(pageId);
    return dialog.open();
};