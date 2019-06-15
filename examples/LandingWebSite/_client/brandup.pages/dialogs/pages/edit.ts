import { DialogOptions, Dialog } from "../dialog";
import { DOM, AjaxQueue, AjaxRequestOptions } from "brandup-ui";
import { IContentForm, IContentField, PageContentForm } from "../../typings/content";
import { TextContent } from "../../content/field/text";
import { HtmlContent } from "../../content/field/html";
import { ImageContent } from "../../content/field/image";
import { ModelField } from "../../content/field/model";
import { HyperLinkContent } from "../../content/field/hyperlink";
import { PagesContent } from "../../content/field/pages";
import "../dialog-form.less";

export class PageEditDialog extends Dialog<any> implements IContentForm {
    private __formElem: HTMLFormElement;
    private navElem: HTMLElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: IContentField } = {};
    private __modelPath: string;
    private __queue: AjaxQueue;
    readonly editId: string;

    constructor(editId: string, modelPath?: string, options?: DialogOptions) {
        super(options);

        this.editId = editId;
        this.__modelPath = modelPath ? modelPath : "";
    }

    get typeName(): string { return "BrandUpPages.PageEditDialog"; }
    get modelPath(): string { return this.__modelPath; }
    get queue(): AjaxQueue { return this.__queue; }

    protected _onRenderContent() {
        this.element.classList.add("bp-dialog-form");

        this.content.appendChild(this.__formElem = <HTMLFormElement>DOM.tag("form", { method: "POST", class: "nopad" }));
        this.__formElem.appendChild(this.__fieldsElem = DOM.tag("div", { class: "fields" }));

        this.__formElem.addEventListener("submit", (e: Event) => {
            e.preventDefault();
            return false;
        });

        this.setHeader("Контент страницы");

        this.__loadForm();

        this.registerCommand("navigate", (elem: HTMLElement) => {
            let path = elem.getAttribute("data-path");
            this.navigate(path);
        });
    }
    private __loadForm() {
        if (this.__queue)
            this.__queue.destroy();
        this.__queue = new AjaxQueue();

        for (let fieldName in this.__fields) {
            let field = this.__fields[fieldName];
            field.destroy();
        }

        DOM.empty(this.__fieldsElem);
        this.__fields = {};

        this.setLoading(true);

        this.__queue.request({
            url: "/brandup.pages/page/content/form",
            urlParams: { editId: this.editId, modelPath: this.__modelPath },
            method: "GET",
            success: (data: PageContentForm, status: number) => {
                if (status !== 200) {
                    this.setError("Не удалось загрузить форму.");
                    return;
                }

                this.__renderForm(data);

                this.setLoading(false);
            }
        });
    }
    private __renderForm(model: PageContentForm) {
        if (!this.navElem) {
            this.navElem = DOM.tag("ol", { class: "nav" });
            this.content.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }

        var path = model.path;
        while (path) {
            let title = path.title;
            if (path.index >= 0)
                title = `#${path.index + 1} ${title}`;

            this.navElem.insertAdjacentElement("afterbegin", DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "navigate", "data-path": path.modelPath }, title)));

            path = path.parent;
        }

        for (let i = 0; i < model.fields.length; i++) {
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

    navigate(modelPath: string) {
        this.__modelPath = modelPath ? modelPath : "";

        this.__loadForm();
    }
    request(field: IContentField, options: AjaxRequestOptions) {
        if (!options.urlParams)
            options.urlParams = {};

        options.urlParams["editId"] = this.editId;
        options.urlParams["path"] = this.modelPath;
        options.urlParams["field"] = field.name;

        this.__queue.request(options);
    }

    validate(): boolean {
        return true;
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

    protected _onClose() {
        this.resolve(null);
    }

    destroy() {
        this.__queue.destroy();

        for (let fieldName in this.__fields) {
            let field = this.__fields[fieldName];
            field.destroy();
        }

        super.destroy();
    }
}

export var editPage = (editId: string, modelPath?: string) => {
    let dialog = new PageEditDialog(editId, modelPath);
    return dialog.open();
};