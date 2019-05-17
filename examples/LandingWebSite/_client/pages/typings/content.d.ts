import { AjaxQueue } from "brandup-ui";

export interface IContentForm {
    editId: string;
    contentPath: string;
    queue: AjaxQueue;

    getField(name: string): IContentField;
}

export interface IContentField {
    form: IContentForm;
    name: string;
    
    setValue(value: any);
    hasValue(): boolean;
    setErrors(errors: Array<string>);
    render(containr: HTMLElement);
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