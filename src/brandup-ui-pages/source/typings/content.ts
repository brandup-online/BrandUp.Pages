import { AjaxQueue, AjaxRequest } from "brandup-ui-ajax";

export interface IContentForm {
    editId: string;
    modelPath: string;
    queue: AjaxQueue;

    request(field: IContentField, options: AjaxRequest);
    navigate(modelPath: string);
    getField(name: string): IContentField;
}

export interface IContentField {
    form: IContentForm;
    name: string;
    
    setValue(value: any);
    hasValue(): boolean;
    setErrors(errors: Array<string>);
    render(containr: HTMLElement);
    destroy();
}

export interface IPageDesigner {
    editId: string;
    queue: AjaxQueue;
    render();
    accentField(field: IContentFieldDesigner);
    clearAccent();
    destroy();
}

export interface IContentFieldDesigner {
    page: IPageDesigner;
    element: HTMLElement;
    path: string;
    name: string;
    fullPath: string;
    
    hasValue(): boolean;
    destroy();
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