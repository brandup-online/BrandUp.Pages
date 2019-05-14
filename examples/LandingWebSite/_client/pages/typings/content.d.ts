import { AjaxQueue } from "brandup-ui";

export interface IContentForm {
    editId: string;
    contentPath: string;
    queue: AjaxQueue;

    getField(name: string): IContentField;
    getValues(): { [key: string]: any };
}

export interface IContentField {
    form: IContentForm;
    name: string;

    getValue(): any;
    setValue(value: any);
    hasValue(): boolean;
    setErrors(errors: Array<string>);
    render(containr: HTMLElement);
}

export interface IPageDesigner {
    editId: string;
    queue: AjaxQueue;
    destroy();
}

export interface IContentFieldDesigner {
    page: IPageDesigner;
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