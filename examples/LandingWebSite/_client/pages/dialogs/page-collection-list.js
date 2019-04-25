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
define(["require", "exports", "brandup-ui", "./page-collection-create", "./page-collection-delete", "./page-collection-update", "./dialog-list", "./page-list"], function (require, exports, brandup_ui_1, page_collection_create_1, page_collection_delete_1, page_collection_update_1, dialog_list_1, page_list_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var PageCollectionListDialog = /** @class */ (function (_super) {
        __extends(PageCollectionListDialog, _super);
        function PageCollectionListDialog(pageId, options) {
            var _this = _super.call(this, options) || this;
            _this.pageId = pageId;
            return _this;
        }
        Object.defineProperty(PageCollectionListDialog.prototype, "typeName", {
            get: function () { return "BrandUpPages.PageCollectionListDialog"; },
            enumerable: true,
            configurable: true
        });
        PageCollectionListDialog.prototype._onRenderContent = function () {
            var _this = this;
            this.setHeader("Коллекции страниц");
            this.setNotes("Просмотр и управление коллекциями страниц.");
            _super.prototype._onRenderContent.call(this);
            this.registerCommand("item-create", function () {
                page_collection_create_1.createPageCollection(_this.pageId).then(function (pageCollection) {
                    _this.loadItems();
                });
            });
            this.registerItemCommand("item-update", function (itemId, el) {
                page_collection_update_1.updatePageCollection(itemId).then(function (pageCollection) {
                    _this.loadItems();
                });
            });
            this.registerItemCommand("item-delete", function (itemId, el) {
                page_collection_delete_1.deletePageCollection(itemId).then(function (id) {
                    _this.loadItems();
                });
            });
            this.registerItemCommand("page-list", function (itemId) {
                page_list_1.listPage(itemId);
            });
            this.loadItems();
        };
        PageCollectionListDialog.prototype._getItemsUrl = function () {
            return "/brandup.pages/collection";
        };
        PageCollectionListDialog.prototype._onSetUrlParams = function (urlParams) {
            if (this.pageId)
                urlParams["pageId"] = this.pageId;
        };
        PageCollectionListDialog.prototype._getItemId = function (item) {
            return item.id;
        };
        PageCollectionListDialog.prototype._renderItemContent = function (item, contentElem) {
            contentElem.appendChild(brandup_ui_1.DOM.tag("div", { class: "title" }, brandup_ui_1.DOM.tag("a", { href: "", "data-command": "page-list" }, item.title)));
        };
        PageCollectionListDialog.prototype._renderItemMenu = function (item, menuElem) {
            menuElem.appendChild(brandup_ui_1.DOM.tag("li", null, [brandup_ui_1.DOM.tag("a", { href: "", "data-command": "page-list" }, "View pages")]));
            menuElem.appendChild(brandup_ui_1.DOM.tag("li", { class: "split" }));
            menuElem.appendChild(brandup_ui_1.DOM.tag("li", null, [brandup_ui_1.DOM.tag("a", { href: "", "data-command": "item-update" }, "Edit")]));
            menuElem.appendChild(brandup_ui_1.DOM.tag("li", null, [brandup_ui_1.DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
        };
        PageCollectionListDialog.prototype._renderEmpty = function (container) {
            container.innerText = "Коллекций не создано.";
        };
        PageCollectionListDialog.prototype._renderNewItem = function (containerElem) {
            containerElem.appendChild(brandup_ui_1.DOM.tag("a", { href: "", "data-command": "item-create" }, "Создать коллекцию страниц"));
        };
        return PageCollectionListDialog;
    }(dialog_list_1.ListDialog));
    exports.PageCollectionListDialog = PageCollectionListDialog;
    exports.listPageCollection = function (pageId) {
        var dialog = new PageCollectionListDialog(pageId);
        return dialog.open();
    };
});
//# sourceMappingURL=page-collection-list.js.map