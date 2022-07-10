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
import { Dialog } from "./dialog";
import { DOM, AjaxQueue } from "brandup-ui";
import { Textbox } from "../form/textbox";
import { ComboBoxField } from "../form/combobox";
import { StringArrayField } from "../form/string-array";
import "./dialog-form.less";
var FormDialog = /** @class */ (function (_super) {
    __extends(FormDialog, _super);
    function FormDialog(options) {
        var _this = _super.call(this, options) || this;
        _this.__fields = {};
        _this.__model = null;
        _this.queue = new AjaxQueue();
        return _this;
    }
    Object.defineProperty(FormDialog.prototype, "model", {
        get: function () { return this.__model; },
        enumerable: false,
        configurable: true
    });
    FormDialog.prototype._onRenderContent = function () {
        var _this = this;
        this.element.classList.add("bp-dialog-form");
        this.content.appendChild(this.__formElem = DOM.tag("form", { method: "POST" }));
        this.__formElem.appendChild(this.__fieldsElem = DOM.tag("div", { class: "fields" }));
        this.__formElem.addEventListener("submit", function (e) {
            e.preventDefault();
            _this.__save();
            return false;
        });
        this.__formElem.addEventListener("changed", function (e) {
            _this.__changeValue(e.detail.field);
        });
        this.registerCommand("save", function () { _this.__save(); });
        this.__loadForm();
    };
    FormDialog.prototype.__loadForm = function () {
        var _this = this;
        var urlParams = {};
        this._buildUrlParams(urlParams);
        this.setLoading(true);
        this.queue.push({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "GET",
            success: function (response) {
                _this.setLoading(false);
                switch (response.status) {
                    case 400: {
                        _this.__applyModelState(response.data);
                        break;
                    }
                    case 200: {
                        _this.__model = response.data;
                        _this._buildForm(_this.__model);
                        _this.setValues(_this.__model.values);
                        _this.addAction("close", "Отмена", false);
                        _this.addAction("save", _this._getSaveButtonTitle(), true);
                        break;
                    }
                    default:
                        throw "";
                }
            }
        });
    };
    FormDialog.prototype.__changeValue = function (field) {
        //var urlParams: { [key: string]: string; } = {
        //    field: field.name
        //};
        //this._buildUrlParams(urlParams);
        //this.queue.request({
        //    url: this._buildUrl(),
        //    urlParams: urlParams,
        //    method: "PUT",
        //    type: "JSON",
        //    data: this.getValues(),
        //    success: (data: any, status: number) => {
        //        switch (status) {
        //            case 400: {
        //                this.__applyModelState(<ValidationProblemDetails>data);
        //                break;
        //            }
        //            case 200: {
        //                break;
        //            }
        //            default:
        //                throw "";
        //        }
        //    }
        //});
    };
    FormDialog.prototype.__save = function () {
        var _this = this;
        if (!this.__model || !this.validate())
            return;
        var urlParams = {};
        this._buildUrlParams(urlParams);
        this.setLoading(true);
        this.queue.push({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "POST",
            type: "JSON",
            data: this.getValues(),
            success: function (response) {
                _this.setLoading(false);
                switch (response.status) {
                    case 400: {
                        _this.__applyModelState(response.data);
                        break;
                    }
                    case 201:
                    case 200: {
                        _this.resolve(response.data);
                        break;
                    }
                    default:
                        throw "";
                }
            }
        });
    };
    FormDialog.prototype.__applyModelState = function (state) {
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
            this.setError(state.errors[""]);
        }
    };
    FormDialog.prototype.validate = function () {
        return true;
    };
    FormDialog.prototype.getValues = function () {
        var values = {};
        for (var key in this.__fields) {
            var field = this.__fields[key];
            values[key] = field.getValue();
        }
        return values;
    };
    FormDialog.prototype.setValues = function (values) {
        for (var key in values) {
            var field = this.getField(key);
            field.setValue(values[key]);
        }
    };
    FormDialog.prototype.getField = function (name) {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw "Field \"".concat(name, "\" not exists.");
        return this.__fields[name.toLowerCase()];
    };
    FormDialog.prototype.addField = function (title, field) {
        if (this.__fields.hasOwnProperty(field.name.toLowerCase()))
            throw "Field name \"".concat(field.name, "\" already exists.");
        var containerElem = DOM.tag("div", { class: "field" });
        if (title)
            containerElem.appendChild(DOM.tag("label", { for: field.name }, title));
        field.render(containerElem);
        this.__fieldsElem.appendChild(containerElem);
        this.__fields[field.name.toLowerCase()] = field;
    };
    FormDialog.prototype.addTextBox = function (name, title, options) {
        var field = new Textbox(name, options);
        this.addField(title, field);
    };
    FormDialog.prototype.addComboBox = function (name, title, options, items) {
        var field = new ComboBoxField(name, options);
        this.addField(title, field);
        field.addItems(items);
    };
    FormDialog.prototype.addStringArray = function (name, title, options) {
        var field = new StringArrayField(name, options);
        this.addField(title, field);
    };
    FormDialog.prototype._buildUrlParams = function (urlParams) { };
    FormDialog.prototype.destroy = function () {
        this.queue.destroy();
        _super.prototype.destroy.call(this);
    };
    return FormDialog;
}(Dialog));
export { FormDialog };
