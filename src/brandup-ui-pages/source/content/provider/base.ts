import { ContentFieldModel, IContentField, IContentFieldDesigner, IContentFieldProvider } from "../../typings/content";
import { AjaxRequest } from "brandup-ui-ajax";
import { Content } from "../../content/content";

export abstract class FieldProvider<TValue, TOptions> implements IContentFieldProvider {
    protected __model: ContentFieldModel<TValue, TOptions>;
    designer: IContentFieldDesigner;
    field: IContentField;
    protected __valueElem: HTMLElement;
    readonly content: Content;

    constructor(content: Content, model: ContentFieldModel<TValue, TOptions>, valueElem: HTMLElement = null) {
        this.__model = model;
        this.__valueElem = valueElem;
        this.content = content;
    }
    
    renderDesigner() {
        this.designer = this.createDesigner();
    }

    getValue() {
        return this.__model.value;
    }

    setValue(value) {
        this.__model.value = value;
        this.designer?.setValue(value);
        this.field?.setValue(value);
    }

    setErrors(errors: Array<string>) {
        this.designer?.setValid(errors.length === 0);
        this.field?.setErrors(errors);
    }

    protected request(options: AjaxRequest) {
        if (!options.urlParams)
            options.urlParams = {};

        options.urlParams["editId"] = this.content.__editor.editId;
        options.urlParams["path"] = this.__valueElem.dataset.contentFieldPath;
        options.urlParams["field"] = this.__model.name;

        this.content.__editor.queue.push(options);
    }

    abstract createDesigner(): IContentFieldDesigner;
    // abstract createField(): IContentField;

    destroy() {
        this.designer?.destroy();
        this.field?.destroy();
    }
}