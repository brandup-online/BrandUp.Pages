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
import "./text.less";
var TextDesigner = /** @class */ (function (_super) {
    __extends(TextDesigner, _super);
    function TextDesigner() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Object.defineProperty(TextDesigner.prototype, "typeName", {
        get: function () { return "BrandUpPages.TextDesigner"; },
        enumerable: false,
        configurable: true
    });
    TextDesigner.prototype.onRender = function (elem) {
        var _this = this;
        elem.classList.add("text-designer");
        elem.setAttribute("tabindex", "0");
        elem.contentEditable = "true";
        if (this.options.placeholder)
            elem.setAttribute("data-placeholder", this.options.placeholder);
        elem.addEventListener("paste", function (e) {
            _this.__isChanged = true;
            e.preventDefault();
            var text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, _this.normalizeValue(text));
        });
        elem.addEventListener("cut", function () {
            _this.__isChanged = true;
        });
        elem.addEventListener("keydown", function (e) {
            if (!_this.options.allowMultiline && e.keyCode === 13) {
                e.preventDefault();
                return false;
            }
            _this.__refreshUI();
        });
        elem.addEventListener("keyup", function () {
            _this.__isChanged = true;
        });
        elem.addEventListener("focus", function () {
            _this.__isChanged = false;
            _this.page.accentField(_this);
        });
        elem.addEventListener("blur", function () {
            if (_this.__isChanged)
                _this._onChanged();
            _this.page.clearAccent();
        });
        elem.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();
        });
        this.__refreshUI();
    };
    TextDesigner.prototype.getValue = function () {
        var val = this.normalizeValue(this.element.innerText);
        return val ? val : null;
    };
    TextDesigner.prototype.setValue = function (value) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline)
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        this.element.innerHTML = value ? value : "";
        this.__refreshUI();
    };
    TextDesigner.prototype.hasValue = function () {
        var val = this.normalizeValue(this.element.innerText);
        return val ? true : false;
    };
    TextDesigner.prototype._onChanged = function () {
        var _this = this;
        this.__refreshUI();
        var value = this.getValue();
        this.request({
            url: '/brandup.pages/content/text',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: function (response) {
                if (response.status === 200) {
                    _this.setValue(response.data);
                }
            }
        });
    };
    TextDesigner.prototype.__refreshUI = function () {
        if (this.hasValue())
            this.element.classList.remove("empty-value");
        else {
            this.element.classList.add("empty-value");
        }
    };
    TextDesigner.prototype.normalizeValue = function (value) {
        if (!value)
            return "";
        value = value.trim();
        if (!this.options.allowMultiline)
            value = value.replace("\n\r", " ");
        return value;
    };
    return TextDesigner;
}(FieldDesigner));
export { TextDesigner };
