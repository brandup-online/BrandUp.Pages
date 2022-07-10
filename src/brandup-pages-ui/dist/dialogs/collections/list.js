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
import { createPageCollection } from "./create";
import { deletePageCollection } from "./delete";
import { updatePageCollection } from "./update";
import { ListDialog } from "../dialog-list";
import { DOM } from "brandup-ui";
var PageCollectionListDialog = /** @class */ (function (_super) {
    __extends(PageCollectionListDialog, _super);
    function PageCollectionListDialog(pageId, options) {
        var _this = _super.call(this, options) || this;
        _this.__isModified = false;
        _this.pageId = pageId;
        return _this;
    }
    Object.defineProperty(PageCollectionListDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageCollectionListDialog"; },
        enumerable: false,
        configurable: true
    });
    PageCollectionListDialog.prototype._onRenderContent = function () {
        var _this = this;
        _super.prototype._onRenderContent.call(this);
        this.setHeader("Коллекции страниц");
        this.setNotes("Просмотр и управление коллекциями страниц.");
        this.registerCommand("item-create", function () {
            createPageCollection(_this.pageId).then(function (createdItem) {
                _this.loadItems();
                _this.__isModified = true;
            });
        });
        this.registerItemCommand("item-update", function (pageCollectionId, el) {
            updatePageCollection(pageCollectionId).then(function (updatedItem) {
                _this.loadItems();
                _this.__isModified = true;
            });
        });
        this.registerItemCommand("item-delete", function (pageCollectionId, el) {
            deletePageCollection(pageCollectionId).then(function (deletedItem) {
                _this.loadItems();
                _this.__isModified = true;
            });
        });
    };
    PageCollectionListDialog.prototype._onClose = function () {
        if (this.__isModified)
            this.resolve(null);
        else
            _super.prototype._onClose.call(this);
    };
    PageCollectionListDialog.prototype._buildUrl = function () {
        return "/brandup.pages/collection/list";
    };
    PageCollectionListDialog.prototype._buildUrlParams = function (urlParams) {
        if (this.pageId)
            urlParams["pageId"] = this.pageId;
    };
    PageCollectionListDialog.prototype._buildList = function (model) {
        if (!this.navElem) {
            this.navElem = DOM.tag("ol", { class: "nav" });
            this.content.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }
        this.navElem.appendChild(DOM.tag("li", null, DOM.tag("span", null, "root")));
        if (model.parents && model.parents.length) {
            for (var i = 0; i < model.parents.length; i++) {
                this.navElem.appendChild(DOM.tag("li", null, DOM.tag("span", {}, model.parents[i])));
            }
        }
    };
    PageCollectionListDialog.prototype._getItemId = function (item) {
        return item.id;
    };
    PageCollectionListDialog.prototype._renderItemContent = function (item, contentElem) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("span", {}, item.title)));
    };
    PageCollectionListDialog.prototype._renderItemMenu = function (item, menuElem) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-update" }, "Редактировать")]));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Удалить")]));
    };
    PageCollectionListDialog.prototype._renderEmpty = function (container) {
        container.innerText = "Коллекций не создано.";
    };
    PageCollectionListDialog.prototype._renderNewItem = function (containerElem) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Создать коллекцию страниц"));
    };
    return PageCollectionListDialog;
}(ListDialog));
export { PageCollectionListDialog };
export var listPageCollection = function (pageId) {
    var dialog = new PageCollectionListDialog(pageId);
    return dialog.open();
};
