var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        if (typeof b !== "function" && b !== null)
            throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
import { ModelDesigner } from "./model";
import { DOM } from "brandup-ui";
import "./page-blocks.less";
import iconRefresh from "../../svg/page-blocks-refresh.svg";
import iconSettings from "../../svg/page-blocks-settings.svg";
import iconDelete from "../../svg/page-blocks-delete.svg";
import iconUp from "../../svg/page-blocks-up.svg";
import iconDown from "../../svg/page-blocks-down.svg";
import iconAdd from "../../svg/page-blocks-add.svg";
var PageBlocksDesigner = /** @class */ (function (_super) {
    __extends(PageBlocksDesigner, _super);
    function PageBlocksDesigner() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Object.defineProperty(PageBlocksDesigner.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageBlocksDesigner"; },
        enumerable: false,
        configurable: true
    });
    PageBlocksDesigner.prototype.onRender = function (elem) {
        _super.prototype.onRender.call(this, elem);
        elem.classList.add("page-blocks-designer");
        if (this.options.isListValue) {
            elem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "page-blocks-designer-new-item bp-elem" }, '<div><ol><li><a href="#" data-command="item-add" class="accent">Добавить блок</a></li></ol></div>'));
        }
    };
    PageBlocksDesigner.prototype._renderBlock = function (blockElem) {
        if (blockElem.classList.contains("page-blocks-designer-item")) {
            blockElem.classList.remove("page-blocks-designer-item");
            for (var i = 0; i < blockElem.children.length; i++) {
                var elem = blockElem.children.item(i);
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
        _super.prototype._renderBlock.call(this, blockElem);
        var type = blockElem.getAttribute("content-type");
        if (this.options.isListValue) {
            var index = parseInt(blockElem.getAttribute("content-path-index"));
            type = '<i>#' + (index + 1) + '</i>' + type;
        }
        blockElem.classList.add("page-blocks-designer-item");
        blockElem.insertAdjacentElement("beforeend", DOM.tag("a", { class: "bp-elem page-blocks-designer-item-add", href: "#", "data-command": "item-add", title: this.options.addText ? this.options.addText : "Добавить" }, iconAdd));
        blockElem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "bp-elem page-blocks-designer-item-tools" }, '<ul class="pad">' +
            '   <li data-command="item-view" class="no-icon"><span><b>' + type + '</b></span></li>' +
            '</ul>'));
        blockElem.insertAdjacentElement("beforeend", DOM.tag("div", { class: "bp-elem page-blocks-designer-item-tools page-blocks-designer-item-tools-right" }, '<ul class="pad">' +
            '   <li data-command="item-refresh" title="Обновить">' + iconRefresh + '</li>' +
            '   <li data-command="item-settings" title="Изменить параметры">' + iconSettings + '</li>' +
            '   <li data-command="item-delete" title="Удалить блок">' + iconDelete + '</li>' +
            '</ul>' +
            '<ul>' +
            '   <li data-command="item-up" title="Поднять блок вверх">' + iconUp + '</li>' +
            '   <li data-command="item-down" title="Опустить блок вниз">' + iconDown + '</li>' +
            '</ul>'));
    };
    PageBlocksDesigner.prototype.destroy = function () {
        DOM.queryElements(this.element, "* > [content-path-index] .page-blocks-designer-item-add").forEach(function (elem) { elem.remove(); });
        DOM.queryElements(this.element, "* > [content-path-index] .page-blocks-designer-item-tools").forEach(function (elem) { elem.remove(); });
        DOM.queryElements(this.element, "* > .page-blocks-designer-new-item").forEach(function (elem) { elem.remove(); });
        _super.prototype.destroy.call(this);
    };
    return PageBlocksDesigner;
}(ModelDesigner));
export { PageBlocksDesigner };
