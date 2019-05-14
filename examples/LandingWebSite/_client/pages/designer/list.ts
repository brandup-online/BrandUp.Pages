import { FieldDesigner } from "./field";
import { DOM } from "brandup-ui";
import "./list.less";

export class ListDesigner extends FieldDesigner<ListDesignerOptions> {
    private __addBlockPanel: HTMLElement = null;

    get typeName(): string { return "BrandUpPages.ListDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("list-designer");

        this.__addBlockPanel = DOM.tag("div", { class: "list-designer-new-item brandup-pages-elem" }, '<div><ol>' +
            '   <li><a href="#" data-command="add-item" class="accent">Все блоки</a></li>' +
            '   <li>' +
            '       <ul>' +
            '           <li><a href="#" data-command="add-item" data-item-type="Content.Text">Текст</a></li>' +
            '           <li class="split"></li>' +
            '           <li><a href="#" data-command="add-item" data-item-type="Content.Image">Изображение</a></li>' +
            '           <li class="split"></li>' +
            '           <li><a href="#" data-command="add-item" data-item-type="GTR.ImagesBlock">Галерея</a></li>' +
            '       </ul>' +
            '   </li>' +
            '   <li><a href="#" data-command="add-item">Автоподбор</a></li>' +
            '</ol></div>');
        this.element.insertAdjacentElement("beforeend", this.__addBlockPanel);

        this.registerCommand("add-item", () => {

        });
    }

    hasValue(): boolean {
        return DOM.queryElements(this.element, "* > [content-path-index]").length > 0;
    }
}

export interface ListDesignerOptions {

}