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
import ContentEditor from "brandup-pages-ckeditor";
import "./html.less";
var HtmlDesigner = /** @class */ (function (_super) {
    __extends(HtmlDesigner, _super);
    function HtmlDesigner() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Object.defineProperty(HtmlDesigner.prototype, "typeName", {
        get: function () { return "BrandUpPages.HtmlDesigner"; },
        enumerable: false,
        configurable: true
    });
    HtmlDesigner.prototype.onRender = function (elem) {
        var _this = this;
        elem.classList.add("html-designer");
        if (this.options.placeholder)
            elem.setAttribute("data-placeholder", this.options.placeholder);
        ContentEditor.create(elem, { placeholder: this.options.placeholder })
            .then(function (editor) {
            _this.__editor = editor;
            editor.model.document.on('change', function () {
                if (editor.model.document.differ.hasDataChanges())
                    _this.__isChanged = true;
                _this.__refreshUI();
            });
            _this.__refreshUI();
        });
        this.element.addEventListener("focus", function () {
            _this.__isChanged = false;
        });
        this.element.addEventListener("blur", function () {
            if (_this.__isChanged) {
                _this.__editor.model.document.differ.reset();
                _this._onChanged();
            }
            _this.__refreshUI();
        });
    };
    HtmlDesigner.prototype.getValue = function () {
        var data = this.__editor.data.get();
        return data ? data : null;
    };
    HtmlDesigner.prototype.setValue = function (value) {
        this.__editor.data.set(value ? value : "");
        this.__refreshUI();
    };
    HtmlDesigner.prototype.hasValue = function () {
        var value = this.normalizeValue(this.element.innerText);
        if (!value)
            return false;
        var val = this.__editor.model.hasContent(this.__editor.model.document.getRoot(), { ignoreWhitespaces: true });
        return value && val ? true : false;
    };
    HtmlDesigner.prototype._onChanged = function () {
        var _this = this;
        this.__refreshUI();
        var value = this.getValue();
        this.page.queue.push({
            url: '/brandup.pages/content/html',
            urlParams: {
                editId: this.page.editId,
                path: this.path,
                field: this.name
            },
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
    HtmlDesigner.prototype.__refreshUI = function () {
        if (this.hasValue())
            this.element.classList.remove("empty-value");
        else
            this.element.classList.add("empty-value");
    };
    HtmlDesigner.prototype.normalizeValue = function (value) {
        if (!value)
            return "";
        value = value.trim();
        return value;
    };
    HtmlDesigner.prototype.destroy = function () {
        var _this = this;
        this.__editor.destroy().then(function () {
            _super.prototype.destroy.call(_this);
        });
        _super.prototype.destroy.call(this);
    };
    return HtmlDesigner;
}(FieldDesigner));
export { HtmlDesigner };
