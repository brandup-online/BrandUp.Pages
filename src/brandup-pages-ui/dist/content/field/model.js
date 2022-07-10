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
import { Field } from "../../form/field";
import { DOM } from "brandup-ui";
import iconEdit from "../../svg/toolbar-button-edit.svg";
import iconDelete from "../../svg/toolbar-button-discard.svg";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import "./model.less";
var ModelField = /** @class */ (function (_super) {
    __extends(ModelField, _super);
    function ModelField(form, name, options) {
        var _this = _super.call(this, name, options) || this;
        _this.form = form;
        return _this;
    }
    Object.defineProperty(ModelField.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.Field.Content"; },
        enumerable: false,
        configurable: true
    });
    ModelField.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("content");
        this.__itemsElem = DOM.tag("div", { class: "items" });
        this.element.appendChild(this.__itemsElem);
        this.registerCommand("item-settings", function (elem) {
            var itemElem = elem.closest("[content-path-index]");
            var itemIndex = itemElem.getAttribute("content-path-index");
            var modelPath = "".concat(_this.name, "[").concat(itemIndex, "]");
            if (_this.form.modelPath)
                modelPath = _this.form.modelPath + "." + modelPath;
            _this.form.navigate(modelPath);
            //editPage(this.form.editId, contentPath).then(() => {
            //    this.__refreshItem(itemElem);
            //});
        });
        this.registerCommand("item-delete", function (elem) {
            var itemElem = elem.closest("[content-path-index]");
            var itemIndex = parseInt(itemElem.getAttribute("content-path-index"));
            itemElem.remove();
            _this._refreshBlockIndexes();
            _this.form.request(_this, {
                url: '/brandup.pages/content/model',
                urlParams: { itemIndex: itemIndex.toString() },
                method: "DELETE",
                success: function () { return; }
            });
        });
        this.registerCommand("item-add", function () {
            if (_this.options.itemTypes.length === 1) {
                _this.__addItem(_this.options.itemTypes[0].name);
            }
            else {
                selectContentType(_this.options.itemTypes).then(function (type) {
                    _this.__addItem(type.name);
                });
            }
        });
        this.__itemsElem.addEventListener("dragstart", function (e) {
            var target = e.target;
            var itemElem = target.closest("[content-path-index]");
            if (itemElem) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("content-path-index", itemElem.getAttribute('content-path-index'));
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
            var sourceIndex = e.dataTransfer.getData("content-path-index");
            var elem = target.closest("[content-path-index]");
            if (elem) {
                var destIndex = elem.getAttribute("content-path-index");
                if (destIndex !== sourceIndex) {
                    console.log("Source: ".concat(sourceIndex, "; Dest: ").concat(destIndex));
                    var sourceElem = DOM.queryElement(_this.__itemsElem, "[content-path-index=\"".concat(sourceIndex, "\"]"));
                    if (sourceElem) {
                        if (destIndex < sourceIndex) {
                            elem.insertAdjacentElement("beforebegin", sourceElem);
                        }
                        else {
                            elem.insertAdjacentElement("afterend", sourceElem);
                        }
                        _this.form.request(_this, {
                            url: '/brandup.pages/content/model/move',
                            urlParams: { itemIndex: sourceIndex, newIndex: destIndex },
                            method: "POST",
                            success: function (response) {
                                if (response.status === 200) {
                                    _this.setValue(response.data);
                                }
                            }
                        });
                    }
                }
            }
            e.stopPropagation();
            return false;
        });
    };
    ModelField.prototype.getValue = function () {
        return this.__value;
    };
    ModelField.prototype.setValue = function (value) {
        this.__value = value;
        this.__renderItems();
    };
    ModelField.prototype.hasValue = function () {
        return this.__value && this.__value.items && this.__value.items.length > 0;
    };
    ModelField.prototype.__renderItems = function () {
        DOM.empty(this.__itemsElem);
        var i = 0;
        for (i = 0; i < this.__value.items.length; i++) {
            var item = this.__value.items[i];
            this.__itemsElem.appendChild(this.__createItemElem(item, i));
        }
        this.__itemsElem.appendChild(DOM.tag("div", { class: "item new" }, [
            DOM.tag("div", { class: "index" }, "#".concat(i + 1)),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-add" }, this.options.addText ? this.options.addText : "Добавить")
        ]));
    };
    ModelField.prototype.__createItemElem = function (item, index) {
        var itemElem = DOM.tag("div", { class: "item", "content-path-index": index.toString() }, [
            DOM.tag("div", { class: "index", draggable: "true", title: "Нажмите, чтобы перетащить" }, "#".concat(index + 1)),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-settings" }, item.title),
            DOM.tag("div", { class: "type" }, item.type.title),
            DOM.tag("ul", null, [
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-settings", title: "Редактировать" }, iconEdit)),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-delete", title: "Удалить" }, iconDelete))
            ])
        ]);
        return itemElem;
    };
    ModelField.prototype.eachItems = function (f) {
        for (var i = 0; i < this.__itemsElem.children.length; i++) {
            var itemElem = this.__itemsElem.children.item(i);
            if (!itemElem.hasAttribute("content-path-index"))
                continue;
            f(itemElem, i);
        }
    };
    ModelField.prototype._refreshBlockIndexes = function () {
        this.eachItems(function (elem, index) {
            elem.setAttribute("content-path-index", index.toString());
            DOM.getElementByClass(elem, "index").innerText = "#".concat(index + 1);
        });
    };
    ModelField.prototype.__refreshItem = function (elem) {
        var _this = this;
        var itemIndex = parseInt(elem.getAttribute("content-path-index"));
        this.form.request(this, {
            url: '/brandup.pages/content/model/data',
            urlParams: { itemIndex: itemIndex.toString() },
            method: "GET",
            success: function (response) {
                if (response.status === 200) {
                    _this.__value.items[itemIndex] = response.data;
                    var newElem = _this.__createItemElem(response.data, itemIndex);
                    elem.insertAdjacentElement("afterend", newElem);
                    elem.remove();
                }
            }
        });
    };
    ModelField.prototype.__addItem = function (itemType) {
        var _this = this;
        this.form.request(this, {
            url: '/brandup.pages/content/model',
            urlParams: { itemType: itemType },
            method: "PUT",
            success: function (response) {
                if (response.status === 200) {
                    _this.setValue(response.data);
                }
            }
        });
    };
    ModelField.prototype.__refreshItems = function () {
        var _this = this;
        this.form.request(this, {
            url: '/brandup.pages/content/model',
            method: "GET",
            success: function (response) {
                if (response.status === 200) {
                    _this.setValue(response.data);
                }
            }
        });
    };
    return ModelField;
}(Field));
export { ModelField };
