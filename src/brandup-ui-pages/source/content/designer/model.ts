import { DesignerEvent, FieldDesigner } from "./base";
import { DOM } from "brandup-ui-dom";
import { editPage } from "../../dialogs/pages/edit";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { ContentTypeModel } from "../../typings/models";
import "./model.less";
import { AjaxResponse } from "brandup-ui-ajax";
import { IPageDesigner } from "../../typings/content";

export class ModelDesigner extends FieldDesigner<ModelDesignerOptions> {
    get typeName(): string { return "BrandUpPages.ModelDesigner"; }

    constructor(page: IPageDesigner, elem: HTMLElement, options: ModelDesignerOptions) {
        super(page, elem, options);
        this.renderBlocks();
    }

    setCallback(name: CallbackType, handler: (e: DesignerEvent<any>) => void) {
        super.setCallback(name, handler);
    }
    protected triggerCallback(name: CallbackType, e: DesignerEvent<any>) {
        super.triggerCallback(name, e);
    }
    removeCallback(name:CallbackType) {
        super.removeCallback(name);
    }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("content-designer");

        this.registerCommand("item-add", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            this.triggerCallback("add-item", {value: this.getItemIndex(itemElem), target: elem});
        });
        this.registerCommand("item-view", () => { return; });
        this.registerCommand("item-settings", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            const contentPath = itemElem.getAttribute("data-content-path");
            const index = this.getItemIndex(itemElem);

            this.triggerCallback("item-settings", {value: { contentPath, index }, target: itemElem as HTMLElement});
        });
        this.registerCommand("item-delete", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");
            const index = this.getItemIndex(itemElem);
            itemElem.remove();
            // this._refreshBlockIndexes();
            this.renderBlocks();
            this.triggerCallback("item-delete", {value: { index, itemElem }, target: elem});
        });
        this.registerCommand("item-up", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            const itemIndex = this.getItemIndex(itemElem);
            if (itemIndex <= 0)
                return;

            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            itemElem.previousElementSibling.insertAdjacentElement("beforebegin", itemElem);
            this._refreshBlockIndexes();
            this.renderBlocks();

            this.triggerCallback("item-up", { target: itemElem as HTMLElement, value: itemIndex});
        });
        this.registerCommand("item-down", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            const itemIndex = this.getItemIndex(itemElem);

            if (itemIndex + 1 >= this.countItems())
                return;

            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            itemElem.nextElementSibling.insertAdjacentElement("afterend", itemElem);
            this._refreshBlockIndexes();
            this.renderBlocks();

            this.triggerCallback("item-down", { target: itemElem as HTMLElement, value: itemIndex});
        });
        this.registerCommand("item-refresh", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            this.triggerCallback("item-refresh", {target: itemElem as HTMLElement, value: this.getItemIndex(itemElem)});
        });
    }
    protected eachItems(f: (elem: Element, index: number) => void) {
        const elements = this.getItems();
        for ( let i=0; i < elements.length; i++ )
            f(elements.item(i), i);
    }
    protected countItems(): number {
        let i = 0;
        this.eachItems(() => i++);
        return i;
    }
    protected getItemIndex(item: Element) {
        const items = this.getItems();
        for (let i = 0; i < items.length; i++) {
            if (items.item(i) === item) return i;
        }
        return -1;
    }
    protected getItems () {
        return DOM.queryElements(this.element, `.page-blocks-designer > [data-content-path]`);
    }
    protected getItem(index: number): Element {
        let itemElem: Element;

        for (let i = 0; i < this.element.children.length; i++) {
            itemElem = this.element.children.item(i);
            if (itemElem.hasAttribute("data-content-path") && i === index)
                return itemElem;
        }

        return itemElem;
    }
    addItem (data, index) {
        const fragment = document.createDocumentFragment();
        const container = DOM.tag("div", null, data);
        fragment.appendChild(container);
        const newElem = DOM.queryElement(container, "[data-content-path]");

        if (this.options.isListValue) {
            if (index > 0)
                this.getItem(index - 1).insertAdjacentElement("afterend", newElem);
            else if (index === 0)
                this.element.insertAdjacentElement("afterbegin", newElem);
            else
                this.getItem(-1).insertAdjacentElement("afterend", newElem);
        }

        this.page.redraw();

        this._renderBlock(newElem);

        this._refreshBlockIndexes();
    }

    renderBlocks() {
        this.eachItems((elem) => { this._renderBlock(elem); });
    }
    protected _renderBlock(itemElem: Element) { }
    protected _refreshBlockIndexes() {
        this.eachItems((elem, index) => { elem.setAttribute("data-content-path-index", index.toString()); });
    }

    refreshItem(elem: Element, content: any) {
        const fragment = document.createDocumentFragment();
        const container = DOM.tag("div", null, content);
        fragment.appendChild(container);

        const newElem = DOM.queryElement(container, "[data-content-path]");
        elem.insertAdjacentElement("afterend", newElem);
        elem.remove();

        this._renderBlock(newElem);
        this.page.redraw();
    }

    hasValue(): boolean {
        for (let i = 0; i < this.element.children.length; i++) {
            const itemElem = this.element.children.item(i);
            if (itemElem.hasAttribute("data-content-path"))
                return true;
        }
        return false;
    }

    destroy() {
        this.element.classList.remove("content-designer");

        super.destroy();
    }
}

type CallbackType = "add-item" | "item-settings" | "item-delete" | "item-refresh" | "item-up" | "item-down";
export interface ModelDesignerOptions {
    addText: string;
    isListValue: boolean;
    itemType: ContentTypeModel;
    itemTypes: Array<ContentTypeModel>;
}