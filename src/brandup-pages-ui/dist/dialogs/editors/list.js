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
import { assignPageEditor } from "./assign";
import { deletePageEditor } from "./delete";
var PageEditorListDialog = /** @class */ (function (_super) {
    __extends(PageEditorListDialog, _super);
    function PageEditorListDialog(pageId, options) {
        var _this = _super.call(this, options) || this;
        _this.__isModified = false;
        _this.pageId = pageId;
        return _this;
    }
    Object.defineProperty(PageEditorListDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageEditorListDialog"; },
        enumerable: false,
        configurable: true
    });
    PageEditorListDialog.prototype._onRenderContent = function () {
        var _this = this;
        _super.prototype._onRenderContent.call(this);
        this.setHeader("Редакторы страниц");
        this.setNotes("Просмотр и управление редакторами страниц.");
        this.registerCommand("item-create", function () {
            assignPageEditor().then(function () {
                _this.loadItems();
            });
        });
        this.registerItemCommand("item-delete", function (id) {
            deletePageEditor(id).then(function () {
                _this.loadItems();
            });
        });
    };
    PageEditorListDialog.prototype._onClose = function () {
        if (this.__isModified)
            this.resolve(null);
        else
            _super.prototype._onClose.call(this);
    };
    PageEditorListDialog.prototype._buildUrl = function () {
        return "/brandup.pages/editor/list";
    };
    PageEditorListDialog.prototype._buildUrlParams = function (urlParams) {
    };
    PageEditorListDialog.prototype._buildList = function (model) {
    };
    PageEditorListDialog.prototype._getItemId = function (item) {
        return item.id;
    };
    PageEditorListDialog.prototype._renderItemContent = function (item, contentElem) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("span", {}, item.email)));
    };
    PageEditorListDialog.prototype._renderItemMenu = function (item, menuElem) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
    };
    PageEditorListDialog.prototype._renderEmpty = function (container) {
        container.innerText = "Редакторов не назначено.";
    };
    PageEditorListDialog.prototype._renderNewItem = function (containerElem) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Назначить редактора"));
    };
    return PageEditorListDialog;
}(ListDialog));
export { PageEditorListDialog };
export var listPageEditor = function () {
    var dialog = new PageEditorListDialog(null);
    return dialog.open();
};
