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
import ContentEditor from "brandup-pages-ckeditor";
import "./html.less";
var HtmlContent = /** @class */ (function (_super) {
    __extends(HtmlContent, _super);
    function HtmlContent(form, name, options) {
        var _this = _super.call(this, name, options) || this;
        _this.form = form;
        return _this;
    }
    Object.defineProperty(HtmlContent.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.Field.Html"; },
        enumerable: false,
        configurable: true
    });
    HtmlContent.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("html");
        this.element.appendChild(this.__value = DOM.tag("div", { class: "value" }));
        if (this.options.placeholder)
            this.__value.setAttribute("data-placeholder", this.options.placeholder);
        ContentEditor.create(this.__value, { placeholder: this.options.placeholder })
            .then(function (editor) {
            _this.__editor = editor;
            editor.model.document.on('change', function () {
                if (editor.model.document.differ.hasDataChanges())
                    _this.__isChanged = true;
                _this.__refreshUI();
            });
            _this.__refreshUI();
        });
        this.__value.addEventListener("focus", function () {
            _this.__isChanged = false;
        });
        this.__value.addEventListener("blur", function () {
            if (_this.__isChanged) {
                _this.__editor.model.document.differ.reset();
                _this._onChanged();
            }
            _this.__refreshUI();
        });
    };
    HtmlContent.prototype.getValue = function () {
        var data = this.__editor.data.get();
        return data ? data : null;
    };
    HtmlContent.prototype.setValue = function (value) {
        if (this.__editor) {
            this.__editor.data.set(value ? value : "");
            this.__refreshUI();
        }
        else
            this.__value.innerHTML = value ? value : "";
    };
    HtmlContent.prototype.hasValue = function () {
        var value = this.normalizeValue(this.__value.innerText);
        if (!value)
            return false;
        var val = this.__editor.model.hasContent(this.__editor.model.document.getRoot(), { ignoreWhitespaces: true });
        return value && val ? true : false;
    };
    HtmlContent.prototype._onChanged = function () {
        var _this = this;
        this.__refreshUI();
        var value = this.getValue();
        this.form.request(this, {
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: function (response) {
                if (response.status === 200) {
                    _this.setValue(response.data);
                }
                else {
                    _this.setErrors(["error"]);
                }
            }
        });
    };
    HtmlContent.prototype.__refreshUI = function () {
        if (this.hasValue())
            this.element.classList.remove("empty-value");
        else
            this.element.classList.add("empty-value");
    };
    HtmlContent.prototype.normalizeValue = function (value) {
        if (!value)
            return "";
        value = value.trim();
        return value;
    };
    HtmlContent.prototype.destroy = function () {
        var _this = this;
        this.__editor.destroy().then(function () {
            _super.prototype.destroy.call(_this);
        });
        _super.prototype.destroy.call(this);
    };
    return HtmlContent;
}(Field));
export { HtmlContent };
