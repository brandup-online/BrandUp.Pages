import { AjaxQueue, AjaxRequest } from "@brandup/ui-ajax";

export interface IContentForm {
    editId: string;
    modelPath: string;
    queue: AjaxQueue | undefined;

    request(field: IContentField, options: AjaxRequest): void;
    navigate(modelPath: string): void;
    getField(name: string): IContentField;
}

export interface IContentField {
    form: IContentForm;
    name: string;

    setValue(value: any): void;
    hasValue(): boolean;
    setErrors(errors: Array<string> | null): void;
    render(containr: HTMLElement): void;
    destroy(): void;
}

export interface IPageDesigner {
    editId: string;
    queue: AjaxQueue;
    render(): void;
    accentField(field: IContentFieldDesigner): void;
    clearAccent(): void;
    destroy(): void;
}

export interface IContentFieldDesigner {
    page: IPageDesigner;
    element: HTMLElement;
    path: string;
    name: string;
    fullPath: string;

    hasValue(): boolean;
    destroy(): void;
}

export interface PageContentForm {
    path: PageContentPath;
    fields: Array<ContentFieldModel>;
    values: { [key: string]: any };
}

export interface PageContentPath {
    parent: PageContentPath;
    name: string;
    title: string;
    index: number;
    modelPath: string;
}

export interface ContentFieldModel {
    type: string;
    name: string;
    title: string;
    options: any;
    value: any;
}