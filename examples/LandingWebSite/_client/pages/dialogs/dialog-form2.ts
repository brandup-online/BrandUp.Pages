import { Dialog, DialogOptions } from "./dialog";
import { UIControl, DOM, ajaxRequest, AjaxQueue } from "brandup-ui";

export abstract class FormDialog<TForm extends FormModel<TValues>, TValues, TResult> extends Dialog<TResult> {
    private __formElem: HTMLFormElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: FormField<any, any> } = {};
    private __model: TForm = null;
    readonly queue: AjaxQueue;

    constructor(options?: DialogOptions) {
        super(options);

        this.queue = new AjaxQueue();
    }

    get model(): TForm { return this.__model; }

    protected _onRenderContent() {
        this.element.classList.add("website-dialog-form");
        
        this.content.appendChild(this.__formElem = <HTMLFormElement>DOM.tag("form", { method: "POST" }));
        this.__formElem.appendChild(this.__fieldsElem = DOM.tag("div", { class: "fields" }));

        this.__formElem.addEventListener("submit", (e: Event) => {
            e.preventDefault();

            this.__save();

            return false;
        });
        this.__formElem.addEventListener("changed", (e: CustomEvent) => {
            this.__changeValue(e.detail.field);
        });

        this.registerCommand("save", () => { this.__save(); });

        this.__loadForm();
    }

    private __loadForm() {
        var urlParams: { [key: string]: string; } = {};

        this._buildUrlParams(urlParams);

        this.setLoading(true);

        this.queue.request({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "GET",
            success: (data: any, status: number) => {
                this.setLoading(false);

                switch (status) {
                    case 400: {
                        this.__applyModelState(<ValidationProblemDetails>data);
                        break;
                    }
                    case 200: {
                        this.__model = <TForm>data;
                        this._buildForm(this.__model);
                        this.setValues(this.__model.values);

                        this.addAction("close", "Отмена", false);
                        this.addAction("save", this._getSaveButtonTitle(), true);

                        break;
                    }
                    default:
                        throw "";
                }
            }
        });
    }
    private __changeValue(field: FormField<any, any>) {
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
    }
    private __save() {
        if (!this.__model || !this.validate())
            return;

        var urlParams: { [key: string]: string; } = {};

        this._buildUrlParams(urlParams);

        this.setLoading(true);

        this.queue.request({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "POST",
            type: "JSON",
            data: this.getValues(),
            success: (data: any, status: number) => {
                this.setLoading(false);

                switch (status) {
                    case 400: {
                        this.__applyModelState(data);
                        break;
                    }
                    case 201:
                    case 200: {
                        this.resolve(data);
                        break;
                    }
                    default:
                        throw "";
                }
            }
        });
    }
    private __applyModelState(state: ValidationProblemDetails) {
        for (let key in this.__fields) {
            let field = this.__fields[key];
            field.setErrors(null);
        }

        if (state && state.errors) {
            for (let key in state.errors) {
                if (key === "")
                    continue;

                let field = this.getField(key);
                field.setErrors(state.errors[key]);
            }
        }
        
        if (state && state.errors && state.errors.hasOwnProperty("")) {
            alert(state.errors[""]);
        }
    }

    validate(): boolean {
        return true;
    }
    getValues(): { [key: string]: any } {
        var values: { [key: string]: any } = {};

        for (var key in this.__fields) {
            var field = this.__fields[key];

            values[key] = field.getValue();
        }

        return values;
    }
    setValues(values: TValues) {
        for (var key in values) {
            var field = this.getField(key);
            field.setValue(values[key]);
        }
    }

    protected getField(name: string): FormField<any, any> {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw `Field "${name}" not exists.`;
        return this.__fields[name.toLowerCase()];
    }
    protected addField(title: string, field: FormField<any, any>) {
        if (this.__fields.hasOwnProperty(field.name.toLowerCase()))
            throw `Field name "${field.name}" already exists.`;

        var containerElem = DOM.tag("div", { class: "form-field" });

        if (title)
            containerElem.appendChild(DOM.tag("label", { for: field.name }, title));

        field.render(containerElem);

        this.__fieldsElem.appendChild(containerElem);

        this.__fields[field.name.toLowerCase()] = field;
    }
    protected addTextBox(name: string, title: string, options: TextFieldOptions) {
        var field = new TextField(name, options);
        this.addField(title, field);
    }
    protected addComboBox(name: string, title: string, options: ComboBoxFieldOptions, items: Array<ComboBoxItem>) {
        var field = new ComboBoxField(name, options);
        this.addField(title, field);

        field.addItems(items);
    }

    protected abstract _getSaveButtonTitle(): string;
    protected abstract _buildUrl(): string;
    protected _buildUrlParams(urlParams: { [key: string]: string; }) { }
    protected abstract _buildForm(model: TForm);

    destroy() {
        this.queue.destroy();

        super.destroy();
    }
}

export interface FormModel<TValues> {
    values: TValues;
}

interface ValidationProblemDetails {
    type: string;
    title: string;
    status: number;
    detail: string;
    traceId: string;
    instance: string;
    errors: { [key: string]: Array<string> };
}

abstract class FormField<TValue, TOptions> extends UIControl<TOptions> {
    readonly name: string;
    private __errorsElem: HTMLElement;

    constructor(name: string, options: TOptions) {
        super(options);

        this.name = name;
    }

    protected _onRender() {
        this.element.classList.add("field");

        this.__createEvent("changed", { bubbles: true, cancelable: false });
    }

    protected raiseChanged() {
        this.__raiseEvent("changed", {
            field: this,
            value: this.getValue()
        });
    }

