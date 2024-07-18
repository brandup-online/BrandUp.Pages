import { Dialog, DialogOptions } from "./dialog";
import { DOM } from "brandup-ui-dom";
import { Field } from "../form/field";
import { TextboxOptions, Textbox } from "../form/textbox";
import { ComboBoxFieldOptions, ComboBoxItem, ComboBoxField } from "../form/combobox";
import { StringArrayFieldOptions, StringArrayField } from "../form/string-array";
import { ValidationProblemDetails } from "../typings/page";
import { AjaxQueue } from "brandup-ui-ajax";
import "./dialog-form.less";

export abstract class FormDialog<TForm extends FormModel<TValues>, TValues, TResult> extends Dialog<TResult> {
    private __formElem: HTMLFormElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: Field<any, any> } = {};
    private __model: TForm | null = null;
    readonly queue: AjaxQueue;

    constructor(options?: DialogOptions) {
        super(options);

        this.__formElem = DOM.tag("form", { method: "POST" }) as HTMLFormElement;
        this.__fieldsElem = DOM.tag("div", { class: "fields" });

        this.queue = new AjaxQueue();
    }

    get model(): TForm | null { return this.__model; }

    protected _onRenderContent() {
        this.element?.classList.add("bp-dialog-form");

        this.content?.appendChild(this.__formElem);
        this.__formElem.appendChild(this.__fieldsElem);

        this.__formElem.addEventListener("submit", (e: Event) => {
            e.preventDefault();

            this.__save();

            return false;
        });

        /* @ts-ignore */
        this.__formElem.addEventListener("changed", (e: CustomEvent) => {
            this.__changeValue(e.detail.field);
        });
        
        this.registerCommand("save", () => { this.__save(); });

        this.__loadForm();
    }

    private __loadForm() {
        const urlParams: { [key: string]: string; } = {};

        this._buildUrlParams(urlParams);

        this.setLoading(true);

        this.queue.push({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "GET",
            success: (response) => {
                this.setLoading(false);

                switch (response.status) {
                    case 400: {
                        this.__applyModelState(response.data);
                        break;
                    }
                    case 200: {
                        if (!response.data) return;
                        this.__model = response.data as TForm;
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

        const urlParams: { [key: string]: string; } = {};

        this._buildUrlParams(urlParams);

        this.setLoading(true);

        this.queue.push({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "POST",
            type: "JSON",
            data: this.getValues(),
            success: (response) => {
                this.setLoading(false);

                switch (response.status) {
                    case 400: {
                        this.__applyModelState(response.data);
                        break;
                    }
                    case 201:
                    case 200: {
                        this.resolve(response.data);
                        break;
                    }
                    default:
                        throw "";
                }
            }
        });
    }
    private __applyModelState(state: ValidationProblemDetails) {
        for (const key in this.__fields) {
            const field = this.__fields[key];
            field.setErrors([]);
        }

        if (state && state.errors) {
            for (const key in state.errors) {
                if (key === "")
                    continue;

                const field = this.getField(key);
                field.setErrors(state.errors[key]);
            }
        }

        if (state && state.errors && state.errors.hasOwnProperty("")) {
            this.setError(state.errors[""]);
        }
    }

    validate(): boolean {
        return true;
    }
    getValues(): { [key: string]: any } {
        const values: { [key: string]: any } = {};

        for (const key in this.__fields) {
            const field = this.__fields[key];

            values[key] = field.getValue();
        }

        return values;
    }
    setValues(values: TValues) {
        for (const key in values) {
            const field = this.getField(key);
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

        const containerElem = DOM.tag("div", { class: "field" });

        if (title)
            containerElem.appendChild(DOM.tag("label", { for: field.name }, title));

        field.render(containerElem);

        this.__fieldsElem.appendChild(containerElem);

        this.__fields[field.name.toLowerCase()] = field;
    }
    protected addTextBox(name: string, title: string, options: TextboxOptions) {
        const field = new Textbox(name, [], options);
        this.addField(title, field);
    }
    protected addComboBox(name: string, title: string, options: ComboBoxFieldOptions, items: Array<ComboBoxItem>) {
        const field = new ComboBoxField(name, [], options);
        this.addField(title, field);

        field.addItems(items);
    }
    protected addStringArray(name: string, title: string, options: StringArrayFieldOptions) {
        const field = new StringArrayField(name, [], options);
        this.addField(title, field);
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