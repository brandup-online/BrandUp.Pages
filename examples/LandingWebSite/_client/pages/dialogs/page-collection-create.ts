import { Dialog, DialogOptions } from "./dialog";
import { DOM, UIControl, ajaxRequest } from "brandup-ui";

export class PageCollectionCreateDialog extends Dialog<PageCollectionModel> {
    readonly pageId: string;
    private __formElem: HTMLFormElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: FormField<any, any> } = {};

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionCreateDialog"; }

    protected _onRenderContent() {
        this.element.classList.add("website-dialog-form");

        this.setHeader("Создание коллекции страниц");
        this.addAction("close", "Отмена", false);
        this.addAction("save", "Создать", true);

        this.content.appendChild(this.__formElem = <HTMLFormElement>DOM.tag("form", { method: "POST" }));
        this.__formElem.appendChild(this.__fieldsElem = DOM.tag("div", { class: "fields" }));

        this.__formElem.addEventListener("submit", () => {

        });

        this.registerCommand("save", () => {
            if (!this.validate())
                return;

            this.setLoading(true);

            ajaxRequest({
                url: "/brandup.pages/collection",
                urlParams: { pageId: this.pageId },
                method: "PUT",
                type: "JSON",
                data: this.getValues(),
                success: (data: any, status: number) => {
                    this.setLoading(false);
                    
                    switch (status) {
                        case 400: {
                            this.__applyModelState(data);
                            break;
                        }
                        case 201: {
                            this.resolve(data);
                            break;
                        }
                        default:
                            throw "";
                    }
                }
            });
        });

        this.addField("Название", new TextField("Title", { placeholder: "Введите название коллекции" }));

        this.addComboBox2<PageTypeModel>("PageType", "Тип страниц", { placeholder: "Выберите тип страниц" }, null, "/brandup.pages/pageType", (item) => { return { value: item.name, title: item.title } });

        this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, [{ value: "FirstOld", title: "Сначало старые" }, { value: "FirstNew", title: "Сначало новые" }], "FirstOld");
    }

    private __applyModelState(state: FormModelState) {
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

    protected getField(name: string): FormField<any, any> {
        if (!this.__fields.hasOwnProperty(name))
            throw `Field "${name}" not exists.`;
        return this.__fields[name];
    }
    protected addField(title: string, field: FormField<any, any>) {
        if (this.__fields.hasOwnProperty(field.name))
            throw `Field name "${field.name}" already exists.`;

        var containerElem = DOM.tag("div", { class: "form-field" });

        if (title)
            containerElem.appendChild(DOM.tag("label", { for: field.name }, title));

        field.render(containerElem);

        this.__fieldsElem.appendChild(containerElem);

        this.__fields[field.name] = field;
    }
    protected addComboBox(name: string, title: string, options: ComboBoxFieldOptions, items: Array<ComboBoxItem>, value: any) {
        var field = new ComboBoxField(name, options);
        this.addField(title, field);

        field.addItems(items);

        field.setValue(value);
    }
    protected addComboBox2<TItem>(name: string, title: string, options: ComboBoxFieldOptions, value: any, url: string, map: (item: TItem) => ComboBoxItem) {
        var field = new ComboBoxField(name, options);
        this.addField(title, field);

        ajaxRequest({
            url: url,
            success: (data: Array<TItem>, status: number) => {
                if (status != 200)
                    throw "";

                for (let i = 0; i < data.length; i++)
                    field.addItem(map(data[i]));

                field.setValue(value);
            }
        });
    }
}

interface FormModelState {
    errors: { [key: string]: Array<string> };
    title: string;
    status: number;
    traceId: string;
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
        this.__valueElem.addEventListener("keyup", (e: KeyboardEvent) => {
            this.__isChanged = true;
        });
        this.__valueElem.addEventListener("keydown", (e: KeyboardEvent) => {
            if (!this.options.allowMultiline && e.keyCode == 13) {
                e.preventDefault();
                return false;
            }
        });
        this.__valueElem.addEventListener("focus", () => {
            this.__isChanged = false;
            this.element.classList.add("focused");
        });
        this.__valueElem.addEventListener("blur", () => {
            this.element.classList.remove("focused");
            if (this.__isChanged)
                this.__onChanged(this.__valueElem.innerText);
        });
    }

    private __refreshUI() {
        let hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }
    private __onChanged(value: string) {
        value = this.normalizeValue(value);

        this.__refreshUI();
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
        for (var i = 0; i < items.length; i++)
            this.addItem(items[i]);
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
        if (value) {
            var itemElem = DOM.queryElement(this.__itemsElem, `li[data-value=${value}]`);
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
interface ComboBoxItem {
    value: string;
    title: string;
}

export var createPageCollection = (pageId: string) => {
    let dialog = new PageCollectionCreateDialog(pageId);
    return dialog.open();
};