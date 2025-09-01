import { FieldDesigner } from "./base";
import { DOM } from "@brandup/ui-dom";
import { editPage } from "../../dialogs/pages/edit";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { ContentTypeModel } from "../../typings/models";
import "./model.less";
import { AjaxResponse, QueryData } from "@brandup/ui-ajax";

export class ModelDesigner extends FieldDesigner<ModelDesignerOptions> {
    get typeName(): string { return "BrandUpPages.ModelDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("content-designer");

        this.registerCommand("item-add", (ctx) => {
            const itemType = ctx.target.getAttribute("data-item-type");
            let itemIndex = -1;

            if (this.options.isListValue) {
                if (ctx.target.parentElement.hasAttribute("content-path-index"))
                    itemIndex = parseInt(ctx.target.parentElement.getAttribute("content-path-index")) + 1;
                else
                    itemIndex = this.countItems();
            }

            if (!itemType) {
                selectContentType(this.options.itemTypes).then((type) => {
                    this.addItem(type.name, itemIndex);
                });
            }
            else
                this.addItem(itemType, itemIndex);
        });
        this.registerCommand("item-view", () => { return; });
        this.registerCommand("item-settings", (ctx) => {
            const itemElem = ctx.target.closest("[content-path]");
            const contentPath = itemElem.getAttribute("content-path");

            editPage(this.page.editId, contentPath).then(() => {
                this.__refreshItem(itemElem);
            }).catch(() => {
                this.__refreshItem(itemElem);
            });
        });
        this.registerCommand("item-delete", (ctx) => {
            const itemElem = ctx.target.closest("[content-path-index]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            const itemIndex = itemElem.getAttribute("content-path-index");

            itemElem.remove();
            this._refreshBlockIndexes();
            this._renderBlocks();

            this.request({
                url: '/brandup.pages/content/model',
                query: { itemIndex: itemIndex },
                method: "DELETE",
                success: () => itemElem.classList.remove("processing")
            });
        });
        this.registerCommand("item-up", (ctx) => {
            const itemElem = ctx.target.closest("[content-path-index]");
            const itemIndex = itemElem.getAttribute("content-path-index");
            if (parseInt(itemIndex) <= 0)
                return;

            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            itemElem.previousElementSibling.insertAdjacentElement("beforebegin", itemElem);
            this._refreshBlockIndexes();
            this._renderBlocks();

            this.request({
                url: '/brandup.pages/content/model/up',
                query: { itemIndex: itemIndex },
                method: "POST",
                success: () => itemElem.classList.remove("processing")
            });
        });
        this.registerCommand("item-down", (ctx) => {
            const itemElem = ctx.target.closest("[content-path-index]");
            const itemIndex = itemElem.getAttribute("content-path-index");

            if (parseInt(itemIndex) >= DOM.queryElements(this.element, "* > [content-path-index]").length - 1)
                return;

            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            itemElem.nextElementSibling.insertAdjacentElement("afterend", itemElem);
            this._refreshBlockIndexes();
            this._renderBlocks();

            this.request({
                url: '/brandup.pages/content/model/down',
                query: { itemIndex: itemIndex },
                method: "POST",
                success: () => itemElem.classList.remove("processing")
            });
        });
        this.registerCommand("item-refresh", (ctx) => {
            const itemElem = ctx.target.closest("[content-path]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            this.__refreshItem(itemElem);
        });

        this._renderBlocks();
    }
    protected eachItems(f: (elem: Element, index: number) => void) {
        for (let i = 0; i < this.element.children.length; i++) {
            const itemElem = this.element.children.item(i);
            if (!itemElem.hasAttribute("content-path-index"))
                continue;
            f(itemElem, i);
        }
    }
    protected countItems(): number {
        let i = 0;
        this.eachItems(() => i++);
        return i;
    }
    protected getItem(index: number): Element {
        let itemElem: Element;

        for (let i = 0; i < this.element.children.length; i++) {
            itemElem = this.element.children.item(i);
            if (!itemElem.hasAttribute("content-path-index"))
                continue;

            if (i === index)
                break;
        }

        return itemElem;
    }
    protected _renderBlocks() {
        this.eachItems((elem) => { this._renderBlock(elem); });
    }
    protected _renderBlock(_itemElem: Element) { }
    protected _refreshBlockIndexes() {
        this.eachItems((elem, index) => { elem.setAttribute("content-path-index", index.toString()); });
    }
    private __refreshItem(elem: Element) {
        const urlParams: QueryData = {};
        if (this.options.isListValue)
            urlParams["itemIndex"] = elem.getAttribute("content-path-index") ?? "";

        this.request({
            url: '/brandup.pages/content/model/view',
            query: urlParams,
            method: "GET",
            success: (response: AjaxResponse<string>) => {
                if (response.status === 200) {
                    const fragment = document.createDocumentFragment();
                    const container = DOM.tag("div", null, response.data);
                    fragment.appendChild(container);

                    const newElem = DOM.queryElement(container, "[content-path]");
                    elem.insertAdjacentElement("afterend", newElem);
                    elem.remove();

                    this._renderBlock(newElem);
                    this.page.render();
                }
            }
        });
    }

    hasValue(): boolean {
        for (let i = 0; i < this.element.children.length; i++) {
            const itemElem = this.element.children.item(i);
            if (itemElem.hasAttribute("content-path-index"))
                return true;
        }
        return false;
    }
    addItem(itemType: string, index: number) {
        this.request({
            url: '/brandup.pages/content/model',
            query: {
                itemType: itemType,
                itemIndex: index.toString()
            },
            method: "PUT",
            success: (response: AjaxResponse<string>) => {
                if (response.status === 200) {
                    this.request({
                        url: '/brandup.pages/content/model/view',
                        query: { itemIndex: index.toString() },
                        method: "GET",
                        success: (response: AjaxResponse<string>) => {
                            if (response.status === 200) {
                                const fragment = document.createDocumentFragment();
                                const container = DOM.tag("div", null, response.data);
                                fragment.appendChild(container);
                                const newElem = DOM.queryElement(container, "[content-path]");

                                if (this.options.isListValue) {
                                    if (index > 0)
                                        this.getItem(index - 1).insertAdjacentElement("afterend", newElem);
                                    else if (index === 0)
                                        this.element.insertAdjacentElement("afterbegin", newElem);
                                    else
                                        this.getItem(-1).insertAdjacentElement("afterend", newElem);
                                }

                                this.page.render();

                                this._renderBlock(newElem);

                                this._refreshBlockIndexes();
                            }
                        }
                    });
                }
            }
        });
    }
}

export interface ModelDesignerOptions {
    addText: string;
    isListValue: boolean;
    itemType: ContentTypeModel;
    itemTypes: Array<ContentTypeModel>;
}