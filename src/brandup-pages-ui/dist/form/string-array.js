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
import { Field } from "./field";
import { DOM } from "brandup-ui";
import "./string-array.less";
import iconDelete from "../svg/toolbar-button-discard.svg";
var StringArrayField = /** @class */ (function (_super) {
    __extends(StringArrayField, _super);
    function StringArrayField() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.__items = [];
        return _this;
    }
    Object.defineProperty(StringArrayField.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.Field.StringArray"; },
        enumerable: false,
        configurable: true
    });
    StringArrayField.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("string-array");
        this.element.appendChild(this.__itemsElem = DOM.tag("div", { class: "items" }));
        this.__renderItems();
        this.__refreshUI();
        this.registerCommand("item-delete", function (elem) {
            var itemElem = elem.closest(".item");
            var index = parseInt(itemElem.getAttribute("data-index"));
            itemElem.remove();
            _this.__items.splice(index, 1);
            _this.__refreshIndexes();
        });
        this.element.addEventListener("keyup", function (e) {
            var t = e.target;
            var elem = t.closest(".item");
            var index = parseInt(elem.getAttribute("data-index"));
            var value = _this.normalizeValue(t.value);
            if (value) {
                elem.classList.add("has-value");
                if (_this.__items.length - 1 < index)
                    _this.__items.push(value);
                else
                    _this.__items[index] = value;
            }
            else
                elem.classList.remove("has-value");
            if (index === _this.__itemsElem.childElementCount - 1)
                _this.__itemsElem.appendChild(_this.__createItemElem("", index + 1));
        });
        this.element.addEventListener("change", function (e) {
            var t = e.target;
            var elem = t.closest(".item");
            var index = parseInt(elem.getAttribute("data-index"));
            var value = _this.normalizeValue(t.value);
            if (!value) {
                elem.remove();
                _this.__items.splice(index, 1);
                _this.__refreshIndexes();
            }
        });
        //this.__itemsElem.addEventListener("mousedown", (e: MouseEvent) => {
        //    let target = <Element>e.target;
        //    if (target.tagName === "INPUT") {
        //        e.stopImmediatePropagation();
        //        e.stopPropagation();
        //        console.log("input");
        //        return false;
        //    }
        //});
        this.__itemsElem.addEventListener("dragstart", function (e) {
            var target = e.target;
            var itemElem = target.closest("[data-index]");
            if (itemElem && itemElem.classList.contains("has-value")) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("data-index", itemElem.getAttribute('data-index'));
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
            var sourceIndex = parseInt(e.dataTransfer.getData("data-index"));
            var elem = target.closest("[data-index]");
            if (elem) {
                var destIndex = parseInt(elem.getAttribute("data-index"));
                if (destIndex !== sourceIndex) {
                    console.log("Source: ".concat(sourceIndex, "; Dest: ").concat(destIndex));
                    var sourceElem = DOM.queryElement(_this.__itemsElem, "[data-index=\"".concat(sourceIndex, "\"]"));
                    if (sourceElem) {
                        if (destIndex < sourceIndex) {
                            elem.insertAdjacentElement("beforebegin", sourceElem);
                        }
                        else {
                            elem.insertAdjacentElement("afterend", sourceElem);
                        }
                        var removed = _this.__items.splice(sourceIndex, 1);
                        _this.__items.splice(destIndex, 0, removed[0]);
                        _this.__refreshIndexes();
                        console.log(_this.__items);
                    }
                }
            }
            e.stopPropagation();
            return false;
        });
    };
    StringArrayField.prototype.__renderItems = function () {
        DOM.empty(this.__itemsElem);
        var i = 0;
        for (i = 0; i < this.__items.length; i++) {
            var item = this.__items[i];
            this.__createItemElem(item, i);
        }
        this.__createItemElem("", i + 1);
    };
    StringArrayField.prototype.__createItemElem = function (item, index) {
        var itemElem = DOM.tag("div", { class: "item", "data-index": index.toString() }, [
            DOM.tag("div", { class: "index", draggable: "true", title: "Нажмите, чтобы перетащить" }, "#".concat(index + 1)),
            DOM.tag("input", { type: "text", value: item, placeholder: this.options.placeholder ? this.options.placeholder : "" }),
            DOM.tag("ul", null, [
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-delete", title: "Удалить" }, iconDelete))
            ])
        ]);
        if (item)
            itemElem.classList.add("has-value");
        this.__itemsElem.appendChild(itemElem);
        return itemElem;
    };
    StringArrayField.prototype._onChanged = function () {
        this.__refreshUI();
        this.raiseChanged();
    };
    StringArrayField.prototype.__refreshUI = function () {
        var hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    };
    StringArrayField.prototype.__refreshIndexes = function () {
        for (var i = 0; i < this.__itemsElem.childElementCount; i++) {
            var elem = this.__itemsElem.children.item(i);
            elem.setAttribute("data-index", i.toString());
            DOM.getElementByClass(elem, "index").innerText = "#".concat(i + 1);
        }
    };
    StringArrayField.prototype.getValue = function () {
        if (this.hasValue())
            return this.__items;
        return null;
    };
    StringArrayField.prototype.setValue = function (value) {
        var temp = new Array();
        if (value) {
            for (var i = 0; i < value.length; i++) {
                var item = value[i];
                if (!item)
                    continue;
                temp.push(item);
            }
        }
        this.__items = temp;
        this.__renderItems();
        this.__refreshIndexes();
    };
    StringArrayField.prototype.hasValue = function () {
        return this.__items !== null && this.__items.length !== 0;
    };
    StringArrayField.prototype.normalizeValue = function (value) {
        if (!value)
            return "";
        return value.trim();
    };
    return StringArrayField;
}(Field));
export { StringArrayField };
