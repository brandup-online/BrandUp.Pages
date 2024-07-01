import { ModelDesigner, ModelDesignerOptions } from "./model";
import { DOM } from "brandup-ui-dom";
import "./page-blocks.less";
import iconRefresh from "../../svg/new/update.svg";
import iconEddit from "../../svg/new/edit.svg";
import iconDelete from "../../svg/new/trash.svg";
import iconSort from "../../svg/new/sort.svg";
import iconSortDown from "../../svg/new/sort-down.svg";
import iconAdd from "../../svg/page-blocks-add.svg";
import { IPageDesigner } from "../../typings/content";
import { DesignerEvent } from "./base";

export class PageBlocksDesigner extends ModelDesigner {
    get typeName(): string { return "BrandUpPages.PageBlocksDesigner"; }

    constructor(page: IPageDesigner, elem: HTMLElement, options: ModelDesignerOptions) {
        super(page, elem, options);
        this.renderBlocks();
    }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("content-designer");
        elem.classList.add("page-blocks-designer");

        if (this.options.isListValue) {
            elem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "page-blocks-designer-new-item bp-elem" }, '<div><ol><li><a href="#" data-command="item-add" class="accent">Добавить блок</a></li></ol></div>'));
        }

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
            this._refreshBlockIndexes();
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
            this.renderBlocks();
            this._refreshBlockIndexes();

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
            this.renderBlocks();
            this._refreshBlockIndexes();

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
        return this.getItems().length;
    }
    protected getItemIndex(item: Element) {
        const index = +item.getAttribute("data-content-path-index");
        if (index >=0) return index;
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

    protected _refreshBlockIndexes() {
        this.eachItems((elem, index) => elem.setAttribute("data-content-path-index", index.toString()));
    }

    addItem (data: string, index: number) {
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
        return newElem;
    }

    renderBlocks() {
        this.eachItems((elem) => { this._renderBlock(elem as HTMLElement); });
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
        this._refreshBlockIndexes();
    }

    protected _renderBlock(blockElem: HTMLElement) {
        if (blockElem.classList.contains("page-blocks-designer-item")) {
            blockElem.classList.remove("page-blocks-designer-item");

            for (let i = 0; i < blockElem.children.length; i++) {
                let elem = blockElem.children.item(i);

                if (elem.classList.contains("page-blocks-designer-item-add")) {
                    elem.remove();
                    continue;
                }

                if (elem.classList.contains("page-blocks-designer-item-tools")) {
                    elem.remove();
                    continue;
                }
            }
        }

        var type = blockElem.dataset.contentType;;

        if (this.options.isListValue) {
            let index = this.getItemIndex(blockElem);
            type = '<i>#' + (index + 1) + '</i>' + type;
        }

        blockElem.classList.add("page-blocks-designer-item");

        blockElem.insertAdjacentElement("beforeend", DOM.tag("a", { class: "bp-elem page-blocks-designer-item-add", href: "#", "data-command": "item-add", title: this.options.addText ? this.options.addText : "Добавить" }, iconAdd));

        blockElem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "bp-elem page-blocks-designer-item-tools" }, '<ul class="pad">' +
            '   <li data-command="item-view" class="no-icon"><span><b>' + type + '</b></span></li>' +
            '</ul>'));

        blockElem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "bp-elem page-blocks-designer-item-tools page-blocks-designer-item-tools-right" }, '<ul class="pad">' +
            '   <li data-command="item-refresh" title="Обновить">' + iconRefresh+'</li>' +
            '   <li data-command="item-settings" title="Изменить параметры">' + iconEddit +'</li>' +
            '   <li data-command="item-delete" class="red" title="Удалить блок">' + iconDelete +'</li>' +
            '</ul>' +
            '<ul>' +
            '   <li data-command="item-up" title="Поднять блок вверх">' + iconSort +'</li>' +
            '   <li data-command="item-down" title="Опустить блок вниз">' + iconSortDown +'</li>' +
            '</ul>'));
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

    hasValue(): boolean {
        for (let i = 0; i < this.element.children.length; i++) {
            const itemElem = this.element.children.item(i);
            if (itemElem.hasAttribute("data-content-path"))
                return true;
        }
        return false;
    }

    destroy() {
        DOM.queryElements(this.element, "* > [data-content-path-index] .page-blocks-designer-item-add").forEach((elem) => { elem.remove(); });
        DOM.queryElements(this.element, "* > [data-content-path-index] .page-blocks-designer-item-tools").forEach((elem) => { elem.remove(); });
        DOM.queryElements(this.element, "* > .page-blocks-designer-new-item").forEach((elem) => { elem.remove(); });

        this.element.classList.remove("content-designer");
        super.destroy();
    }
}

type CallbackType = "add-item" | "item-settings" | "item-delete" | "item-refresh" | "item-up" | "item-down";