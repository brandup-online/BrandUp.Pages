import { Dialog, DialogOptions } from "./dialog";
import { request, AjaxQueue, AjaxResponse } from "@brandup/ui-ajax";
import "./dialog-list.less";
import iconDots from "../svg/list-item-dots.svg";
import iconSort from "../svg/list-item-sort.svg";
import { DOM } from "@brandup/ui-dom";
import { CommandContext } from "@brandup/ui";

export abstract class ListDialog<TList, TItem> extends Dialog {
    protected __itemsElem: HTMLElement;
    private __newItemElem?: HTMLElement;
    readonly queue: AjaxQueue;
    private __closeItemMenuFunc: (e: MouseEvent) => void = () => {};
    protected __model?: TList;
    private __enableSorting = false;

    constructor(options?: DialogOptions) {
        super(options);

        this.__itemsElem = DOM.tag("div", { class: "items" });

        this.queue = new AjaxQueue();
    }

    setSorting(enable: boolean) {
        this.__enableSorting = enable;
    }

    protected _onRenderContent() {
        this.element?.classList.add("bp-dialog-list");

        this.content?.appendChild(this.__itemsElem);

        this.registerCommand("item-open-menu", (context: CommandContext) => {
            context.target.parentElement?.parentElement?.classList.add("opened-menu");
        });

        this.__closeItemMenuFunc = (e: MouseEvent) => {
            const itemElem = (e.target as HTMLElement).closest(".opened-menu");
            if (!itemElem)
                this.__hideItemMenu();
        };

        document.body.addEventListener("mousedown", this.__closeItemMenuFunc);

        this.__loadList();

        this.__itemsElem.addEventListener("dragstart", (e: DragEvent) => {
            const target = e.target as Element;
            const itemElem = target.closest("[data-index]");
            if (itemElem && e.dataTransfer && itemElem.hasAttribute("data-id") && itemElem.hasAttribute("data-id")) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("data-id", itemElem.getAttribute("data-id")!);
                e.dataTransfer.setData("data-index", itemElem.getAttribute("data-index")!);
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
        this.__itemsElem.addEventListener("drop", async (e: DragEvent) => {
            const target = e.target as Element;
            const sourceId = e.dataTransfer?.getData("data-id");
            const sourceIndex = parseInt(e.dataTransfer?.getData("data-index") || "-1");
            const elem = target.closest("[data-index]");
            if (elem && sourceId && sourceIndex >= 0) {
                const destId = elem.getAttribute("data-id");
                const destIndex = parseInt(elem.getAttribute("data-index") || "-1");
                if (destIndex !== sourceIndex && destId && destIndex >= 0) {
                    const sourceElem = DOM.queryElement(this.__itemsElem, `[data-index="${sourceIndex}"]`);
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

                        const urlParams: { [key: string]: string } = {
                            sourceId: sourceId,
                            destId: destId,
                            destPosition: destPosition
                        };
                        this._buildUrlParams(urlParams);

                        let url = this._buildUrl();
                        url += "/sort";

                        this.setLoading(true);

                        const response: AjaxResponse = await this.queue.enque({
                            url: url,
                            query: urlParams,
                            method: "POST",
                        });

                        this.setLoading(false);

                        if (response.status !== 200) {
                            this.setError("Error loading items.");
                            return;
                        }

                        this.loadItems();
                    }
                }
            }

            e.stopPropagation();
            return false;
        });
    }

    private __refreshIndexes() {
        for (let i = 0; i < this.__itemsElem.childElementCount; i++) {
            const elem = this.__itemsElem.children.item(i);
            elem?.setAttribute("data-index", i.toString());
        }
    }
    private __loadList() {
        const urlParams: { [key: string]: string } = {};
        this._buildUrlParams(urlParams);

        this.setLoading(true);

        this.queue.enque({
            url: this._buildUrl(),
            query: urlParams,
            method: "GET",
        }).then((response: AjaxResponse<TList>) => {
            this.setLoading(false);

            switch (response.status) {
                case 200: {
                    if (!response.data) throw new Error("data loading error");

                    this.__model = response.data;
                    this._buildList(this.__model);
                    this.loadItems();
                    break;
                }
                default: {
                    this.setError("Error loading list.");
                    return;
                }
            }
        });
    }

    refresh() {
        this.__loadList();
    }
    async loadItems() {
        if (!this._allowLoadItems()) {
            DOM.empty(this.__itemsElem);

            if (this.__newItemElem) {
                this.__newItemElem.remove();
                this.__newItemElem = undefined;
            }

            return;
        }

        this.setLoading(true);

        const urlParams: { [key: string]: string } = {};
        this._buildUrlParams(urlParams);

        urlParams["offset"] = "0";
        urlParams["limit"] = "50";

        let url = this._buildUrl();
        url += "/item";

        const response: AjaxResponse<Array<TItem>> = await this.queue.enque({
            url: url,
            query: urlParams,
        });

        this.setLoading(false);

        if (response.status !== 200 || !response.data) {
            this.setError("Error loading items.");
            return;
        }

        this.__renderItems(response.data);
    }
    protected registerItemCommand(name: string, execute: (itemId: string, model: any, commandElem: HTMLElement) => void, canExecute?: (itemId: string, model: any, commandElem: HTMLElement) => boolean) {
        this.registerCommand(name, (context: CommandContext) => {
            const item = this._findItemIdFromElement(context.target);
            if (item === null)
                return;

            execute(item.id, item.model, context.target);
        }, (context: CommandContext) => {
            if (!canExecute)
                return true;

            const item = this._findItemIdFromElement(context.target);
            if (item === null)
                return false;

            return canExecute(item.id, item.model, context.target);
        });
    }
    protected _findItemIdFromElement(elem: HTMLElement): { id: string; model: any; } {
        const itemElem = elem.closest(".item[data-id]");
        if (!itemElem)
            throw new Error("");

        return { id: itemElem.getAttribute("data-id")!, model: (itemElem as any)["_model_"] };
    }

    private __renderItems(items: Array<TItem>) {
        const fragment = document.createDocumentFragment();

        if (items && items.length) {
            for (let i = 0; i < items.length; i++) {
                const item = items[i];
                const itemId = this._getItemId(item);
                const itemElem = DOM.tag("div", { class: "item", "data-id": itemId, "data-index": i.toString() });

                (itemElem as any)["_model_"] = item;

                this.__renderItem(itemId, item, itemElem);

                fragment.appendChild(itemElem);
            }
        }
        else {
            const emptyContainer = DOM.tag("div", { class: "empty" });
            this._renderEmpty(emptyContainer);
            fragment.appendChild(emptyContainer);
        }

        DOM.empty(this.__itemsElem);
        this.__itemsElem.appendChild(fragment);

        if (!this.__newItemElem) {
            this.__newItemElem = DOM.tag("div", { class: "new-item" });
            this.content?.appendChild(this.__newItemElem);
            this._renderNewItem(this.__newItemElem);
        }
    }
    private __renderItem(_itemId: string, item: TItem, elem: HTMLElement) {
        let contentElem: HTMLElement;
        let menuElem: HTMLElement;

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
    protected _buildUrlParams(_urlParams: { [key: string]: string }) { }
    protected abstract _buildList(model: TList): void;
    protected _allowLoadItems(): boolean { return true; }
    protected abstract _getItemId(item: TItem): string;
    protected abstract _renderItemContent(item: TItem, contentElem: HTMLElement): void;
    protected abstract _renderItemMenu(item: TItem, menuElem: HTMLElement): void;
    protected abstract _renderEmpty(container: HTMLElement): void;
    protected abstract _renderNewItem(containerElem: HTMLElement): void;

    destroy() {
        document.body.removeEventListener("mousedown", this.__closeItemMenuFunc);

        this.queue.destroy();

        super.destroy();
    }
}