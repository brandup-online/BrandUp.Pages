﻿import { FieldDesigner } from "./field";
import { DOM } from "brandup-ui";
import "./content.less";
import { editPage } from "../dialogs/page-edit";
import { Dialog, DialogOptions } from "../dialogs/dialog";

export class ContentDesigner extends FieldDesigner<ContentDesignerOptions> {
    get typeName(): string { return "BrandUpPages.ListDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("list-designer");
        
        elem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "list-designer-new-item brandup-pages-elem" }, '<div><ol>' +
            '   <li><a href="#" data-command="item-add" class="accent">Все блоки</a></li>' +
            '   <li>' +
            '       <ul>' +
            '           <li><a href="#" data-command="item-add" data-item-type="Content.Text">Текст</a></li>' +
            '           <li class="split"></li>' +
            '           <li><a href="#" data-command="item-add" data-item-type="Content.Image">Изображение</a></li>' +
            '           <li class="split"></li>' +
            '           <li><a href="#" data-command="item-add" data-item-type="GTR.ImagesBlock">Галерея</a></li>' +
            '       </ul>' +
            '   </li>' +
            '   <li><a href="#" data-command="item-add">Автоподбор</a></li>' +
            '</ol></div>'));

        var items = DOM.queryElements(elem, "* > [content-path-index]");
        items.forEach((elem) => {
            this.__renderItemUI(elem);
        });

        this.registerCommand("item-add", (elem: HTMLElement) => {
            let itemType = elem.getAttribute("data-item-type");
            if (!itemType) {
                new SelectItemTypeDialog(this.options.itemsTypes).open().then((type) => {
                    this.addItem(type.name);
                });
            }
            else
                this.addItem(itemType);
        });
        this.registerCommand("item-view", () => { });
        this.registerCommand("item-settings", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let contentPath = itemElem.getAttribute("content-path");

            editPage(this.page.editId, contentPath).then(() => {

            });
        });
        this.registerCommand("item-delete", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");

            itemElem.remove();
            this.__refreshItemIndexes();

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
            this.__refreshItemIndexes();

            this.request({
                url: '/brandup.pages/content/content/up',
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
        this.registerCommand("item-down", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");
            
            if (parseInt(itemIndex) >= DOM.queryElements(this.element, "* > [content-path-index]").length - 1)
                return;

            itemElem.nextElementSibling.insertAdjacentElement("afterend", itemElem);
            this.__refreshItemIndexes();

            this.request({
                url: '/brandup.pages/content/content/down',
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
    }

    private __renderItemUI(elem: HTMLElement) {
        elem.classList.add("list-designer-item");
        
        elem.insertAdjacentElement("beforeend", DOM.tag("a", { class: "list-designer-item-add", href: "#", "data-command": "item-add" }));
        
        elem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "list-designer-item-tools" }, '<ul class="pad">' +
            '   <li data-command="item-view" title="Выбер макета" class="no-icon"><span><b>Макет</b></span></li>' +
            '</ul>'));
        
        elem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "list-designer-item-tools list-designer-item-tools-right" }, '<ul class="pad">' +
            '   <li data-command="item-settings" title="Изменить параметры"><svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 26 26"><path d="M 11.46875 0.96875 L 10.90625 4.53125 C 10.050781 4.742188 9.234375 5.058594 8.5 5.5 L 5.5625 3.40625 L 3.4375 5.53125 L 5.5 8.46875 C 5.054688 9.207031 4.714844 10.015625 4.5 10.875 L 0.96875 11.46875 L 0.96875 14.46875 L 4.5 15.09375 C 4.714844 15.953125 5.054688 16.761719 5.5 17.5 L 3.40625 20.4375 L 5.53125 22.5625 L 8.46875 20.5 C 9.203125 20.941406 10.019531 21.257813 10.875 21.46875 L 11.46875 25.03125 L 14.46875 25.03125 L 15.125 21.46875 C 15.976563 21.253906 16.769531 20.914063 17.5 20.46875 L 20.46875 22.5625 L 22.59375 20.4375 L 20.46875 17.5 C 20.90625 16.769531 21.257813 15.972656 21.46875 15.125 L 25.03125 14.46875 L 25.03125 11.46875 L 21.46875 10.875 C 21.257813 10.027344 20.90625 9.230469 20.46875 8.5 L 22.5625 5.53125 L 20.4375 3.40625 L 17.5 5.53125 C 16.769531 5.089844 15.949219 4.746094 15.09375 4.53125 L 14.46875 0.96875 Z M 13 6.46875 C 16.605469 6.46875 19.53125 9.394531 19.53125 13 C 19.53125 16.605469 16.605469 19.53125 13 19.53125 C 9.394531 19.53125 6.46875 16.601563 6.46875 13 C 6.46875 9.398438 9.394531 6.46875 13 6.46875 Z M 13 8.0625 C 10.28125 8.0625 8.0625 10.28125 8.0625 13 C 8.0625 15.71875 10.28125 17.9375 13 17.9375 C 15.71875 17.9375 17.9375 15.71875 17.9375 13 C 17.9375 10.28125 15.71875 8.0625 13 8.0625 Z M 12.96875 10.9375 C 14.113281 10.9375 15.0625 11.851563 15.0625 13 C 15.0625 14.144531 14.113281 15.0625 12.96875 15.0625 C 11.824219 15.0625 10.90625 14.144531 10.90625 13 C 10.90625 11.851563 11.824219 10.9375 12.96875 10.9375 Z "></path></svg></li>' +
            '   <li data-command="item-delete" title="Удалить блок"><svg xmlns="http://www.w3.org/2000/svg" width="26" height="26" viewBox="0 0 26 26"><path d="M 11.5 -0.03125 C 9.5416406 -0.03125 7.96875 1.5955183 7.96875 3.5625 L 7.96875 4 L 4 4 C 3.449 4 3 4.449 3 5 L 3 6 L 2 6 L 2 8 L 4 8 L 4 23 C 4 24.645063 5.3549372 26 7 26 L 19 26 C 20.645063 26 22 24.645063 22 23 L 22 8 L 24 8 L 24 6 L 23 6 L 23 5 C 23 4.449 22.551 4 22 4 L 18.03125 4 L 18.03125 3.5625 C 18.03125 1.5955183 16.458359 -0.03125 14.5 -0.03125 L 11.5 -0.03125 z M 11.5 2.03125 L 14.5 2.03125 C 15.303641 2.03125 15.96875 2.6874817 15.96875 3.5625 L 15.96875 4 L 10.03125 4 L 10.03125 3.5625 C 10.03125 2.6874817 10.696359 2.03125 11.5 2.03125 z M 6 8 L 11.125 8 C 11.249098 8.0134778 11.372288 8.03125 11.5 8.03125 L 14.5 8.03125 C 14.627712 8.03125 14.750902 8.0134778 14.875 8 L 20 8 L 20 23 C 20 23.562937 19.562937 24 19 24 L 7 24 C 6.4370628 24 6 23.562937 6 23 L 6 8 z M 8 10 L 8 22 L 10 22 L 10 10 L 8 10 z M 12 10 L 12 22 L 14 22 L 14 10 L 12 10 z M 16 10 L 16 22 L 18 22 L 18 10 L 16 10 z"></path></svg></li>' +
            '</ul>' +
            '<ul>' +
            '   <li data-command="item-up" title="Поднять блок вверх"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 26 26" version="1.1"><g id="surface1"><path style=" " d="M 5 11.300781 C 7.800781 7.101563 12.199219 3.300781 12.398438 3.199219 C 12.5 3.101563 12.800781 3 13 3 C 13.199219 3 13.5 3.101563 13.699219 3.199219 C 13.898438 3.300781 18.300781 7.199219 21.101563 11.300781 C 21.300781 11.601563 21.300781 12 21.199219 12.398438 C 21 12.699219 20.699219 13 20.300781 13 L 16.601563 13 C 16.601563 13 16.101563 21.699219 15.800781 22.101563 C 15.300781 22.601563 14.101563 23 13 23 C 11.898438 23 10.800781 22.601563 10.398438 22.101563 C 10.199219 21.699219 9.5 13 9.5 13 L 5.800781 13 C 5.398438 13 5.101563 12.800781 4.898438 12.398438 C 4.699219 12.101563 4.800781 11.699219 5 11.300781 Z "></path></g></svg></li>' +
            '   <li data-command="item-down" title="Опустить блок вниз"><svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 0 26 26" version="1.1"><g id="surface1"><path style=" " d="M 21 14.699219 C 18.199219 18.898438 13.800781 22.699219 13.601563 22.800781 C 13.5 22.898438 13.199219 23 13 23 C 12.800781 23 12.5 22.898438 12.300781 22.800781 C 12.101563 22.699219 7.699219 18.800781 4.898438 14.699219 C 4.699219 14.398438 4.699219 14 4.800781 13.601563 C 5.101563 13.199219 5.398438 13 5.800781 13 L 9.5 13 C 9.5 13 10 4.300781 10.300781 3.898438 C 10.699219 3.398438 11.898438 3 13 3 C 14.101563 3 15.199219 3.398438 15.601563 3.898438 C 15.898438 4.300781 16.5 13 16.5 13 L 20.199219 13 C 20.601563 13 20.898438 13.199219 21.101563 13.601563 C 21.300781 13.898438 21.199219 14.300781 21 14.699219 Z "></path></g></svg></li>' +
            '</ul>'));
    }
    private __refreshItemIndexes() {
        var items = DOM.queryElements(this.element, "* > [content-path-index]");
        items.forEach((elem, index) => {
            elem.setAttribute("content-path-index", index.toString());
        });
    }

    hasValue(): boolean {
        return DOM.queryElements(this.element, "* > [content-path-index]").length > 0;
    }
    addItem(itemType: string) {
        this.request({
            url: '/brandup.pages/content/list',
            urlParams: {
                itemType: itemType
            },
            method: "PUT",
            success: (data: string, status: number) => {
                if (status === 200) {
                }
            }
        });
    }

    destroy() {
        DOM.queryElements(this.element, "* > [content-path-index] .list-designer-item-add").forEach((elem) => { elem.remove(); });
        DOM.queryElements(this.element, "* > [content-path-index] .list-designer-item-tools").forEach((elem) => { elem.remove(); });
        DOM.queryElements(this.element, "* > .list-designer-new-item").forEach((elem) => { elem.remove(); });
        
        super.destroy();
    }
}

export interface ContentDesignerOptions {
    isListValue: boolean;
    itemsTypes: Array<ContentItemType>;
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
        this.setHeader("Выберите тип элемента");

        //this.__types.map((type) => {

        //});
    }
}