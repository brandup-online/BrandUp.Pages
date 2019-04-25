var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports", "./dialog-list", "brandup-ui", "./page-create"], function (require, exports, dialog_list_1, brandup_ui_1, page_create_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var PageListDialog = /** @class */ (function (_super) {
        __extends(PageListDialog, _super);
        function PageListDialog(collectionId, options) {
            var _this = _super.call(this, options) || this;
            _this.collectionId = collectionId;
            return _this;
        }
        Object.defineProperty(PageListDialog.prototype, "typeName", {
            get: function () { return "BrandUpPages.PageListDialog"; },
            enumerable: true,
            configurable: true
        });
        PageListDialog.prototype._onRenderContent = function () {
            var _this = this;
            _super.prototype._onRenderContent.call(this);
            this.setHeader("Страницы коллекции");
            this.setNotes("Просмотр и управление страницами.");
            this.registerCommand("item-create", function () {
                page_create_1.createPage(_this.collectionId).then(function () {
                    _this.loadItems();
                });
            });
            this.registerItemCommand("item-update", function () { });
            this.registerItemCommand("item-delete", function () { });
            this.loadItems();
        };
        PageListDialog.prototype._getItemsUrl = function () {
            return "/brandup.pages/page";
        };
        PageListDialog.prototype._onSetUrlParams = function (urlParams) {
            urlParams["collectionId"] = this.collectionId;
        };
        PageListDialog.prototype._getItemId = function (item) {
            return item.id;
        };
        PageListDialog.prototype._renderItemContent = function (item, contentElem) {
            contentElem.appendChild(brandup_ui_1.DOM.tag("div", { class: "title" }, item.title));
        };
        PageListDialog.prototype._renderItemMenu = function (item, menuElem) {
            menuElem.appendChild(brandup_ui_1.DOM.tag("li", null, [brandup_ui_1.DOM.tag("a", { href: "", "data-command": "item-update" }, "Edit")]));
            menuElem.appendChild(brandup_ui_1.DOM.tag("li", null, [brandup_ui_1.DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
        };
        PageListDialog.prototype._renderEmpty = function (container) {
            container.innerText = "Страниц не создано.";
        };
        PageListDialog.prototype._renderNewItem = function (containerElem) {
            containerElem.appendChild(brandup_ui_1.DOM.tag("a", { href: "", "data-command": "item-create" }, "Новая страница"));
        };
        return PageListDialog;
    }(dialog_list_1.ListDialog));
    exports.PageListDialog = PageListDialog;
    exports.listPage = function (pageId) {
        var dialog = new PageListDialog(pageId);
        return dialog.open();
    };
});
//# sourceMappingURL=page-list.js.map