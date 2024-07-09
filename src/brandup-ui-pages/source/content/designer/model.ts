import { FieldDesigner } from "./base";
import { ModelFieldProvider } from "../provider/model";
import { DOM } from "brandup-ui-dom";
import "./model.less";

export class ModelDesigner extends FieldDesigner<ModelFieldProvider> {
    get typeName(): string { return "BrandUpPages.ModelDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("content-designer");

        this.registerCommand("item-add", (elem: HTMLElement) => {
            const position = elem.dataset.position ?? "last";

            let itemIndex: number = -1;
            if (this.provider.options.isListValue) {
                if (position == "after") {
                    const contentElem = <HTMLElement>elem.closest("[data-content-path]");
                    const content = this.provider.content.host.editor.navigate(contentElem.dataset.contentPath);
                    if (!content)
                        throw "";
                    itemIndex = content.index + 1;
                }
            }

            this.provider.addItem(itemIndex);
        });

        this.registerCommand("item-view", () => { return; });

        this.registerCommand("item-settings", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path]");
            const contentPath = itemElem.getAttribute("data-content-path");

            this.provider.settingItem(contentPath);
        });

        this.registerCommand("item-delete", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path-index]");
            if (itemElem.classList.contains("processing"))
                return;
            const path = itemElem.getAttribute("data-content-path");
            itemElem.classList.add("processing");
            const index = this.getItemIndex(itemElem);
            
            this.provider.deleteItem(index, path);
        });

        this.registerCommand("item-up", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path-index]");
            const itemIndex = this.getItemIndex(itemElem);
            if (itemIndex <= 0)
                return;

            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            itemElem.previousElementSibling.insertAdjacentElement("beforebegin", itemElem);

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
            this.provider.itemDown(itemIndex, itemElem);
        });

        this.registerCommand("item-refresh", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path-index]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");

            this.provider.refreshItem(itemElem, this.getItemIndex(itemElem));
        });
    }
    
    refreshItem(elem: Element, content: any) {
        const fragment = document.createDocumentFragment();
        const container = DOM.tag("div", null, content);
        fragment.appendChild(container);

        const newElem = DOM.queryElement(container, "[data-content-path]");
        elem.insertAdjacentElement("afterend", newElem);
        elem.remove();

        this.renderBlock(newElem);
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

    protected countItems(): number {
        return this.getItems().length;
    }

    protected getItemIndex(item: Element) {
        const index = +item?.getAttribute("data-content-path-index");
        if (index >=0) return index;
        return -1;
    }

    renderBlock(itemElem: Element) { }

    renderBlocks() {
        this.eachItems((elem) => { this.renderBlock(elem as HTMLElement); });
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