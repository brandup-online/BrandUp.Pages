import { Dialog, DialogOptions } from "./dialog";
import { ajaxRequest, DOM, AjaxQueue } from "brandup-ui";
import "./dialog-list.less";
import iconDots from "../svg/list-item-dots.svg";
import iconSort from "../svg/list-item-sort.svg";

export abstract class ListDialog<TList, TItem> extends Dialog<any> {
    protected __itemsElem: HTMLElement;
    private __newItemElem: HTMLElement;
    readonly queue: AjaxQueue;
    private __closeItemMenuFunc: (e: MouseEvent) => void;
    protected __model: TList;
    private __enableSorting: boolean = false;

    constructor(options?: DialogOptions) {
        super(options);

        this.queue = new AjaxQueue();
    }

    setSorting(enable: boolean) {
        this.__enableSorting = enable;
    }

    protected _onRenderContent() {
        this.element.classList.add("bp-dialog-list");
        
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

        this.__itemsElem.addEventListener("dragstart", (e: DragEvent) => {
            let target = <Element>e.target;
            let itemElem = target.closest("[data-index]");
            if (itemElem) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("data-id", itemElem.getAttribute("data-id"));
                e.dataTransfer.setData("data-index", itemElem.getAttribute("data-index"));
                e.dataTransfer.setDragImage(itemElem, 0, 0);
                return true;
            }
            e.stopPropagation();
            e.preventDefault();
            return false;
        }, false);
        this.__itemsElem.addEventListener("dragenter", (e: DragEvent) => {
            e.preventDefault();
            return true;
        });
        this.__itemsElem.addEventListener("dragover", (e: DragEvent) => {
            e.preventDefault();
        });
        this.__itemsElem.addEventListener("drop", (e: DragEvent) => {
                let target = <Element>e.target;
                var sourceId = e.dataTransfer.getData("data-id");
                var sourceIndex = parseInt(e.dataTransfer.getData("data-index"));
                let elem = target.closest("[data-index]");
                if (elem) {
                    let destId = elem.getAttribute("data-id");
                    let destIndex = parseInt(elem.getAttribute("data-index"));
                    if (destIndex !== sourceIndex) {
                        let sourceElem = DOM.queryElement(this.__itemsElem, `[data-index="${sourceIndex}"]`);
                        if (sourceElem) {
                            let destPosition: string;
                            if (destIndex < sourceIndex) {
                                elem.insertAdjacentElement("beforebegin", sourceElem);
                                destPosition = "before";
                            }
                            else {
                                elem.insertAdjacentElement("afterend", sourceElem);
                                destPosition = "after";
                            }

                            console.log(`Source: ${sourceIndex}; Dest: ${destIndex}; Position: ${destPosition}`);
                            console.log(`Source: ${sourceId}; Dest: ${destId}; Position: ${destPosition}`);

                            this.__refreshIndexes();

                            var urlParams: { [key: string]: string; } = {
                                sourceId: sourceId,
                                destId: destId,
                                destPosition: destPosition
                            };
                            this._buildUrlParams(urlParams);

                            var url = this._buildUrl();
                            url += "/sort";

                            this.setLoading(true);

                            ajaxRequest({
                                url: url,
                                urlParams: urlParams,
                                method: "POST",
                                success: (data: any, status: number) => {
                                    this.setLoading(false);

                                    if (status !== 200) {
                                        this.setError("Error loading items.");
                                        return;
                                    }

                                    this.loadItems();
                                }
                            });
                        }
                    }
                }

                e.stopPropagation();
                return false;
            });
    }

    private __refreshIndexes() {
        for (let i = 0; i < this.__itemsElem.childElementCount; i++) {
            let elem = this.__itemsElem.children.item(i);
            elem.setAttribute("data-index", i.toString());
        }
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
                let itemElem = DOM.tag("div", { class: "item", "data-id": itemId, "data-index": i.toString() });

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
        var sortElem: HTMLElement;
        var contentElem: HTMLElement;
        var menuElem: HTMLElement;

        if (this.__enableSorting) {
            elem.appendChild(contentElem = DOM.tag("div", { class: "sort", draggable: "true", title: "Нажмите, чтобы перетащить" }, iconSort));
        }

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