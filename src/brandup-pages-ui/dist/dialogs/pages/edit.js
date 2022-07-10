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
import { Dialog } from "../dialog";
import { DOM, AjaxQueue } from "brandup-ui";
import { TextContent } from "../../content/field/text";
import { HtmlContent } from "../../content/field/html";
import { ImageContent } from "../../content/field/image";
import { ModelField } from "../../content/field/model";
import { HyperLinkContent } from "../../content/field/hyperlink";
import { PagesContent } from "../../content/field/pages";
import "../dialog-form.less";
var PageEditDialog = /** @class */ (function (_super) {
    __extends(PageEditDialog, _super);
    function PageEditDialog(editId, modelPath, options) {
        var _this = _super.call(this, options) || this;
        _this.__fields = {};
        _this.editId = editId;
        _this.__modelPath = modelPath ? modelPath : "";
        return _this;
    }
    Object.defineProperty(PageEditDialog.prototype, "typeName", {
        get: function () { return "BrandUpPages.PageEditDialog"; },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(PageEditDialog.prototype, "modelPath", {
        get: function () { return this.__modelPath; },
        enumerable: false,
        configurable: true
    });
    Object.defineProperty(PageEditDialog.prototype, "queue", {
        get: function () { return this.__queue; },
        enumerable: false,
        configurable: true
    });
    PageEditDialog.prototype._onRenderContent = function () {
        var _this = this;
        this.element.classList.add("bp-dialog-form");
        this.content.appendChild(this.__formElem = DOM.tag("form", { method: "POST", class: "nopad" }));
        this.__formElem.appendChild(this.__fieldsElem = DOM.tag("div", { class: "fields" }));
        this.__formElem.addEventListener("submit", function (e) {
            e.preventDefault();
            return false;
        });
        this.setHeader("Контент страницы");
        this.__loadForm();
        this.registerCommand("navigate", function (elem) {
            var path = elem.getAttribute("data-path");
            _this.navigate(path);
        });
    };
    PageEditDialog.prototype.__loadForm = function () {
        var _this = this;
        if (this.__queue)
            this.__queue.destroy();
        this.__queue = new AjaxQueue();
        for (var fieldName in this.__fields) {
            var field = this.__fields[fieldName];
            field.destroy();
        }
        DOM.empty(this.__fieldsElem);
        this.__fields = {};
        this.setLoading(true);
        this.__queue.push({
            url: "/brandup.pages/page/content/form",
            urlParams: { editId: this.editId, modelPath: this.__modelPath },
            method: "GET",
            success: function (response) {
                if (response.status !== 200) {
                    _this.setError("Не удалось загрузить форму.");
                    return;
                }
                _this.__renderForm(response.data);
                _this.setLoading(false);
            }
        });
    };
    PageEditDialog.prototype.__renderForm = function (model) {
        if (!this.navElem) {
            this.navElem = DOM.tag("ol", { class: "nav" });
            this.content.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }
        var path = model.path;
        while (path) {
            var title = path.title;
            if (path.index >= 0)
                title = "#".concat(path.index + 1, " ").concat(title);
            this.navElem.insertAdjacentElement("afterbegin", DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "navigate", "data-path": path.modelPath }, title)));
            path = path.parent;
        }
        for (var i = 0; i < model.fields.length; i++) {
            var fieldModel = model.fields[i];
            switch (fieldModel.type.toLowerCase()) {
                case "text": {
                    this.addField(fieldModel.title, new TextContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "html": {
                    this.addField(fieldModel.title, new HtmlContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "image": {
                    this.addField(fieldModel.title, new ImageContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "model": {
                    this.addField(fieldModel.title, new ModelField(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "hyperlink": {
                    this.addField(fieldModel.title, new HyperLinkContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "pages": {
                    this.addField(fieldModel.title, new PagesContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                default: {
                    throw "";
                }
            }
        }
        this.setValues(model.values);
    };
    PageEditDialog.prototype.__applyModelState = function (state) {
        for (var key in this.__fields) {
            var field = this.__fields[key];
            field.setErrors(null);
        }
        if (state && state.errors) {
            for (var key in state.errors) {
                if (key === "")
                    continue;
                var field = this.getField(key);
                field.setErrors(state.errors[key]);
            }
        }
        if (state && state.errors && state.errors.hasOwnProperty("")) {
            alert(state.errors[""]);
        }
    };
    PageEditDialog.prototype.navigate = function (modelPath) {
        this.__modelPath = modelPath ? modelPath : "";
        this.__loadForm();
    };
    PageEditDialog.prototype.request = function (field, options) {
        if (!options.urlParams)
            options.urlParams = {};
        options.urlParams["editId"] = this.editId;
        options.urlParams["path"] = this.modelPath;
        options.urlParams["field"] = field.name;
        this.__queue.push(options);
    };
    PageEditDialog.prototype.validate = function () {
        return true;
    };
    PageEditDialog.prototype.setValues = function (values) {
        for (var key in values) {
            var field = this.getField(key);
            field.setValue(values[key]);
        }
    };
    PageEditDialog.prototype.getField = function (name) {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw "Field \"".concat(name, "\" not exists.");
        return this.__fields[name.toLowerCase()];
    };
    PageEditDialog.prototype.addField = function (title, field) {
        if (this.__fields.hasOwnProperty(field.name.toLowerCase()))
            throw "Field name \"".concat(field.name, "\" already exists.");
        var containerElem = DOM.tag("div", { class: "field" });
        if (title)
            containerElem.appendChild(DOM.tag("label", { for: field.name }, title));
        field.render(containerElem);
        this.__fieldsElem.appendChild(containerElem);
        this.__fields[field.name.toLowerCase()] = field;
    };
    PageEditDialog.prototype._onClose = function () {
        this.resolve(null);
    };
    PageEditDialog.prototype.destroy = function () {
        this.__queue.destroy();
        for (var fieldName in this.__fields) {
            var field = this.__fields[fieldName];
            field.destroy();
        }
        _super.prototype.destroy.call(this);
    };
    return PageEditDialog;
}(Dialog));
export { PageEditDialog };
export var editPage = function (editId, modelPath) {
    var dialog = new PageEditDialog(editId, modelPath);
    return dialog.open();
};
