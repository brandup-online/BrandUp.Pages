import { ModelDesigner } from "./model";
import { DOM } from "brandup-ui-dom";
import "./page-blocks.less";
import iconRefresh from "../../svg/new/update.svg";
import iconEddit from "../../svg/new/edit.svg";
import iconDelete from "../../svg/new/trash.svg";
import iconSort from "../../svg/new/sort.svg";
import iconSortDown from "../../svg/new/sort-down.svg";
import iconAdd from "../../svg/page-blocks-add.svg";

export class PageBlocksDesigner extends ModelDesigner {
    get typeName(): string { return "BrandUpPages.PageBlocksDesigner"; }

    protected onRender(elem: HTMLElement) {
        super.onRender(elem);

        elem.classList.add("page-blocks-designer");

        if (this.options.isListValue) {
            elem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "page-blocks-designer-new-item bp-elem" }, '<div><ol><li><a href="#" data-command="item-add" class="accent">Добавить блок</a></li></ol></div>'));
        }
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

        super._renderBlock(blockElem);

        var type = blockElem.getAttribute("content-type");

        if (this.options.isListValue) {
            let index = parseInt(blockElem.getAttribute("data-content-path-index"));
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

    destroy() {
        DOM.queryElements(this.element, "* > [data-content-path-index] .page-blocks-designer-item-add").forEach((elem) => { elem.remove(); });
        DOM.queryElements(this.element, "* > [data-content-path-index] .page-blocks-designer-item-tools").forEach((elem) => { elem.remove(); });
        DOM.queryElements(this.element, "* > .page-blocks-designer-new-item").forEach((elem) => { elem.remove(); });

        super.destroy();
    }
}