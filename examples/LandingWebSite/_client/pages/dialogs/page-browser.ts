import { DialogOptions } from "./dialog";
import { ListDialog } from "./dialog-list";
import { DOM, ajaxRequest } from "brandup-ui";
import { createPage } from "./page-create";
import { deletePage } from "./page-delete";

export class PageBrowserDialog extends ListDialog<PageModel> {
    readonly pageId: string;
    private collectionId: string;
    private tabsElem: HTMLElement;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageBrowserDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();

        this.setHeader("Страницы коллекции");
        this.setNotes("Просмотр и управление страницами.");

        this.content.insertAdjacentElement("afterbegin", this.tabsElem = DOM.tag("ul", { class: "tabs" }));

        this.registerCommand("item-create", () => {
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

        var urlParams = {};
        if (this.pageId)
            urlParams["pageId"] = this.pageId;

        ajaxRequest({
            url: "/brandup.pages/collection",
            urlParams: urlParams,
            success: (data: Array<PageCollectionModel>, status: number) => {
                this.setLoading(false);

                if (status !== 200) {
                    this.setError("Error loading items.");
                    return;
                }

                for (let i = 0; i < data.length; i++) {
                    let collection = data[i];
                    this.tabsElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-value": collection.id, "data-command": "select-collection" }, collection.title)));
                }

                if (data.length)
                    this.selectCollection(data[0].id);
            }
        });

        this.registerCommand("select-collection", (elem) => {
            let collectionId = elem.getAttribute("data-value");
            this.selectCollection(collectionId);
        });
    }

    private selectCollection(collectionId: string) {
        if (this.collectionId) {
            DOM.removeClass(this.tabsElem, "a[data-value]", "selected");
        }

        this.collectionId = collectionId;

        if (this.collectionId)
            DOM.queryElement(this.tabsElem, `a[data-value="${this.collectionId}"]`).classList.add("selected");

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
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Страниц не создано.";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Новая страница"));
    }
}

export var browserPage = (pageId: string) => {
    let dialog = new PageBrowserDialog(pageId);
    return dialog.open();
};