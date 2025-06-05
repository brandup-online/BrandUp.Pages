import { AjaxRequest } from "@brandup/ui-ajax";
import { Content } from "../../content/content";
import { ContentFieldModel, FieldValueResult } from "../../typings/content";

export abstract class FieldProvider<TValue, TOptions> {
    readonly content: Content;
    readonly name: string;
    readonly title: string;
    readonly options: TOptions;
    readonly isRequired: boolean;
    
    private __value: TValue;
    private __errors: Array<string>;
    private __valueElem?: HTMLElement = null;

    designer: IFieldDesigner;
    field: IContentField;

    get valueElem(): HTMLElement { return this.__valueElem; }

    constructor(content: Content, model: ContentFieldModel) {
        this.content = content;
        this.name = model.name;
        this.title = model.title;
        this.options = model.options;
        this.isRequired = model.isRequired;

        this.__value = model.value;
        this.__errors = model.errors;
    }

    // IFieldProvider members

    getValue() {
        return this.__value;
    }

    hasValue(): boolean {
        return !!this.__value;
    }

    // other

    renderDesigner(valueElem: HTMLElement) {
        this.__valueElem = valueElem;

        this.designer = this.createDesigner();
    }

    abstract createField();
    
    protected request(options: AjaxRequest) {
        if (!options.query)
            options.query = {};

        options.query["editId"] = this.content.host.editor.editId;
        options.query["path"] = this.content.path;
        options.query["field"] = this.name;

        this.content.host.editor.api(options);
    }

    abstract createDesigner(): IFieldDesigner;
    
    protected onSavedValue(model: FieldValueResult) {
        this.__value = model.value;
        this.__errors = model.errors;
        this.field?.setErrors(this.__errors);
        this.designer?.setErrors(this.__errors);
    }

    destroy() {
        this.designer?.destroy();
    }
}

export interface IFieldDesigner {
    provider: FieldProvider<any, any>;
    element: HTMLElement;

    destroy();
    setErrors(errors: string[]);
}

export interface IContentField {
    readonly name: string;

    setValue(value: any);
    hasValue(): boolean;
    setErrors(errors: Array<string>);
    render(containr: HTMLElement);
    destroy();
}