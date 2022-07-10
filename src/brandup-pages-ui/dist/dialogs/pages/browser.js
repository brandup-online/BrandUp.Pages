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
import { createPage } from "./create";
import { createPageCollection } from "../collections/create";
import { deletePage } from "./delete";
import { listPageCollection } from "../collections/list";
import iconClose from "../../svg/list-item-add.svg";
var PageBrowserDialog = /** @class */ (function (_super) {
    __extends(PageBrowserDialog, _super);
    function PageBrowserDialog(pageId, options) {
        var _this = _super.call(this, options) || this;
        _this.collectionId = null;
        _this.__pageId = pageId;
        _this.setSorting(true);
        return _this;
    }
    Object.defineProperty(PageBrowserDialog.prototype, "pageId", {
        get: function () { return this.__pageId; },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(PageBrowserDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageBrowserDialog"; },
        enumerable: false,
        configurable: true
    });
    PageBrowserDialog.prototype._onRenderContent = function () {
        var _this = this;
        _super.prototype._onRenderContent.call(this);
        this.setHeader("Страницы");
        this.setNotes("Просмотр и управление страницами.");
        this.registerCommand("nav", function (elem) {
            var pageId = elem.getAttribute("data-page-id");
            _this.__pageId = pageId;
            _this.collectionId = null;
            _this.refresh();
        });
        this.registerCommand("item-create", function () {
            if (!_this.collectionId)
                return;
            createPage(_this.collectionId).then(function (createdItem) {
                _this.loadItems();
            });
        });
        this.registerItemCommand("item-open", function (itemId, model) {
            location.href = model.url;
        });
        this.registerItemCommand("item-delete", function (itemId) {
            deletePage(itemId).then(function (deletedItem) {
                _this.loadItems();
            });
        });
        this.registerCommand("select-collection", function (elem) {
            var collectionId = elem.getAttribute("data-value");
            _this.selectCollection(collectionId, true);
        });
        this.registerCommand("collection-sesttings", function () {
            listPageCollection(_this.pageId).then(function () {
                _this.refresh();
            });
        });
        this.registerCommand("create-collection", function () {
            createPageCollection(_this.pageId).then(function () {
                _this.refresh();
            });
        });
    };
    PageBrowserDialog.prototype.selectCollection = function (collectionId, needLoad) {
        if (this.collectionId)
            DOM.removeClass(this.tabsElem, "a[data-value]", "selected");
        this.collectionId = collectionId;
        for (var i = 0; i < this.__model.collections.length; i++) {
            var collection = this.__model.collections[i];
            if (collection.id == collectionId) {
                this.setSorting(collection.customSorting);
                break;
            }
        }
        if (this.collectionId) {
            var tabItem = DOM.queryElement(this.tabsElem, "a[data-value=\"".concat(this.collectionId, "\"]"));
            if (tabItem)
                tabItem.classList.add("selected");
            else
                this.collectionId = null;
        }
        if (needLoad)
            this.loadItems();
    };
    PageBrowserDialog.prototype._allowLoadItems = function () {
        return this.collectionId ? true : false;
    };
    PageBrowserDialog.prototype._buildUrl = function () {
        return "/brandup.pages/page/list";
    };
    PageBrowserDialog.prototype._buildUrlParams = function (urlParams) {
        if (this.__pageId)
            urlParams["pageId"] = this.__pageId;
        urlParams["collectionId"] = this.collectionId;
    };
    PageBrowserDialog.prototype._buildList = function (model) {
        if (this.__createCollElem) {
            this.__createCollElem.remove();
            this.__createCollElem = null;
        }
        if (!this.tabsElem) {
            this.navElem = DOM.tag("ol", { class: "nav" });
            this.content.insertAdjacentElement("afterbegin", this.navElem);
            this.tabsElem = DOM.tag("ul", { class: "tabs" });
            this.navElem.insertAdjacentElement("afterend", this.tabsElem);
        }
        else {
            DOM.empty(this.tabsElem);
            DOM.empty(this.navElem);
        }
        this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav" }, "root")));
        if (model.parents && model.parents.length) {
            for (var i = 0; i < model.parents.length; i++) {
                var pagePath = model.parents[i];
                this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav", "data-page-id": pagePath.id }, pagePath.header)));
            }
        }
        for (var i = 0; i < model.collections.length; i++) {
            var collection = model.collections[i];
            this.tabsElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-value": collection.id, "data-command": "select-collection" }, collection.title)));
        }
        var colId = this.collectionId;
        if (model.collections.length) {
            this.tabsElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "collection-sesttings", class: "secondary", title: "коллекций страниц" }, "коллекции")));
            if (!colId)
                colId = model.collections[0].id;
        }
        else {
            colId = null;
            this.__itemsElem.insertAdjacentElement("beforebegin", this.__createCollElem = DOM.tag("div", { class: "empty" }, [
                DOM.tag("div", { class: "text" }, "Для страницы не создано коллекций страниц."),
                DOM.tag("div", { class: "buttons" }, DOM.tag("button", { class: "bp-button", "data-command": "create-collection" }, "Создать коллекцию"))
            ]));
        }
        this.selectCollection(colId, false);
    };
    PageBrowserDialog.prototype._getItemId = function (item) {
        return item.id;
    };
    PageBrowserDialog.prototype._renderItemContent = function (item, contentElem) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, [
            DOM.tag("a", { href: "", "data-command": "nav", "data-page-id": item.id, title: item.title }, item.title),
            DOM.tag("div", { class: "text", title: item.url }, item.url)
        ]));
        contentElem.appendChild(DOM.tag("div", { class: "status ".concat(item.status.toLowerCase()) }, item.status));
    };
    PageBrowserDialog.prototype._renderItemMenu = function (item, menuElem) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-open" }, "Открыть")]));
        menuElem.appendChild(DOM.tag("li", { class: "split" }));
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Удалить")]));
    };
    PageBrowserDialog.prototype._renderEmpty = function (container) {
        container.innerText = "Страниц не создано.";
    };
    PageBrowserDialog.prototype._renderNewItem = function (containerElem) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, [iconClose, "Новая страница"]));
    };
    return PageBrowserDialog;
}(ListDialog));
export { PageBrowserDialog };
export var browserPage = function (pageId) {
    var dialog = new PageBrowserDialog(pageId);
    return dialog.open();
};
