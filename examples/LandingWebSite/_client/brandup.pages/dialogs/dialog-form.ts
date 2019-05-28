import { Dialog, DialogOptions } from "./dialog";
import { DOM, AjaxQueue } from "brandup-ui";
import { Field } from "../form/field";
import { TextboxOptions, Textbox } from "../form/textbox";
import { ComboBoxFieldOptions, ComboBoxItem, ComboBoxField } from "../form/combobox";
import "./dialog-form.less";

export abstract class FormDialog<TForm extends FormModel<TValues>, TValues, TResult> extends Dialog<TResult> {
    private __formElem: HTMLFormElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: Field<any, any> } = {};
    private __model: TForm = null;
    readonly queue: AjaxQueue;

    constructor(options?: DialogOptions) {
        super(options);

        this.queue = new AjaxQueue();
    }

    get model(): TForm { return this.__model; }

    protected _onRenderContent() {
        this.element.classList.add("bp-dialog-form");
        
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
    private __changeValue(field: Field<any, any>) {
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

    protected getField(name: string): Field<any, any> {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw `Field "${name}" not exists.`;
        return this.__fields[name.toLowerCase()];
    }
    protected addField(title: string, field: Field<any, any>) {
        if (this.__fields.hasOwnProperty(field.name.toLowerCase()))
            throw `Field name "${field.name}" already exists.`;

        var containerElem = DOM.tag("div", { class: "field" });

        if (title)
            containerElem.appendChild(DOM.tag("label", { for: field.name }, title));

        field.render(containerElem);

        this.__fieldsElem.appendChild(containerElem);

        this.__fields[field.name.toLowerCase()] = field;
    }
    protected addTextBox(name: string, title: string, options: TextboxOptions) {
        var field = new Textbox(name, options);
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