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
import { ListDialog } from "../dialog-list";
import { DOM } from "brandup-ui";
var ContentTypeListDialog = /** @class */ (function (_super) {
    __extends(ContentTypeListDialog, _super);
    function ContentTypeListDialog(baseContentType, options) {
        var _this = _super.call(this, options) || this;
        _this.__isModified = false;
        _this.baseContentType = baseContentType ? baseContentType : null;
        return _this;
    }
    Object.defineProperty(ContentTypeListDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageCollectionListDialog"; },
        enumerable: false,
        configurable: true
    });
    ContentTypeListDialog.prototype._onRenderContent = function () {
        var _this = this;
        _super.prototype._onRenderContent.call(this);
        this.setHeader("Типы контента");
        this.setNotes("Просмотр и настройка типов контента страниц.");
        this.registerItemCommand("nav", function (id) {
            _this.baseContentType = id;
            _this.refresh();
        });
        this.registerCommand("nav2", function (elem) {
            var name = elem.getAttribute("data-value");
            _this.baseContentType = name ? name : null;
            _this.refresh();
        });
    };
    ContentTypeListDialog.prototype._onClose = function () {
        if (this.__isModified)
            this.resolve(null);
        else
            _super.prototype._onClose.call(this);
    };
    ContentTypeListDialog.prototype._buildUrl = function () {
        return "/brandup.pages/content-type/list";
    };
    ContentTypeListDialog.prototype._buildUrlParams = function (urlParams) {
        if (this.baseContentType)
            urlParams["baseType"] = this.baseContentType;
    };
    ContentTypeListDialog.prototype._buildList = function (model) {
        if (!this.navElem) {
            this.navElem = DOM.tag("ol", { class: "nav" });
            this.content.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }
        this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav2", "data-value": "" }, "root")));
        if (model.parents && model.parents.length) {
            for (var i = 0; i < model.parents.length; i++) {
                this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav2", "data-value": model.parents[i] }, model.parents[i])));
            }
        }
    };
    ContentTypeListDialog.prototype._getItemId = function (item) {
        return item.name;
    };
    ContentTypeListDialog.prototype._renderItemContent = function (item, contentElem) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("a", { href: "", "data-command": "nav" }, item.title)));
    };
    ContentTypeListDialog.prototype._renderItemMenu = function (item, menuElem) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-templates" }, "Шаблоны данных")]));
    };
    ContentTypeListDialog.prototype._renderEmpty = function (container) {
        container.innerText = "Типов контента не найдено";
    };
    ContentTypeListDialog.prototype._renderNewItem = function (containerElem) {
    };
    return ContentTypeListDialog;
}(ListDialog));
export { ContentTypeListDialog };
export var listContentType = function (baseContentType) {
    if (baseContentType === void 0) { baseContentType = null; }
    var dialog = new ContentTypeListDialog(baseContentType);
    return dialog.open();
};