    abstract getValue(): TValue;
    abstract setValue(value: TValue);
    abstract hasValue(): boolean;

    setErrors(errors: Array<string>) {
        this.element.classList.remove("has-errors");
        if (this.__errorsElem) {
            this.__errorsElem.remove();
            this.__errorsElem = null;
        }

        if (!errors || errors.length === 0) {
            return;
        }

        this.element.classList.add("has-errors");
        this.__errorsElem = DOM.tag("ul", { class: "field-errors" });
        for (var i = 0; i < errors.length; i++)
            this.__errorsElem.appendChild(DOM.tag("li", null, errors[i]));
        this.element.insertAdjacentElement("afterend", this.__errorsElem);
    }
}

class TextField extends FormField<string, TextFieldOptions> {
    private __valueElem: HTMLElement;
    private __isChanged: boolean;

    get typeName(): string { return "BrandUpPages.Form.TextField"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("text");

        this.__valueElem = <HTMLInputElement>DOM.tag("div", { class: "value", "tabindex": 0, contenteditable: true });
        this.element.appendChild(this.__valueElem);

        let placeholderElem = DOM.tag("div", { class: "placeholder" }, this.options.placeholder);
        placeholderElem.addEventListener("click", () => {
            this.__valueElem.focus();
        });
        this.element.appendChild(placeholderElem);

        this.__valueElem.addEventListener("paste", (e: ClipboardEvent) => {
            this.__isChanged = true;

            e.preventDefault();

            var text = e.clipboardData.getData("text/plain");
            document.execCommand("insertText", false, this.normalizeValue(text));
        });
        this.__valueElem.addEventListener("cut", () => {
            this.__isChanged = true;
        });
        this.__valueElem.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode == 13) {
                e.preventDefault();
                return false;
            }
        });
        this.__valueElem.addEventListener("keyup", (e: KeyboardEvent) => {
            this.__isChanged = true;
        });
        this.__valueElem.addEventListener("focus", () => {
            this.__isChanged = false;
            this.element.classList.add("focused");
        });
        this.__valueElem.addEventListener("blur", () => {
            this.element.classList.remove("focused");
            if (this.__isChanged)
                this.__onChanged();
        });
    }

    private __refreshUI() {
        let hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }
    private __onChanged() {
        this.__refreshUI();

        this.raiseChanged();
    }

    getValue(): string {
        var val = this.normalizeValue(this.__valueElem.innerText);
        return val ? val : null;
    }
    setValue(value: string) {
        value = this.normalizeValue(value);
        if (value && this.options.allowMultiline) {
            value = value.replace(/(?:\r\n|\r|\n)/g, "<br />");
        }
        this.__valueElem.innerHTML = value ? value : "";

        this.__refreshUI();
    }
    hasValue(): boolean {
        var val = this.normalizeValue(this.__valueElem.innerText);
        return val ? true : false;
    }

    normalizeValue(value: string): string {
        if (!value)
            return "";

        value = value.trim();

        if (!this.options.allowMultiline)
            value = value.replace("\n\r", " ");

        return value;
    }
}
interface TextFieldOptions {
    placeholder?: string;
    allowMultiline?: boolean;
}

class ComboBoxField extends FormField<string, ComboBoxFieldOptions> {
    private __valueElem: HTMLElement;
    private __itemsElem: HTMLElement;
    private __value: string = null;
    private __isChanged: boolean;

    get typeName(): string { return "BrandUpPages.Form.ComboBoxField"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("combobox");
        this.element.setAttribute("tabindex", "0");

        this.__valueElem = <HTMLInputElement>DOM.tag("div", { class: "value" });
        this.element.appendChild(this.__valueElem);

        let placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "toggle" }, this.options.placeholder);
        this.element.appendChild(placeholderElem);

        this.__itemsElem = <HTMLInputElement>DOM.tag("ul");
        this.element.appendChild(this.__itemsElem);

        var isFocused = false;
        var md = false;
        this.addEventListener("focus", () => {
            isFocused = true;
        });
        this.addEventListener("blur", () => {
            isFocused = false;
        });

        placeholderElem.addEventListener("mousedown", () => {
            md = isFocused;
        });

        placeholderElem.addEventListener("mouseup", () => {
            if (md && isFocused)
                this.element.blur();
        });

        this.registerCommand("select", (elem: HTMLElement) => {
            DOM.removeClass(this.__itemsElem, ".selected", "selected");

            elem.classList.add("selected");

            this.__value = elem.getAttribute("data-value");
            this.__valueElem.innerText = elem.innerText;

            this.__refreshUI();

            this.element.blur();

            this.raiseChanged();
        });
    }

    private __refreshUI() {
        let hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }

    addItem(item: ComboBoxItem) {
        this.__itemsElem.appendChild(DOM.tag("li", { "data-value": item.value, "data-command": "select" }, item.title));
    }
    addItems(items: Array<ComboBoxItem>) {
        if (items) {
            for (var i = 0; i < items.length; i++)
                this.addItem(items[i]);
        }
    }
    clearItems() {
        DOM.empty(this.__valueElem);

        this.__value = null;
    }

    getValue(): string {
        return this.__value;
    }
    setValue(value: string) {
        var text: string = "";
        if (value !== null) {
            var itemElem = DOM.queryElement(this.__itemsElem, `li[data-value="${value}"]`);
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
    }
    hasValue(): boolean {
        var val = this.__value;
        return val ? true : false;
    }
}
interface ComboBoxFieldOptions {
    placeholder?: string;
    emptyText?: string;
}
export interface ComboBoxItem {
    value: string;
    title: string;
}