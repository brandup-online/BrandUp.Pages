import { IContentField, IContentFieldDesigner, IContentFieldProvider } from "../../typings/content";
import { AjaxRequest } from "brandup-ui-ajax";
import { Content } from "../../content/content";
import { ContentFieldModel, FieldValueResult } from "../../typings/models";

export abstract class FieldProvider<TValue, TOptions> implements IContentFieldProvider {
    readonly content: Content;
    readonly name: string;
    readonly title: string;
    readonly options: TOptions;
    readonly isRequired: boolean;
    readonly valueElem: HTMLElement;
    readonly designerType: string;

    private __value: TValue;
    private __errors: Array<string>;

    designer: IContentFieldDesigner;
    field: IContentField;
    
    constructor(content: Content, model: ContentFieldModel, valueElem: HTMLElement) {
        this.content = content;
        this.name = model.name;
        this.title = model.title;
        this.options = model.options;
        this.isRequired = model.isRequired;
        this.valueElem = valueElem;
        this.designerType = this.valueElem.getAttribute("data-content-designer");

        this.__value = model.value;
        this.__errors = model.errors;
    }
    
    renderDesigner() {
        this.designer = this.createDesigner();
    }

    abstract createField();

    destroyField() {
        this.field?.destroy();
    }

    getValue() {
        return this.__value;
    }

    hasValue(): boolean {
        return !!this.__value;
    }

    protected request(options: AjaxRequest) {
        if (!options.urlParams)
            options.urlParams = {};

        options.urlParams["editId"] = this.content.editor.editId;
        options.urlParams["path"] = this.valueElem.dataset.contentFieldPath;
        options.urlParams["field"] = this.name;

        this.content.editor.queue.push(options);
    }

    abstract createDesigner(): IContentFieldDesigner;
    
    protected onSavedValue(model: FieldValueResult) {
        this.__value = model.value;
        this.__errors = model.errors;
    }

    destroy() {
        this.designer?.destroy();
        this.destroyField();
    }
}