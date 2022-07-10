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
import { FieldDesigner } from "./base";
import { DOM } from "brandup-ui";
import { editPage } from "../../dialogs/pages/edit";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import "./model.less";
var ModelDesigner = /** @class */ (function (_super) {
    __extends(ModelDesigner, _super);
    function ModelDesigner() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Object.defineProperty(ModelDesigner.prototype, "typeName", {
        get: function () { return "BrandUpPages.ModelDesigner"; },
        enumerable: false,
        configurable: true
    });
    ModelDesigner.prototype.onRender = function (elem) {
        var _this = this;
        elem.classList.add("content-designer");
        this.registerCommand("item-add", function (elem) {
            var itemType = elem.getAttribute("data-item-type");
            var itemIndex = -1;
            if (_this.options.isListValue) {
                if (elem.parentElement.hasAttribute("content-path-index"))
                    itemIndex = parseInt(elem.parentElement.getAttribute("content-path-index")) + 1;
                else
                    itemIndex = _this.countItems();
            }
            if (!itemType) {
                selectContentType(_this.options.itemTypes).then(function (type) {
                    _this.addItem(type.name, itemIndex);
                });
            }
            else
                _this.addItem(itemType, itemIndex);
        });
        this.registerCommand("item-view", function () { return; });
        this.registerCommand("item-settings", function (elem) {
            var itemElem = elem.closest("[content-path]");
            var contentPath = itemElem.getAttribute("content-path");
            editPage(_this.page.editId, contentPath).then(function () {
                _this.__refreshItem(itemElem);
            }).catch(function () {
                _this.__refreshItem(itemElem);
            });
        });
        this.registerCommand("item-delete", function (elem) {
            var itemElem = elem.closest("[content-path-index]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");
            var itemIndex = itemElem.getAttribute("content-path-index");
            itemElem.remove();
            _this._refreshBlockIndexes();
            _this._renderBlocks();
            _this.request({
                url: '/brandup.pages/content/model',
                urlParams: { itemIndex: itemIndex },
                method: "DELETE",
                success: function () { return itemElem.classList.remove("processing"); }
            });
        });
        this.registerCommand("item-up", function (elem) {
            var itemElem = elem.closest("[content-path-index]");
            var itemIndex = itemElem.getAttribute("content-path-index");
            if (parseInt(itemIndex) <= 0)
                return;
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");
            itemElem.previousElementSibling.insertAdjacentElement("beforebegin", itemElem);
            _this._refreshBlockIndexes();
            _this._renderBlocks();
            _this.request({
                url: '/brandup.pages/content/model/up',
                urlParams: { itemIndex: itemIndex },
                method: "POST",
                success: function () { return itemElem.classList.remove("processing"); }
            });
        });
        this.registerCommand("item-down", function (elem) {
            var itemElem = elem.closest("[content-path-index]");
            var itemIndex = itemElem.getAttribute("content-path-index");
            if (parseInt(itemIndex) >= DOM.queryElements(_this.element, "* > [content-path-index]").length - 1)
                return;
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");
            itemElem.nextElementSibling.insertAdjacentElement("afterend", itemElem);
            _this._refreshBlockIndexes();
            _this._renderBlocks();
            _this.request({
                url: '/brandup.pages/content/model/down',
                urlParams: { itemIndex: itemIndex },
                method: "POST",
                success: function () { return itemElem.classList.remove("processing"); }
            });
        });
        this.registerCommand("item-refresh", function (elem) {
            var itemElem = elem.closest("[content-path]");
            if (itemElem.classList.contains("processing"))
                return;
            itemElem.classList.add("processing");
            _this.__refreshItem(itemElem);
        });
        this._renderBlocks();
    };
    ModelDesigner.prototype.eachItems = function (f) {
        for (var i = 0; i < this.element.children.length; i++) {
            var itemElem = this.element.children.item(i);
            if (!itemElem.hasAttribute("content-path-index"))
                continue;
            f(itemElem, i);
        }
    };
    ModelDesigner.prototype.countItems = function () {
        var i = 0;
        this.eachItems(function () { return i++; });
        return i;
    };
    ModelDesigner.prototype.getItem = function (index) {
        var itemElem;
        for (var i = 0; i < this.element.children.length; i++) {
            itemElem = this.element.children.item(i);
            if (!itemElem.hasAttribute("content-path-index"))
                continue;
            if (i === index)
                break;
        }
        return itemElem;
    };
    ModelDesigner.prototype._renderBlocks = function () {
        var _this = this;
        this.eachItems(function (elem) { _this._renderBlock(elem); });
    };
    ModelDesigner.prototype._renderBlock = function (itemElem) { };
    ModelDesigner.prototype._refreshBlockIndexes = function () {
        this.eachItems(function (elem, index) { elem.setAttribute("content-path-index", index.toString()); });
    };
    ModelDesigner.prototype.__refreshItem = function (elem) {
        var _this = this;
        var urlParams = {};
        if (this.options.isListValue)
            urlParams["itemIndex"] = elem.getAttribute("content-path-index");
        this.request({
            url: '/brandup.pages/content/model/view',
            urlParams: urlParams,
            method: "GET",
            success: function (response) {
                if (response.status === 200) {
                    var fragment = document.createDocumentFragment();
                    var container = DOM.tag("div", null, response.data);
                    fragment.appendChild(container);
                    var newElem = DOM.queryElement(container, "[content-path]");
                    elem.insertAdjacentElement("afterend", newElem);
                    elem.remove();
                    _this._renderBlock(newElem);
                    _this.page.render();
                }
            }
        });
    };
    ModelDesigner.prototype.hasValue = function () {
        for (var i = 0; i < this.element.children.length; i++) {
            var itemElem = this.element.children.item(i);
            if (itemElem.hasAttribute("content-path-index"))
                return true;
        }
        return false;
    };
    ModelDesigner.prototype.addItem = function (itemType, index) {
        var _this = this;
        this.request({
            url: '/brandup.pages/content/model',
            urlParams: {
                itemType: itemType,
                itemIndex: index.toString()
            },
            method: "PUT",
            success: function (response) {
                if (response.status === 200) {
                    _this.request({
                        url: '/brandup.pages/content/model/view',
                        urlParams: { itemIndex: index.toString() },
                        method: "GET",
                        success: function (response) {
                            if (response.status === 200) {
                                var fragment = document.createDocumentFragment();
                                var container = DOM.tag("div", null, response.data);
                                fragment.appendChild(container);
                                var newElem = DOM.queryElement(container, "[content-path]");
                                if (_this.options.isListValue) {
                                    if (index > 0)
                                        _this.getItem(index - 1).insertAdjacentElement("afterend", newElem);
                                    else if (index === 0)
                                        _this.element.insertAdjacentElement("afterbegin", newElem);
                                    else
                                        _this.getItem(-1).insertAdjacentElement("afterend", newElem);
                                }
                                _this.page.render();
                                _this._renderBlock(newElem);
                                _this._refreshBlockIndexes();
                            }
                        }
                    });
                }
            }
        });
    };
    return ModelDesigner;
}(FieldDesigner));
export { ModelDesigner };
