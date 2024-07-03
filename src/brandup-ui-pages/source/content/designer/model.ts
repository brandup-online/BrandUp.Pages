import { FieldDesigner } from "./base";
import { ModelFieldProvider } from "../provider/model";
import { DOM } from "brandup-ui-dom";
import "./model.less";

export class ModelDesigner extends FieldDesigner<ModelFieldProvider> {
    get typeName(): string { return "BrandUpPages.ModelDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("content-designer");
        this._refreshBlockIndexes();

        this.registerCommand("item-add", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            const itemType = itemElem.getAttribute("data-item-type");
            const itemIndex = this.getItemIndex(itemElem) + 1;
            this.provider.addItem(itemType, itemIndex);
        });

        this.registerCommand("item-view", () => { return; });

        this.registerCommand("item-settings", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            const contentPath = itemElem.getAttribute("data-content-path");
            const index = this.getItemIndex(itemElem);

            this.provider.settingItem(contentPath);
        });

        this.registerCommand("item-delete", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            if (itemElem.classList.contains("processing"))
                return;
            const path = itemElem.getAttribute("data-content-path");
            itemElem.classList.add("processing");
            const index = this.getItemIndex(itemElem);
            
            this.provider.deleteItem(index, path);
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
            this.renderBlocks();
            this._refreshBlockIndexes();

            this.provider.itemUp(itemIndex, itemElem);
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
            this.renderBlocks();
            this._refreshBlockIndexes();

            this.provider.itemDown(itemIndex, itemElem);
        });

        this.registerCommand("item-refresh", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            this.provider.refreshItem(itemElem, this.getItemIndex(itemElem));
        });
    }

    deleteItem(index: number) {
        const itemElem = this.getItem(index);
        itemElem.remove();
        this._refreshBlockIndexes();
        this.renderBlocks();
    }

    hasValue(): boolean {
        for (let i = 0; i < this.element.children.length; i++) {
            const itemElem = this.element.children.item(i);
            if (itemElem.hasAttribute("data-content-path"))
                return true;
        }
        return false;
    }

    refreshItem(elem: Element, content: any) {
        const fragment = document.createDocumentFragment();
        const container = DOM.tag("div", null, content);
        fragment.appendChild(container);

        const newElem = DOM.queryElement(container, "[data-content-path]");
        elem.insertAdjacentElement("afterend", newElem);
        elem.remove();

        this._renderBlock(newElem);
        this._refreshBlockIndexes();
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

    addItem (data: string, index: number) {
        const fragment = document.createDocumentFragment();
        const container = DOM.tag("div", null, data);
        fragment.appendChild(container);
        const newElem = DOM.queryElement(container, "[data-content-path]");

        if (this.provider.options.isListValue) {
            if (index > 0)
                this.getItem(index - 1).insertAdjacentElement("afterend", newElem);
            else if (index === 0)
                this.element.insertAdjacentElement("afterbegin", newElem);
            else
                this.getItem(-1).insertAdjacentElement("afterend", newElem);
        }
        return newElem;
    }
    
    protected _refreshBlockIndexes() {
        this.eachItems((elem, index) => elem.setAttribute("data-content-path-index", index.toString()));
    }

    protected countItems(): number {
        return this.getItems().length;
    }

    protected getItemIndex(item: Element) {
        const index = +item.getAttribute("data-content-path-index");
        if (index >=0) return index;
        return -1;
    }

    protected _renderBlock(itemElem: Element) { }

    renderBlocks() {
        this.eachItems((elem) => { this._renderBlock(elem as HTMLElement); });
    }

    protected eachItems(f: (elem: Element, index: number) => void) {
        const elements = this.getItems();
        for ( let i=0; i < elements.length; i++ )
            f(elements.item(i), i);
    }

    protected getItems () {
        return DOM.queryElements(this.element, `.page-blocks-designer > [data-content-path]`);
    }
}