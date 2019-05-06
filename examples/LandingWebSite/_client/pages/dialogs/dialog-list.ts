import { Dialog, DialogOptions } from "./dialog";
import { ajaxRequest, DOM } from "brandup-ui";
import "./dialog-list.less";

export abstract class ListDialog<TItem> extends Dialog<any> {
    private __itemsElem: HTMLElement;
    private __closeItemMenuFunc: (e: MouseEvent) => void;
    
    protected _onRenderContent() {
        this.element.classList.add("website-dialog-list");
        
        this.__itemsElem = DOM.tag("div", { class: "items" });
        this.content.appendChild(this.__itemsElem);
        
        let newItemElem = DOM.tag("div", { class: "new-item" });
        this.content.appendChild(newItemElem);
        this._renderNewItem(newItemElem);
        
        this.registerCommand("item-open-menu", (el: HTMLElement) => {
            el.parentElement.parentElement.classList.add("opened-menu");
        });

        this.__closeItemMenuFunc = (e: MouseEvent) => {
            let itemElem = (<HTMLElement>e.target).closest(".opened-menu");
            if (!itemElem)
                this.__hideItemMenu();
        };

        document.body.addEventListener("mousedown", this.__closeItemMenuFunc);
    }

    loadItems() {
        this.setLoading(true);

        var urlParams: { [key: string]: string; } = {};

        this._onSetUrlParams(urlParams);

        ajaxRequest({
            url: this._getItemsUrl(),
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
    }
    private __renderItem(itemId: string, item: TItem, elem: HTMLElement) {
        var contentElem: HTMLElement;
        var menuElem: HTMLElement;

        elem.appendChild(contentElem = DOM.tag("div", { class: "content" }));

        elem.appendChild(DOM.tag("div", { class: "menu" }, [
            DOM.tag("button", { "data-command": "item-open-menu" }),
            menuElem = DOM.tag("ul")
        ]));

        this._renderItemContent(item, contentElem);
        this._renderItemMenu(item, menuElem);
    }
    private __hideItemMenu() {
        DOM.removeClass(this.__itemsElem, ".opened-menu", "opened-menu");
    }

    protected abstract _getItemsUrl(): string;
    protected abstract _onSetUrlParams(urlParams: { [key: string]: string; });
    protected abstract _getItemId(item: TItem): string;
    protected abstract _renderItemContent(item: TItem, contentElem: HTMLElement);
    protected abstract _renderItemMenu(item: TItem, menuElem: HTMLElement);
    protected abstract _renderEmpty(container: HTMLElement);
    protected abstract _renderNewItem(containerElem: HTMLElement);

    destroy() {
        document.body.removeEventListener("mousedown", this.__closeItemMenuFunc);

        super.destroy();
    }
}