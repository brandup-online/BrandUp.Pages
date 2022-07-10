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
import "./combobox.less";
import iconArrow from "../svg/combobox-arrow.svg";
var ComboBoxField = /** @class */ (function (_super) {
    __extends(ComboBoxField, _super);
    function ComboBoxField() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.__value = null;
        return _this;
    }
    Object.defineProperty(ComboBoxField.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.ComboBoxField"; },
        enumerable: false,
        configurable: true
    });
    ComboBoxField.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("combobox");
        this.element.setAttribute("tabindex", "0");
        this.element.appendChild(DOM.tag("i", null, iconArrow));
        this.__valueElem = DOM.tag("div", { class: "value" });
        this.element.appendChild(this.__valueElem);
        var placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "toggle" }, this.options.placeholder);
        this.element.appendChild(placeholderElem);
        this.__itemsElem = DOM.tag("ul");
        this.element.appendChild(this.__itemsElem);
        var isFocused = false;
        var md = false;
        this.addEventListener("focus", function () {
            isFocused = true;
        });
        this.addEventListener("blur", function () {
            isFocused = false;
        });
        placeholderElem.addEventListener("mousedown", function () {
            md = isFocused;
        });
        placeholderElem.addEventListener("mouseup", function () {
            if (md && isFocused)
                _this.element.blur();
        });
        this.registerCommand("select", function (elem) {
            DOM.removeClass(_this.__itemsElem, ".selected", "selected");
            elem.classList.add("selected");
            _this.__value = elem.getAttribute("data-value");
            _this.__valueElem.innerText = elem.innerText;
            _this.__refreshUI();
            _this.element.blur();
            _this.raiseChanged();
        });
    };
    ComboBoxField.prototype.__refreshUI = function () {
        var hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    };
    ComboBoxField.prototype.addItem = function (item) {
        this.__itemsElem.appendChild(DOM.tag("li", { "data-value": item.value, "data-command": "select" }, item.title));
    };
    ComboBoxField.prototype.addItems = function (items) {
        if (items) {
            for (var i = 0; i < items.length; i++)
                this.addItem(items[i]);
        }
    };
    ComboBoxField.prototype.clearItems = function () {
        DOM.empty(this.__valueElem);
        this.__value = null;
    };
    ComboBoxField.prototype.getValue = function () {
        return this.__value;
    };
    ComboBoxField.prototype.setValue = function (value) {
        var text = "";
        if (value !== null) {
            var itemElem = DOM.queryElement(this.__itemsElem, "li[data-value=\"".concat(value, "\"]"));
            if (!itemElem) {
                this.setValue(null);
                return;
            }
            text = itemElem.innerText;
            itemElem.classList.add("selected");
        }
        else
            DOM.removeClass(this.__itemsElem, ".selected", "selected");
        this.__value = value;
        this.__valueElem.innerText = text;
        this.__refreshUI();
    };
    ComboBoxField.prototype.hasValue = function () {
        return this.__value ? true : false;
    };
    return ComboBoxField;
}(Field));
export { ComboBoxField };
