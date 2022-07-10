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
import "./image.less";
var ImageContent = /** @class */ (function (_super) {
    __extends(ImageContent, _super);
    function ImageContent(form, name, options) {
        var _this = _super.call(this, name, options) || this;
        _this.__isChanged = false;
        _this.__value = null;
        _this.form = form;
        return _this;
    }
    Object.defineProperty(ImageContent.prototype, "typeName", {
        get: function () { return "BrandUpPages.Form.Field.Image"; },
        enumerable: false,
        configurable: true
    });
    ImageContent.prototype._onRender = function () {
        var _this = this;
        _super.prototype._onRender.call(this);
        this.element.classList.add("image");
        this.valueElem = DOM.tag("div", { class: "value", "tabindex": 0 }, [
            DOM.tag("span", null, "Выберите или перетащите суда файл с изображением"),
            DOM.tag("button", { "data-command": "select-file" }, [
                "Выбрать файл"
            ]),
            this.__fileInputElem = DOM.tag("input", { type: "file" })
        ]);
        this.element.appendChild(this.valueElem);
        this.__fileInputElem.addEventListener("change", function () {
            if (_this.__fileInputElem.files.length === 0)
                return;
            _this.__uploadFile(_this.__fileInputElem.files.item(0));
            _this.valueElem.focus();
        });
        var dragleaveTime = 0;
        this.valueElem.ondragover = function () {
            clearTimeout(dragleaveTime);
            _this.valueElem.classList.add("draging");
            return false;
        };
        this.valueElem.ondragleave = function () {
            dragleaveTime = window.setTimeout(function () { _this.valueElem.classList.remove("draging"); }, 50);
            return false;
        };
        this.valueElem.ondrop = function (e) {
            e.stopPropagation();
            e.preventDefault();
            _this.valueElem.classList.remove("draging");
            var file = e.dataTransfer.files.item(0);
            if (!file.type)
                return false;
            _this.__uploadFile(file);
            if (e.dataTransfer.items)
                e.dataTransfer.items.clear();
            else
                e.dataTransfer.clearData();
            _this.valueElem.focus();
            return false;
        };
        this.valueElem.addEventListener("paste", function (e) {
            e.preventDefault();
            if (e.clipboardData.files.length > 0) {
                _this.__uploadFile(e.clipboardData.files.item(0));
            }
            else if (e.clipboardData.types.indexOf("text/plain") >= 0) {
                var url = e.clipboardData.getData("text");
                if (url && url.toLocaleLowerCase().startsWith("http"))
                    _this.__uploadFile(url);
            }
            _this.valueElem.blur();
        });
        this.registerCommand("select-file", function () {
            _this.__fileInputElem.click();
        });
    };
    ImageContent.prototype.__uploadFile = function (file) {
        var _this = this;
        this.valueElem.classList.add("uploading");
        if (file instanceof File) {
            this.form.request(this, {
                url: "/brandup.pages/content/image",
                urlParams: { fileName: file.name },
                method: "POST",
                data: file,
                success: function (response) {
                    switch (response.status) {
                        case 200:
                            _this.setValue(response.data);
                            _this.valueElem.classList.remove("uploading");
                            break;
                        default:
                            throw "";
                    }
                }
            });
            return;
        }
        else if (typeof file === "string") {
            this.form.request(this, {
                url: "/brandup.pages/content/image/url",
                urlParams: { url: file },
                method: "POST",
                success: function (response) {
                    switch (response.status) {
                        case 200:
                            _this.setValue(response.data);
                            _this.valueElem.classList.remove("uploading");
                            break;
                        default:
                            throw "";
                    }
                }
            });
        }
        else {
            this.valueElem.classList.remove("uploading");
        }
    };
    ImageContent.prototype.__refreshValueUI = function () {
        this.valueElem.style.backgroundImage = this.__value ? "url(" + this.__value.previewUrl + ")" : "none";
        if (this.__value)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    };
    ImageContent.prototype.getValue = function () {
        return this.__value;
    };
    ImageContent.prototype.setValue = function (value) {
        this.__value = value;
        this.__refreshValueUI();
    };
    ImageContent.prototype.hasValue = function () {
        return this.__value ? true : false;
    };
    return ImageContent;
}(Field));
export { ImageContent };
