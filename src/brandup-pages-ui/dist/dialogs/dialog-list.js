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
import { Dialog } from "./dialog";
import { ajaxRequest, DOM, AjaxQueue } from "brandup-ui";
import "./dialog-list.less";
import iconDots from "../svg/list-item-dots.svg";
import iconSort from "../svg/list-item-sort.svg";
var ListDialog = /** @class */ (function (_super) {
    __extends(ListDialog, _super);
    function ListDialog(options) {
        var _this = _super.call(this, options) || this;
        _this.__enableSorting = false;
        _this.queue = new AjaxQueue();
        return _this;
    }
    ListDialog.prototype.setSorting = function (enable) {
        this.__enableSorting = enable;
    };
    ListDialog.prototype._onRenderContent = function () {
        var _this = this;
        this.element.classList.add("bp-dialog-list");
        this.__itemsElem = DOM.tag("div", { class: "items" });
        this.content.appendChild(this.__itemsElem);
        this.registerCommand("item-open-menu", function (el) {
            el.parentElement.parentElement.classList.add("opened-menu");
        });
        this.__closeItemMenuFunc = function (e) {
            var itemElem = e.target.closest(".opened-menu");
            if (!itemElem)
                _this.__hideItemMenu();
        };
        document.body.addEventListener("mousedown", this.__closeItemMenuFunc);
        this.__loadList();
        this.__itemsElem.addEventListener("dragstart", function (e) {
            var target = e.target;
            var itemElem = target.closest("[data-index]");
            if (itemElem) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("data-id", itemElem.getAttribute("data-id"));
                e.dataTransfer.setData("data-index", itemElem.getAttribute("data-index"));
                e.dataTransfer.setDragImage(itemElem, 0, 0);
                return true;
            }
            e.stopPropagation();
            e.preventDefault();
            return false;
        }, false);
        this.__itemsElem.addEventListener("dragenter", function (e) {
            e.preventDefault();
            return true;
        });
        this.__itemsElem.addEventListener("dragover", function (e) {
            e.preventDefault();
        });
        this.__itemsElem.addEventListener("drop", function (e) {
            var target = e.target;
            var sourceId = e.dataTransfer.getData("data-id");
            var sourceIndex = parseInt(e.dataTransfer.getData("data-index"));
            var elem = target.closest("[data-index]");
            if (elem) {
                var destId = elem.getAttribute("data-id");
                var destIndex = parseInt(elem.getAttribute("data-index"));
                if (destIndex !== sourceIndex) {
                    var sourceElem = DOM.queryElement(_this.__itemsElem, "[data-index=\"".concat(sourceIndex, "\"]"));
                    if (sourceElem) {
                        var destPosition = void 0;
                        if (destIndex < sourceIndex) {
                            elem.insertAdjacentElement("beforebegin", sourceElem);
                            destPosition = "before";
                        }
                        else {
                            elem.insertAdjacentElement("afterend", sourceElem);
                            destPosition = "after";
                        }
                        console.log("Source: ".concat(sourceIndex, "; Dest: ").concat(destIndex, "; Position: ").concat(destPosition));
                        console.log("Source: ".concat(sourceId, "; Dest: ").concat(destId, "; Position: ").concat(destPosition));
                        _this.__refreshIndexes();
                        var urlParams = {
                            sourceId: sourceId,
                            destId: destId,
                            destPosition: destPosition
                        };
                        _this._buildUrlParams(urlParams);
                        var url = _this._buildUrl();
                        url += "/sort";
                        _this.setLoading(true);
                        ajaxRequest({
                            url: url,
                            urlParams: urlParams,
                            method: "POST",
                            success: function (response) {
                                _this.setLoading(false);
                                if (response.status !== 200) {
                                    _this.setError("Error loading items.");
                                    return;
                                }
                                _this.loadItems();
                            }
                        });
                    }
                }
            }
            e.stopPropagation();
            return false;
        });
    };
    ListDialog.prototype.__refreshIndexes = function () {
        for (var i = 0; i < this.__itemsElem.childElementCount; i++) {
            var elem = this.__itemsElem.children.item(i);
            elem.setAttribute("data-index", i.toString());
        }
    };
    ListDialog.prototype.__loadList = function () {
        var _this = this;
        var urlParams = {};
        this._buildUrlParams(urlParams);
        this.setLoading(true);
        this.queue.push({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "GET",
            success: function (response) {
                _this.setLoading(false);
                switch (response.status) {
                    case 200: {
                        _this.__model = response.data;
                        _this._buildList(_this.__model);
                        _this.loadItems();
                        break;
                    }
                    default: {
                        _this.setError("Error loading list.");
                        return;
                    }
                }
            }
        });
    };
    ListDialog.prototype.refresh = function () {
        this.__loadList();
    };
    ListDialog.prototype.loadItems = function () {
        var _this = this;
        if (!this._allowLoadItems()) {
            DOM.empty(this.__itemsElem);
            if (this.__newItemElem) {
                this.__newItemElem.remove();
                this.__newItemElem = null;
            }
            return;
        }
        this.setLoading(true);
        var urlParams = {};
        this._buildUrlParams(urlParams);
        urlParams["offset"] = "0";
        urlParams["limit"] = "50";
        var url = this._buildUrl();
        url += "/item";
        ajaxRequest({
            url: url,
            urlParams: urlParams,
            success: function (response) {
                _this.setLoading(false);
                if (response.status !== 200) {
                    _this.setError("Error loading items.");
                    return;
                }
                _this.__renderItems(response.data);
            }
        });
    };
    ListDialog.prototype.registerItemCommand = function (name, execute, canExecute) {
        var _this = this;
        this.registerCommand(name, function (elem) {
            var item = _this._findItemIdFromElement(elem);
            if (item === null)
                return;
            execute(item.id, item.model, elem);
        }, function (elem) {
            if (!canExecute)
                return true;
            var item = _this._findItemIdFromElement(elem);
            if (item === null)
                return false;
            return canExecute(item.id, item.model, elem);
        });
    };
    ListDialog.prototype._findItemIdFromElement = function (elem) {
        var itemElem = elem.closest(".item[data-id]");
        if (!itemElem)
            return null;
        return { id: itemElem.getAttribute("data-id"), model: itemElem["_model_"] };
    };
    ListDialog.prototype.__renderItems = function (items) {
        var fragment = document.createDocumentFragment();
        if (items && items.length) {
            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                var itemId = this._getItemId(item);
                var itemElem = DOM.tag("div", { class: "item", "data-id": itemId, "data-index": i.toString() });
                itemElem["_model_"] = item;
                this.__renderItem(itemId, item, itemElem);
                fragment.appendChild(itemElem);
            }
        }
        else {
            var emptyContainer = DOM.tag("div", { class: "empty" });
            this._renderEmpty(emptyContainer);
            fragment.appendChild(emptyContainer);
        }
        DOM.empty(this.__itemsElem);
        this.__itemsElem.appendChild(fragment);
        if (!this.__newItemElem) {
            this.__newItemElem = DOM.tag("div", { class: "new-item" });
            this.content.appendChild(this.__newItemElem);
            this._renderNewItem(this.__newItemElem);
        }
    };
    ListDialog.prototype.__renderItem = function (_itemId, item, elem) {
        var contentElem;
        var menuElem;
        if (this.__enableSorting) {
            elem.appendChild(contentElem = DOM.tag("div", { class: "sort", draggable: "true", title: "Нажмите, чтобы перетащить" }, iconSort));
        }
        elem.appendChild(contentElem = DOM.tag("div", { class: "content" }));
        elem.appendChild(DOM.tag("div", { class: "menu" }, [
            DOM.tag("button", { "data-command": "item-open-menu" }, iconDots),
            menuElem = DOM.tag("ul")
        ]));
        this._renderItemContent(item, contentElem);
        this._renderItemMenu(item, menuElem);
    };
    ListDialog.prototype.__hideItemMenu = function () {
        DOM.removeClass(this.__itemsElem, ".opened-menu", "opened-menu");
    };
    ListDialog.prototype._buildUrlParams = function (_urlParams) { };
    ListDialog.prototype._allowLoadItems = function () { return true; };
    ListDialog.prototype.destroy = function () {
        document.body.removeEventListener("mousedown", this.__closeItemMenuFunc);
        this.queue.destroy();
        _super.prototype.destroy.call(this);
    };
    return ListDialog;
}(Dialog));
export { ListDialog };
