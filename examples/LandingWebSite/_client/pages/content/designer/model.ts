import { FieldDesigner } from "./base";
import { DOM } from "brandup-ui";
import { editPage } from "../../dialogs/page-edit";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import "./model.less";

export class ModelDesigner extends FieldDesigner<ModelDesignerOptions> {
    get typeName(): string { return "BrandUpPages.ContentDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("content-designer");
        
        this.registerCommand("item-add", (elem: HTMLElement) => {
            let itemType = elem.getAttribute("data-item-type");
            let itemIndex = -1;

            if (this.options.isListValue) {
                if (elem.parentElement.hasAttribute("content-path-index"))
                    itemIndex = parseInt(elem.parentElement.getAttribute("content-path-index")) + 1;
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
        this.registerCommand("item-view", () => { });
        this.registerCommand("item-settings", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path]");
            let contentPath = itemElem.getAttribute("content-path");

            editPage(this.page.editId, contentPath).then(() => {
                this.__refreshItem(itemElem);
            }).catch(() => {
                this.__refreshItem(itemElem);
            });
        });
        this.registerCommand("item-delete", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            let itemIndex = itemElem.getAttribute("content-path-index");

            itemElem.remove();
            this._refreshBlockIndexes();
            this._renderBlocks();

            this.request({
                url: '/brandup.pages/content/model',
                urlParams: { itemIndex: itemIndex },
                method: "DELETE",
                success: () => itemElem.classList.remove("processing")
            });
        });
        this.registerCommand("item-up", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");
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
                urlParams: { itemIndex: itemIndex },
                method: "POST",
                success: () => itemElem.classList.remove("processing")
            });
        });
        this.registerCommand("item-down", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");
            
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
                urlParams: { itemIndex: itemIndex },
                method: "POST",
                success: () => itemElem.classList.remove("processing")
            });
        });
        this.registerCommand("item-refresh", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            this.__refreshItem(itemElem);
        });

        this._renderBlocks();
    }
    protected eachItems(f: (elem: Element, index: number) => void) {
        for (let i = 0; i < this.element.children.length; i++) {
            let itemElem = this.element.children.item(i);
            if (!itemElem.hasAttribute("content-path-index"))
                continue;
            f(itemElem, i);
        }
    }
    protected countItems(): number {
        var i = 0;
        this.eachItems(() => i++);
        return i;
    }
    protected getItem(index: number): Element {
        var itemElem: Element;

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
    protected _renderBlock(itemElem: Element) { }
    protected _refreshBlockIndexes() {
        this.eachItems((elem, index) => { elem.setAttribute("content-path-index", index.toString()); });
    }
    private __refreshItem(elem: Element) {
        var urlParams = {};
        if (this.options.isListValue)
            urlParams["itemIndex"] = elem.getAttribute("content-path-index");

        this.request({
            url: '/brandup.pages/content/model/view',
            urlParams: urlParams,
            method: "GET",
            success: (data: string, status: number) => {
                if (status === 200) {
                    let fragment = document.createDocumentFragment();
                    let container = DOM.tag("div", null, data);
                    fragment.appendChild(container);

                    let newElem = DOM.queryElement(container, "[content-path]");
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
            let itemElem = this.element.children.item(i);
            if (itemElem.hasAttribute("content-path-index"))
                return true;
        }
        return false;
    }
    addItem(itemType: string, index: number) {
        this.request({
            url: '/brandup.pages/content/model',
            urlParams: {
                itemType: itemType,
                itemIndex: index.toString()
            },
            method: "PUT",
            success: (data: string, status: number) => {
                if (status === 200) {
                    this.request({
                        url: '/brandup.pages/content/model/view',
                        urlParams: { itemIndex: index.toString() },
                        method: "GET",
                        success: (data: string, status: number) => {
                            if (status === 200) {
                                let fragment = document.createDocumentFragment();
                                let container = DOM.tag("div", null, data);
                                fragment.appendChild(container);
                                let newElem = DOM.queryElement(container, "[content-path]");

                                if (this.options.isListValue) {
                                    if (index > 0)
                                        this.getItem(index - 1).insertAdjacentElement("afterend", newElem);
                                    else if (index == 0)
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