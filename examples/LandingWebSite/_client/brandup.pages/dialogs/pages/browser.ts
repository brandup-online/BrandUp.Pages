import { DialogOptions } from "../dialog";
import { ListDialog } from "../dialog-list";
import { DOM } from "brandup-ui";
import { createPage } from "./create";
import { createPageCollection } from "../collections/create";
import { deletePage } from "./delete";
import { listPageCollection } from "../collections/list";

export class PageBrowserDialog extends ListDialog<PageListModel, PageModel> {
    private __pageId: string;
    private collectionId: string = null;
    private navElem: HTMLElement;
    private tabsElem: HTMLElement;
    private __createCollElem: HTMLElement;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.__pageId = pageId;

        this.setSorting(true);
    }

    get pageId(): string { return this.__pageId; }
    get typeName(): string { return "BrandUpPages.PageBrowserDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();

        this.setHeader("Страницы");
        this.setNotes("Просмотр и управление страницами.");

        this.registerCommand("nav", (elem) => {
            let pageId = elem.getAttribute("data-page-id");
            this.__pageId = pageId;
            this.collectionId = null;
            this.refresh();
        });
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
        if (this.__pageId)
            urlParams["pageId"] = this.__pageId;

        urlParams["collectionId"] = this.collectionId;
    }
    protected _buildList(model: PageListModel) {
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

        this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav" }, "root")));
        if (model.parents && model.parents.length) {
            for (let i = 0; i < model.parents.length; i++) {
                let pagePath = model.parents[i];
                this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav", "data-page-id": pagePath.id }, pagePath.header)));
            }
        }

        for (let i = 0; i < model.collections.length; i++) {
            let collection = model.collections[i];
            this.tabsElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-value": collection.id, "data-command": "select-collection" }, collection.title)));
        }

        let colId = this.collectionId;
        if (model.collections.length) {
            this.tabsElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "collection-sesttings", class: "secondary", title: "коллекций страниц" }, "коллекции")));

            if (!colId)
                colId = model.collections[0].id;
        }
        else {
            colId = null;

            this.__itemsElem.insertAdjacentElement("beforebegin", this.__createCollElem = DOM.tag("div", { class: "empty" }, [
                DOM.tag("div", { class: "text" }, "Для страницы не создано коллекций страниц."),
                DOM.tag("div", { class: "buttons" }, DOM.tag("button", { class: "bp-button", "data-command": "create-collection" }, "Создать коллекцию"))
            ]));
        }

        this.selectCollection(colId, false);
    }
    protected _getItemId(item: PageModel): string {
        return item.id;
    }
    protected _renderItemContent(item: PageModel, contentElem: HTMLElement) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, [
            DOM.tag("a", { href: "", "data-command": "nav", "data-page-id": item.id }, item.title),
            DOM.tag("div", { class: "text" }, item.url)
        ]));
        contentElem.appendChild(DOM.tag("div", { class: `status ${item.status.toLowerCase()}` }, item.status));
    }
    protected _renderItemMenu(item: PageModel, menuElem: HTMLElement) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-open" }, "Открыть")]));
        menuElem.appendChild(DOM.tag("li", { class: "split" }));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Удалить")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Страниц не создано.";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Новая страница"));
    }
}

interface PageListModel {
    parents: Array<PagePathModel>;
    collections: Array<PageCollectionModel>;
}

interface PagePathModel {
    id: string;
    header: string;
    url: string;
}

export var browserPage = (pageId: string) => {
    let dialog = new PageBrowserDialog(pageId);
    return dialog.open();
};