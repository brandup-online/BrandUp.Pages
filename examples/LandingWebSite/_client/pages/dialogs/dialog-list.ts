import { Dialog, DialogOptions } from "./dialog";
import { ajaxRequest, DOM, AjaxQueue } from "brandup-ui";
import "./dialog-list.less";
import iconDots from "../svg/list-item-dots.svg";

export abstract class ListDialog<TList, TItem> extends Dialog<any> {
    protected __itemsElem: HTMLElement;
    private __newItemElem: HTMLElement;
    readonly queue: AjaxQueue;
    private __closeItemMenuFunc: (e: MouseEvent) => void;
    protected __model: TList;

    constructor(options?: DialogOptions) {
        super(options);

        this.queue = new AjaxQueue();
    }
    
    protected _onRenderContent() {
        this.element.classList.add("website-dialog-list");
        
        this.__itemsElem = DOM.tag("div", { class: "items" });
        this.content.appendChild(this.__itemsElem);
        
        this.registerCommand("item-open-menu", (el: HTMLElement) => {
            el.parentElement.parentElement.classList.add("opened-menu");
        });

        this.__closeItemMenuFunc = (e: MouseEvent) => {
            let itemElem = (<HTMLElement>e.target).closest(".opened-menu");
            if (!itemElem)
                this.__hideItemMenu();
        };

        document.body.addEventListener("mousedown", this.__closeItemMenuFunc);

        this.__loadList();
    }

    private __loadList() {
        var urlParams: { [key: string]: string; } = {};

        this._buildUrlParams(urlParams);

        this.setLoading(true);

        this.queue.request({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "GET",
            success: (data: any, status: number) => {
                this.setLoading(false);

                switch (status) {
                    case 200: {
                        this.__model = <TList>data;
                        this._buildList(this.__model);
                        this.loadItems();
                        break;
                    }
                    default: {
                        this.setError("Error loading list.");
                        return;
                    }
                }
            }
        });
    }

    refresh() {
        this.__loadList();
    }
    loadItems() {
        if (!this._allowLoadItems()) {
            DOM.empty(this.__itemsElem);

            if (this.__newItemElem) {
                this.__newItemElem.remove();
                this.__newItemElem = null;
            }

            return;
        }

        this.setLoading(true);

        var urlParams: { [key: string]: string; } = {};
        this._buildUrlParams(urlParams);

        urlParams["offset"] = "0";
        urlParams["limit"] = "50";

        var url = this._buildUrl();
        url += "/item";

        ajaxRequest({
            url: url,
            urlParams: urlParams,
            success: (data: Array<TItem>, status: number) => {
                this.setLoading(false);

                if (status !== 200) {
                    this.setError("Error loading items.");
                    return;
                }

                this.__renderItems(data);
            }
        });
    }
    protected registerItemCommand(name: string, execute: (itemId: string, model: any, commandElem: HTMLElement) => void, canExecute?: (itemId: string, model: any, commandElem: HTMLElement) => boolean) {
        this.registerCommand(name, (elem: HTMLElement) => {
            let item = this._findItemIdFromElement(elem);
            if (item === null)
                return;

            execute(item.id, item.model, elem);
        }, (elem: HTMLElement) => {
            if (!canExecute)
                return true;
            
            let item = this._findItemIdFromElement(elem);
            if (item === null)
                return false;

            return canExecute(item.id, item.model, elem);
        });
    }
    protected _findItemIdFromElement(elem: HTMLElement): { id: string; model: any; } {
        let itemElem = elem.closest(".item[data-id]");
        if (!itemElem)
            return null;

        return { id: itemElem.getAttribute("data-id"), model: itemElem["_model_"] };
    }
    
    private __renderItems(items: Array<TItem>) {
        var fragment = document.createDocumentFragment();

        if (items && items.length) {
            for (let i = 0; i < items.length; i++) {
                let item = items[i];
                let itemId = this._getItemId(item);
                let itemElem = DOM.tag("div", { class: "item", "data-id": itemId });

                itemElem["_model_"] = item;

                this.__renderItem(itemId, item, itemElem);

                fragment.appendChild(itemElem);
            }
        }
        else {
            let emptyContainer = DOM.tag("div", { class: "empty" });
            this._renderEmpty(emptyContainer);
            fragment.appendChild(emptyContainer);
        }

        DOM.empty(this.__itemsElem);
        this.__itemsElem.appendChild(fragment);

        if (!this.__newItemElem) {
            this.__newItemElem = DOM.tag("div", { class: "new-item" });
            this.content.appendChild(this.__newItemElem);
            this._renderNewItem(this.__newItemElem);
        }
    }
    private __renderItem(itemId: string, item: TItem, elem: HTMLElement) {
        var contentElem: HTMLElement;
        var menuElem: HTMLElement;

        elem.appendChild(contentElem = DOM.tag("div", { class: "content" }));

        elem.appendChild(DOM.tag("div", { class: "menu" }, [
            DOM.tag("button", { "data-command": "item-open-menu" }, iconDots),
            menuElem = DOM.tag("ul")
        ]));

        this._renderItemContent(item, contentElem);
        this._renderItemMenu(item, menuElem);
    }
    private __hideItemMenu() {
        DOM.removeClass(this.__itemsElem, ".opened-menu", "opened-menu");
    }

    protected abstract _buildUrl(): string;
    protected _buildUrlParams(urlParams: { [key: string]: string; }) { }
    protected abstract _buildList(model: TList);
    protected _allowLoadItems(): boolean { return true; }
    protected abstract _getItemId(item: TItem): string;
    protected abstract _renderItemContent(item: TItem, contentElem: HTMLElement);
    protected abstract _renderItemMenu(item: TItem, menuElem: HTMLElement);
    protected abstract _renderEmpty(container: HTMLElement);
    protected abstract _renderNewItem(containerElem: HTMLElement);

    destroy() {
        document.body.removeEventListener("mousedown", this.__closeItemMenuFunc);

        this.queue.destroy();

        super.destroy();
    }
}