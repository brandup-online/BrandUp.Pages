var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
define(["require", "exports", "./dialog", "brandup-ui"], function (require, exports, dialog_1, brandup_ui_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var PageCollectionCreateDialog = /** @class */ (function (_super) {
        __extends(PageCollectionCreateDialog, _super);
        function PageCollectionCreateDialog(pageId, options) {
            var _this = _super.call(this, options) || this;
            _this.__fields = {};
            _this.pageId = pageId;
            return _this;
        }
        Object.defineProperty(PageCollectionCreateDialog.prototype, "typeName", {
            get: function () { return "BrandUpPages.PageCollectionCreateDialog"; },
            enumerable: true,
            configurable: true
        });
        PageCollectionCreateDialog.prototype._onRenderContent = function () {
            var _this = this;
            this.element.classList.add("website-dialog-form");
            this.setHeader("Создание коллекции страниц");
            this.addAction("close", "Отмена", false);
            this.addAction("save", "Создать", true);
            this.content.appendChild(this.__formElem = brandup_ui_1.DOM.tag("form", { method: "POST" }));
            this.__formElem.appendChild(this.__fieldsElem = brandup_ui_1.DOM.tag("div", { class: "fields" }));
            this.__formElem.addEventListener("submit", function () {
            });
            this.registerCommand("save", function () {
                if (!_this.validate())
                    return;
                _this.setLoading(true);
                brandup_ui_1.ajaxRequest({
                    url: "/brandup.pages/collection",
                    urlParams: { pageId: _this.pageId },
                    method: "PUT",
                    type: "JSON",
                    data: _this.getValues(),
                    success: function (data, status) {
                        _this.setLoading(false);
                        switch (status) {
                            case 400: {
                                _this.__applyModelState(data);
                                break;
                            }
                            case 201: {
                                _this.resolve(data);
                                break;
                            }
                            default:
                                throw "";
                        }
                    }
                });
            });
            this.addField("Название", new TextField("Title", { placeholder: "Введите название коллекции" }));
            this.addComboBox2("PageType", "Тип страниц", { placeholder: "Выберите тип страниц" }, null, "/brandup.pages/pageType", function (item) { return { value: item.name, title: item.title }; });
            this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, [{ value: "FirstOld", title: "Сначало старые" }, { value: "FirstNew", title: "Сначало новые" }], "FirstOld");
        };
        PageCollectionCreateDialog.prototype.__applyModelState = function (state) {
            for (var key in this.__fields) {
                var field = this.__fields[key];
                if (state.errors && state.errors.hasOwnProperty(key)) {
                    field.setErrors(state.errors[key]);
                }
                else
                    field.setErrors(null);
            }
            if (state.errors && state.errors.hasOwnProperty("")) {
                alert(state.errors[""]);
            }
        };
        PageCollectionCreateDialog.prototype.validate = function () {
            return true;
        };
        PageCollectionCreateDialog.prototype.getValues = function () {
            var values = {};
            for (var key in this.__fields) {
                var field = this.__fields[key];
                values[key] = field.getValue();
            }
            return values;
        };
        PageCollectionCreateDialog.prototype.getField = function (name) {
            if (!this.__fields.hasOwnProperty(name))
                throw "Field \"" + name + "\" not exists.";
            return this.__fields[name];
        };
        PageCollectionCreateDialog.prototype.addField = function (title, field) {
            if (this.__fields.hasOwnProperty(field.name))
                throw "Field name \"" + field.name + "\" already exists.";
            var containerElem = brandup_ui_1.DOM.tag("div", { class: "form-field" });
            if (title)
                containerElem.appendChild(brandup_ui_1.DOM.tag("label", { for: field.name }, title));
            field.render(containerElem);
            this.__fieldsElem.appendChild(containerElem);
            this.__fields[field.name] = field;
        };
        PageCollectionCreateDialog.prototype.addComboBox = function (name, title, options, items, value) {
            var field = new ComboBoxField(name, options);
            this.addField(title, field);
            field.addItems(items);
            field.setValue(value);
        };
        PageCollectionCreateDialog.prototype.addComboBox2 = function (name, title, options, value, url, map) {
            var field = new ComboBoxField(name, options);
            this.addField(title, field);
            brandup_ui_1.ajaxRequest({
                url: url,
                success: function (data, status) {
                    if (status != 200)
                        throw "";
                    for (var i = 0; i < data.length; i++)
                        field.addItem(map(data[i]));
                    field.setValue(value);
                }
            });
        };
        return PageCollectionCreateDialog;
    }(dialog_1.Dialog));
    exports.PageCollectionCreateDialog = PageCollectionCreateDialog;
    var FormField = /** @class */ (function (_super) {
        __extends(FormField, _super);
        function FormField(name, options) {
            var _this = _super.call(this, options) || this;
            _this.name = name;
            return _this;
        }
        FormField.prototype._onRender = function () {
            this.element.classList.add("field");
        };
        FormField.prototype.setErrors = function (errors) {
            this.element.classList.remove("has-errors");
            if (this.__errorsElem) {
                this.__errorsElem.remove();
                this.__errorsElem = null;
            }
            if (!errors || errors.length === 0) {
                return;
            }
            this.element.classList.add("has-errors");
            this.__errorsElem = brandup_ui_1.DOM.tag("ul", { class: "field-errors" });
            for (var i = 0; i < errors.length; i++)
                this.__errorsElem.appendChild(brandup_ui_1.DOM.tag("li", null, errors[i]));
            this.element.insertAdjacentElement("afterend", this.__errorsElem);
        };
        return FormField;
    }(brandup_ui_1.UIControl));
    var TextField = /** @class */ (function (_super) {
        __extends(TextField, _super);
        function TextField() {
            return _super !== null && _super.apply(this, arguments) || this;
        }
        Object.defineProperty(TextField.prototype, "typeName", {
            get: function () { return "BrandUpPages.Form.TextField"; },
            enumerable: true,
            configurable: true
        });
        TextField.prototype._onRender = function () {
            var _this = this;
            _super.prototype._onRender.call(this);
            this.element.classList.add("text");
            this.__valueElem = brandup_ui_1.DOM.tag("div", { class: "value", "tabindex": 0, contenteditable: true });
            this.element.appendChild(this.__valueElem);
            var placeholderElem = brandup_ui_1.DOM.tag("div", { class: "placeholder" }, this.options.placeholder);
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
            this.__valueElem.addEventListener("keyup", function (e) {
                _this.__isChanged = true;
            });
            this.__valueElem.addEventListener("keydown", function (e) {
                if (!_this.options.allowMultiline && e.keyCode == 13) {
                    e.preventDefault();
                    return false;
                }
            });
            this.__valueElem.addEventListener("focus", function () {
                _this.__isChanged = false;
                _this.element.classList.add("focused");
            });
            this.__valueElem.addEventListener("blur", function () {
                _this.element.classList.remove("focused");
                if (_this.__isChanged)
                    _this.__onChanged(_this.__valueElem.innerText);
            });
        };
        TextField.prototype.__refreshUI = function () {
            var hasVal = this.hasValue();
            if (hasVal)
                this.element.classList.add("has-value");
            else
                this.element.classList.remove("has-value");
        };
        TextField.prototype.__onChanged = function (value) {
            value = this.normalizeValue(value);
            this.__refreshUI();
        };
        TextField.prototype.getValue = function () {
            var val = this.normalizeValue(this.__valueElem.innerText);
            return val ? val : null;
        };
        TextField.prototype.setValue = function (value) {
            value = this.normalizeValue(value);
            if (value && this.options.allowMultiline) {
                value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
            }
            this.__valueElem.innerHTML = value ? value : "";
            this.__refreshUI();
        };
        TextField.prototype.hasValue = function () {
            var val = this.normalizeValue(this.__valueElem.innerText);
            return val ? true : false;
        };
        TextField.prototype.normalizeValue = function (value) {
            if (!value)
                return "";
            value = value.trim();
            if (!this.options.allowMultiline)
                value = value.replace("\n\r", " ");
            return value;
        };
        return TextField;
    }(FormField));
    var ComboBoxField = /** @class */ (function (_super) {
        __extends(ComboBoxField, _super);
        function ComboBoxField() {
            var _this = _super !== null && _super.apply(this, arguments) || this;
            _this.__value = null;
            return _this;
        }
        Object.defineProperty(ComboBoxField.prototype, "typeName", {
            get: function () { return "BrandUpPages.Form.ComboBoxField"; },
            enumerable: true,
            configurable: true
        });
        ComboBoxField.prototype._onRender = function () {
            var _this = this;
            _super.prototype._onRender.call(this);
            this.element.classList.add("combobox");
            this.element.setAttribute("tabindex", "0");
            this.__valueElem = brandup_ui_1.DOM.tag("div", { class: "value" });
            this.element.appendChild(this.__valueElem);
            var placeholderElem = brandup_ui_1.DOM.tag("div", { class: "placeholder", "data-command": "toggle" }, this.options.placeholder);
            this.element.appendChild(placeholderElem);
            this.__itemsElem = brandup_ui_1.DOM.tag("ul");
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
                brandup_ui_1.DOM.removeClass(_this.__itemsElem, ".selected", "selected");
                elem.classList.add("selected");
                _this.__value = elem.getAttribute("data-value");
                _this.__valueElem.innerText = elem.innerText;
                _this.__refreshUI();
                _this.element.blur();
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
            this.__itemsElem.appendChild(brandup_ui_1.DOM.tag("li", { "data-value": item.value, "data-command": "select" }, item.title));
        };
        ComboBoxField.prototype.addItems = function (items) {
            for (var i = 0; i < items.length; i++)
                this.addItem(items[i]);
        };
        ComboBoxField.prototype.clearItems = function () {
            brandup_ui_1.DOM.empty(this.__valueElem);
            this.__value = null;
        };
        ComboBoxField.prototype.getValue = function () {
            return this.__value;
        };
        ComboBoxField.prototype.setValue = function (value) {
            var text = "";
            if (value) {
                var itemElem = brandup_ui_1.DOM.queryElement(this.__itemsElem, "li[data-value=" + value + "]");
                if (!itemElem) {
                    this.setValue(null);
                    return;
                }
                text = itemElem.innerText;
                itemElem.classList.add("selected");
            }
            else
                brandup_ui_1.DOM.removeClass(this.__itemsElem, ".selected", "selected");
            this.__value = value;
            this.__valueElem.innerText = text;
            this.__refreshUI();
        };
        ComboBoxField.prototype.hasValue = function () {
            var val = this.__value;
            return val ? true : false;
        };
        return ComboBoxField;
    }(FormField));
    exports.createPageCollection = function (pageId) {
        var dialog = new PageCollectionCreateDialog(pageId);
        return dialog.open();
    };
});
//# sourceMappingURL=page-collection-create.js.map