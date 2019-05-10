import { DialogOptions, Dialog } from "./dialog";
import { DOM, AjaxQueue } from "brandup-ui";
import { Field } from "../form/field";
import { TextField } from "../form/textbox";
import { ImageField } from "../form/image";

export class PageEditDialog extends Dialog<any> {
    private __formElem: HTMLFormElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: Field<any, any> } = {};
    readonly queue: AjaxQueue;
    readonly contentPath: string;

    constructor(contentPath?: string, options?: DialogOptions) {
        super(options);

        this.contentPath = contentPath ? contentPath : "";
        this.queue = new AjaxQueue();
    }

    get typeName(): string { return "BrandUpPages.PageEditDialog"; }

    protected _onRenderContent() {
        this.element.classList.add("website-dialog-form");

        this.content.appendChild(this.__formElem = <HTMLFormElement>DOM.tag("form", { method: "POST" }));
        this.__formElem.appendChild(this.__fieldsElem = DOM.tag("div", { class: "fields" }));

        this.__formElem.addEventListener("submit", (e: Event) => {
            e.preventDefault();
            return false;
        });
        this.__formElem.addEventListener("changed", (e: CustomEvent) => {
            this.__onChangeField(e.detail.field);
        });

        this.setHeader("Контент страницы");

        this.queue.request({
            urlParams: { handler: "FormModel", contentPath: this.contentPath },
            method: "GET",
            success: (data: PageContentForm, status: number) => {
                if (status !== 200) {
                    this.setError("Не удалось загрузить форму.");
                    return;
                }

                for (let i = 0; i < data.fields.length; i++) {
                    var fieldModel = data.fields[i];

                    switch (fieldModel.type) {
                        case "Text": {
                            this.addField(fieldModel.title, new TextField(fieldModel.name, fieldModel.options));
                            break;
                        }
                        case "Html": {
                            this.addField(fieldModel.title, new TextField(fieldModel.name, fieldModel.options));
                            break;
                        }
                        case "Image": {
                            this.addField(fieldModel.title, new ImageField(fieldModel.name, fieldModel.options));
                            break;
                        }
                        default: {
                            throw "";
                        }
                    }
                }

                this.setValues(data.values);
            }
        });
    }
    private __onChangeField(field: Field<any, any>) {
        this.queue.request({
            urlParams: { handler: "ChangeValue", contentPath: this.contentPath, fieldName: field.name },
            method: "POST",
            type: "JSON",
            data: field.getValue(),
            success: (data: ContentFieldChangeResult, status: number) => {
                if (status !== 200) {
                    this.setError("Не удалось изменить значение поля.");
                    return;
                }

                field.setValue(data.value);
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
    setValues(values: { [key: string]: any }) {
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

    _onClose() {
        this.resolve(null);

        super._onClose();
    }
    destroy() {
        this.queue.destroy();

        super.destroy();
    }
}

interface PageContentForm {
    path: string;
    fields: Array<ContentFieldModel>;
    values: { [key: string]: any };
}

interface ContentFieldModel {
    type: string;
    name: string;
    title: string;
    options: any;
    value: any;
}

interface ContentFieldChangeResult {
    value: any;
}

export var editPage = (contentPath?: string) => {
    let dialog = new PageEditDialog(contentPath);
    return dialog.open();
};