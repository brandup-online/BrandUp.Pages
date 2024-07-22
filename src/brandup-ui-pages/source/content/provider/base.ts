import { AjaxRequest } from "@brandup/ui-ajax";
import { Content } from "../../content/content";
import { ContentFieldModel, FieldValueResult } from "../../typings/content";

export abstract class FieldProvider<TValue, TOptions> {
    readonly content: Content;
    readonly name: string;
    readonly title: string;
    readonly type: string;
    readonly options: TOptions;
    readonly isRequired: boolean;
    
    private __value: TValue;
    private __errors: Array<string>;
    private __valueElem: HTMLElement | null = null;

    designer: IFieldDesigner | null = null;
    field: IFormField | null = null;

    get valueElem(): HTMLElement | null { return this.__valueElem; }
    get errors(): string[] { return this.__errors; }

    constructor(content: Content, model: ContentFieldModel) {
        this.content = content;
        this.name = model.name;
        this.title = model.title;
        this.options = model.options;
        this.isRequired = model.isRequired;
        this.type = model.type;

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

    registerForm(field: IFormField) {
        this.field = field;
    }

    protected request(options: AjaxRequest) {
        if (!options.query)
            options.query = {};

        options.disableCache = true;

        options.query["editId"] = this.content.host.editor.editId;
        options.query["path"] = this.content.path;
        options.query["field"] = this.name;

        this.content.host.editor.api(options);
    }

    abstract createDesigner(): IFieldDesigner | null;

    abstract saveValue(value: any): void;
    
    protected onSavedValue(model: FieldValueResult) {
        this.__value = model.value;
        this.__errors = model.errors;
        this.designer?.setErrors(this.__errors);
        this.field?.raiseUpdateErrors(model.errors);
        this.field?.raiseUpdateValue(model.value);
    }

    destroy() {
        this.designer?.destroy();
    }
}
export interface IFieldDesigner {
    provider: FieldProvider<any, any>;
    element: HTMLElement | null;

    destroy(): void;
    setErrors(errors: string[]): void;
}

export interface IFormField {
    raiseUpdateValue(value: any): void;
    raiseUpdateErrors(errors: Array<string>): void;
    destroy(): void;
}