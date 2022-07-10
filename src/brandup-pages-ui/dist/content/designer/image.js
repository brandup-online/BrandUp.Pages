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
import "./image.less";
import iconUpload from "../../svg/toolbar-button-picture.svg";
var ImageDesigner = /** @class */ (function (_super) {
    __extends(ImageDesigner, _super);
    function ImageDesigner() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Object.defineProperty(ImageDesigner.prototype, "typeName", {
        get: function () { return "BrandUpPages.ImageDesigner"; },
        enumerable: false,
        configurable: true
    });
    ImageDesigner.prototype.onRender = function (elem) {
        var _this = this;
        elem.classList.add("image-designer");
        var textInput;
        elem.appendChild(DOM.tag("div", { class: "bp-elem image-designer-menu" }, [
            DOM.tag("ul", { class: "field-designer-popup" }, [
                //DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "open-editor" }, "Редактор")),
                //DOM.tag("li", { class: "split" }),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "upload-file" }, "Загрузить с компьютера"))
            ]),
            this.__button = DOM.tag("button", { title: "Управление картинкой" }, iconUpload),
            textInput = DOM.tag("input", { type: "text" })
        ]));
        elem.appendChild(DOM.tag("div", { class: "bp-elem image-designer-progress" }));
        this.__fileInputElem = DOM.tag("input", { type: "file" });
        this.__fileInputElem.addEventListener("change", function () {
            if (_this.__fileInputElem.files.length === 0)
                return;
            _this.__uploadFile(_this.__fileInputElem.files.item(0));
            textInput.focus();
        });
        textInput.addEventListener("paste", function (e) {
            e.preventDefault();
            if (e.clipboardData.files.length > 0) {
                _this.__uploadFile(e.clipboardData.files.item(0));
            }
            else if (e.clipboardData.types.indexOf("text/plain") >= 0) {
                var url = e.clipboardData.getData("text");
                if (url && url.toLocaleLowerCase().startsWith("http"))
                    _this.__uploadFile(url);
            }
            textInput.focus();
            elem.classList.remove("opened-menu");
            document.body.removeEventListener("click", _this.__closeFunc, false);
        });
        this.__closeFunc = function (e) {
            var t = e.target;
            if (!t.closest(".image-designer-menu"))
                elem.classList.remove("opened-menu");
        };
        this.__button.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            if (!elem.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", _this.__closeFunc, false);
                return;
            }
            document.body.addEventListener("click", _this.__closeFunc, false);
            textInput.focus();
        });
        this.registerCommand("upload-file", function () {
            elem.classList.remove("opened-menu");
            document.body.removeEventListener("click", _this.__closeFunc, false);
            _this.__fileInputElem.click();
        });
        var dragleaveTime = 0;
        elem.ondragover = function () {
            clearTimeout(dragleaveTime);
            elem.classList.add("draging");
            return false;
        };
        elem.ondragleave = function () {
            dragleaveTime = window.setTimeout(function () { elem.classList.remove("draging"); }, 50);
            return false;
        };
        elem.ondrop = function (e) {
            e.stopPropagation();
            e.preventDefault();
            elem.classList.remove("draging");
            var file = e.dataTransfer.files.item(0);
            if (!file.type)
                return false;
            _this.__uploadFile(file);
            if (e.dataTransfer.items)
                e.dataTransfer.items.clear();
            else
                e.dataTransfer.clearData();
            textInput.focus();
            return false;
        };
    };
    ImageDesigner.prototype.hasValue = function () {
        return false;
    };
    ImageDesigner.prototype.__uploadFile = function (file) {
        var _this = this;
        this.element.classList.add("uploading");
        var width = this.element.getAttribute("content-image-width");
        var height = this.element.getAttribute("content-image-height");
        if (file instanceof File) {
            this.request({
                url: "/brandup.pages/content/image",
                urlParams: {
                    fileName: file.name,
                    width: width,
                    height: height
                },
                method: "POST",
                data: file,
                success: function (response) {
                    switch (response.status) {
                        case 200:
                            _this.element.style.backgroundImage = "url(".concat(response.data, ")");
                            _this.element.classList.remove("uploading");
                            break;
                        default:
                            throw "";
                    }
                }
            });
            return;
        }
        else if (typeof file === "string") {
            this.request({
                url: "/brandup.pages/content/image/url",
                urlParams: {
                    url: file,
                    width: width,
                    height: height
                },
                method: "POST",
                success: function (response) {
                    switch (response.status) {
                        case 200:
                            _this.element.style.backgroundImage = "url(".concat(response.data, ")");
                            break;
                    }
                    _this.element.classList.remove("uploading");
                }
            });
        }
        else {
            this.element.classList.remove("uploading");
        }
    };
    return ImageDesigner;
}(FieldDesigner));
export { ImageDesigner };
