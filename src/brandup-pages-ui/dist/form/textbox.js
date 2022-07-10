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
import "./textbox.less";
var Textbox = /** @class */ (function (_super) {
    __extends(Textbox, _super);
    function Textbox() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Object.defineProperty(Textbox.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.Field.Text"; },
        enumerable: false,
        configurable: true
    });
    Textbox.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("text");
        this.__valueElem = DOM.tag("div", { class: "value", "tabindex": 0, contenteditable: true });
        this.element.appendChild(this.__valueElem);
        var placeholderElem = DOM.tag("div", { class: "placeholder" }, this.options.placeholder);
        placeholderElem.addEventListener("click", function () {
            _this.__valueElem.focus();
        });
        this.element.appendChild(placeholderElem);
        this.__valueElem.addEventListener("paste", function (e) {
            _this.__isChanged = true;
            e.preventDefault();
            var text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, _this.normalizeValue(text));
        });
        this.__valueElem.addEventListener("cut", function () {
            _this.__isChanged = true;
        });
        this.__valueElem.addEventListener("keydown", function (e) {
            if (!_this.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }
        });
        this.__valueElem.addEventListener("keyup", function () {
            _this.__isChanged = true;
        });
        this.__valueElem.addEventListener("focus", function () {
            _this.__isChanged = false;
            _this.element.classList.add("focused");
        });
        this.__valueElem.addEventListener("blur", function () {
            _this.element.classList.remove("focused");
            if (_this.__isChanged)
                _this._onChanged();
        });
    };
    Textbox.prototype.__refreshUI = function () {
        var hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    };
    Textbox.prototype._onChanged = function () {
        this.__refreshUI();
        this.raiseChanged();
    };
    Textbox.prototype.getValue = function () {
        var val = this.normalizeValue(this.__valueElem.innerText);
        return val ? val : null;
    };
    Textbox.prototype.setValue = function (value) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline) {
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        }
        this.__valueElem.innerHTML = value ? value : "";
        this.__refreshUI();
    };
    Textbox.prototype.hasValue = function () {
        var val = this.normalizeValue(this.__valueElem.innerText);
        return val ? true : false;
    };
    Textbox.prototype.normalizeValue = function (value) {
        if (!value)
            return "";
        value = value.trim();
        if (!this.options.allowMultiline)
            value = value.replace("\n\r", " ");
        return value;
    };
    return Textbox;
}(Field));
export { Textbox };
