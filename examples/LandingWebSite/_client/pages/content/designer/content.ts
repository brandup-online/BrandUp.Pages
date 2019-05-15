import { FieldDesigner } from "./base";
import { DOM } from "brandup-ui";
import "./content.less";
import { editPage } from "../../dialogs/page-edit";
import { Dialog, DialogOptions } from "../../dialogs/dialog";

export class ContentDesigner extends FieldDesigner<ContentDesignerOptions> {
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
                    itemIndex = DOM.queryElements(this.element, "* > [content-path-index]").length;
            }

            if (!itemType) {
                new SelectItemTypeDialog(this.options.itemTypes).open().then((type) => {
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
            });
        });
        this.registerCommand("item-delete", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");

            itemElem.remove();
            this._refreshBlockIndexes();

            this.request({
                url: '/brandup.pages/content/content',
                urlParams: {
                    itemIndex: itemIndex
                },
                method: "DELETE",
                success: (data: string, status: number) => {
                    if (status === 200) {
                    }
                }
            });
        });
        this.registerCommand("item-up", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");
            if (parseInt(itemIndex) <= 0)
                return;

            itemElem.previousElementSibling.insertAdjacentElement("beforebegin", itemElem);
            this._refreshBlockIndexes();

            this.request({
                url: '/brandup.pages/content/content/up',
                urlParams: {
                    itemIndex: itemIndex
                },
                method: "POST",
                success: (data: string, status: number) => {
                    if (status === 200) {
                    }
                }
            });
        });
        this.registerCommand("item-down", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");
            
            if (parseInt(itemIndex) >= DOM.queryElements(this.element, "* > [content-path-index]").length - 1)
                return;

            itemElem.nextElementSibling.insertAdjacentElement("afterend", itemElem);
            this._refreshBlockIndexes();

            this.request({
                url: '/brandup.pages/content/content/down',
                urlParams: {
                    itemIndex: itemIndex
                },
                method: "POST",
                success: (data: string, status: number) => {
                    if (status === 200) {
                    }
                }
            });
        });
        this.registerCommand("item-refresh", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path]");

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
            url: '/brandup.pages/content/content/view',
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
                }
            }
        });
    }

    hasValue(): boolean {
        return DOM.queryElements(this.element, "* > [content-path-index]").length > 0;
    }
    addItem(itemType: string, index: number) {
        this.request({
            url: '/brandup.pages/content/content',
            urlParams: {
                itemType: itemType,
                itemIndex: index.toString()
            },
            method: "PUT",
            success: (data: string, status: number) => {
                if (status === 200) {
                    this.request({
                        url: '/brandup.pages/content/content/view',
                        urlParams: { itemIndex: index.toString() },
                        method: "GET",
                        success: (data: string, status: number) => {
                            if (status === 200) {
                                let fragment = document.createDocumentFragment();
                                let container = DOM.tag("div", null, data);
                                fragment.appendChild(container);
                                let newElem = DOM.queryElement(container, "[content-path]");

                                if (this.options.isListValue) {
                                    let items = DOM.queryElements(this.element, "* > [content-path-index]");

                                    if (index > 0)
                                        items.item(index - 1).insertAdjacentElement("afterend", newElem);
                                    else if (index == 0)
                                        this.element.insertAdjacentElement("afterbegin", newElem);
                                    else
                                        items.item(items.length - 1).insertAdjacentElement("afterend", newElem);
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

export interface ContentDesignerOptions {
    isListValue: boolean;
    itemTypes: Array<ContentItemType>;
}

export interface ContentItemType {
    name: string;
    title: string;
}

class SelectItemTypeDialog extends Dialog<ContentItemType> {
    private __types: Array<ContentItemType>;

    constructor(types: Array<ContentItemType>, options?: DialogOptions) {
        super(options);

        this.__types = types;
    }

    get typeName(): string { return "BrandUpPages.SelectItemTypeDialog"; }

    protected _onRenderContent() {
        this.element.classList.add("website-dialog-select");

        this.setHeader("Выберите тип элемента");

        this.__types.map((type, index) => {
            let itemElem = DOM.tag("a", { class: "item", href: "", "data-command": "select", "data-index": index }, type.title);
            this.content.appendChild(itemElem);
        });

        this.registerCommand("select", (elem: HTMLElement) => {
            let index = parseInt(elem.getAttribute("data-index"));
            let type = this.__types[index];

            this.resolve(type);
        });
    }
}