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
define(["require", "exports", "./dialog", "brandup-ui"], function (require, exports, dialog_1, brandup_ui_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var ListDialog = /** @class */ (function (_super) {
        __extends(ListDialog, _super);
        function ListDialog() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        ListDialog.prototype._onRenderContent = function () {
            var _this = this;
            this.element.classList.add("website-dialog-list");
            this.__itemsElem = brandup_ui_1.DOM.tag("div", { class: "items" });
            this.content.appendChild(this.__itemsElem);
            var newItemElem = brandup_ui_1.DOM.tag("div", { class: "new-item" });
            this.content.appendChild(newItemElem);
            this._renderNewItem(newItemElem);
            this.registerCommand("item-open-menu", function (el) {
                el.parentElement.parentElement.classList.add("opened-menu");
            });
            this.__closeItemMenuFunc = function (e) {
                var itemElem = e.target.closest(".opened-menu");
                if (!itemElem)
                    _this.__hideItemMenu();
            };
            document.body.addEventListener("mousedown", this.__closeItemMenuFunc);
        };
        ListDialog.prototype.loadItems = function () {
            var _this = this;
            this.setLoading(true);
            var urlParams = {};
            this._onSetUrlParams(urlParams);
            brandup_ui_1.ajaxRequest({
                url: this._getItemsUrl(),
                urlParams: urlParams,
                success: function (data, status) {
                    _this.setLoading(false);
                    if (status !== 200) {
                        _this.setError("Error loading items.");
                        return;
                    }
                    _this.__renderItems(data);
                }
            });
        };
        ListDialog.prototype.registerItemCommand = function (name, execute, canExecute) {
            var _this = this;
            this.registerCommand(name, function (elem) {
                var itemId = _this._findItemIdFromElement(elem);
                if (itemId === null)
                    return;
                execute(itemId, elem);
            }, function (elem) {
                if (!canExecute)
                    return true;
                var itemId = _this._findItemIdFromElement(elem);
                if (itemId === null)
                    return false;
                return canExecute(itemId, elem);
            });
        };
        ListDialog.prototype._findItemIdFromElement = function (elem) {
            var itemElem = elem.closest(".item[data-id]");
            if (!itemElem)
                return null;
            return itemElem.getAttribute("data-id");
        };
        ListDialog.prototype.__renderItems = function (items) {
            var fragment = document.createDocumentFragment();
            if (items && items.length) {
                for (var i = 0; i < items.length; i++) {
                    var item = items[i];
                    var itemId = this._getItemId(item);
                    var itemElem = brandup_ui_1.DOM.tag("div", { class: "item", "data-id": itemId });
                    this.__renderItem(itemId, item, itemElem);
                    fragment.appendChild(itemElem);
                }
            }
            else {
                var emptyContainer = brandup_ui_1.DOM.tag("div", { class: "empty" });
                this._renderEmpty(emptyContainer);
                fragment.appendChild(emptyContainer);
            }
            brandup_ui_1.DOM.empty(this.__itemsElem);
            this.__itemsElem.appendChild(fragment);
        };
        ListDialog.prototype.__renderItem = function (itemId, item, elem) {
            var contentElem;
            var menuElem;
            elem.appendChild(contentElem = brandup_ui_1.DOM.tag("div", { class: "content" }));
            elem.appendChild(brandup_ui_1.DOM.tag("div", { class: "menu" }, [
                brandup_ui_1.DOM.tag("button", { "data-command": "item-open-menu" }),
                menuElem = brandup_ui_1.DOM.tag("ul")
            ]));
            this._renderItemContent(item, contentElem);
            this._renderItemMenu(item, menuElem);
        };
        ListDialog.prototype.__hideItemMenu = function () {
            brandup_ui_1.DOM.removeClass(this.__itemsElem, ".opened-menu", "opened-menu");
        };
        ListDialog.prototype.destroy = function () {
            document.body.removeEventListener("mousedown", this.__closeItemMenuFunc);
            _super.prototype.destroy.call(this);
        };
        return ListDialog;
    }(dialog_1.Dialog));
    exports.ListDialog = ListDialog;
});
//# sourceMappingURL=dialog-list.js.map