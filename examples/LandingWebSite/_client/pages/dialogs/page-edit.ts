import { DialogOptions, Dialog } from "./dialog";
import { DOM, AjaxQueue } from "brandup-ui";
import { IContentForm, IContentField, PageContentForm, ContentFieldModel } from "../typings/content";
import { TextContent } from "../content/text";
import { HtmlContent } from "../content/html";
import { ImageContent } from "../content/image";
import { ListContent } from "../content/content";

export class PageEditDialog extends Dialog<any> implements IContentForm {
    private __formElem: HTMLFormElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: IContentField } = {};
    readonly queue: AjaxQueue;
    readonly editId: string;
    readonly contentPath: string;

    constructor(editId: string, contentPath?: string, options?: DialogOptions) {
        super(options);

        this.editId = editId;
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
        //this.__formElem.addEventListener("changed", (e: CustomEvent) => {
        //    this.__onChangeField(e.detail.field);
        //});

        this.setHeader("Контент страницы");

        this.__loadForm();
    }
    private __loadForm() {
        this.queue.request({
            urlParams: { handler: "FormModel", contentPath: this.contentPath },
            method: "GET",
            success: (data: PageContentForm, status: number) => {
                if (status !== 200) {
                    this.setError("Не удалось загрузить форму.");
                    return;
                }

                this.__renderForm(data);

                
            }
        });
    }
    private __renderForm(model: PageContentForm) {
        for (let i = 0; i < model.fields.length; i++) {
            var fieldModel = model.fields[i];

            switch (fieldModel.type) {
                case "Text": {
                    this.addField(fieldModel.title, new TextContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "Html": {
                    this.addField(fieldModel.title, new HtmlContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "Image": {
                    this.addField(fieldModel.title, new ImageContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                case "Content": {
                    this.addField(fieldModel.title, new ListContent(this, fieldModel.name, fieldModel.options));
                    break;
                }
                default: {
                    throw "";
                }
            }
        }

        this.setValues(model.values);
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

    getField(name: string): IContentField {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw `Field "${name}" not exists.`;
        return this.__fields[name.toLowerCase()];
    }
    protected addField(title: string, field: IContentField) {
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

export var editPage = (editId: string, contentPath?: string) => {
    let dialog = new PageEditDialog(editId, contentPath);
    return dialog.open();
};