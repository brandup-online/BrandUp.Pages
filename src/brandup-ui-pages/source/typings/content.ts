import { AjaxQueue, AjaxRequest } from "brandup-ui-ajax";
import { DesignerEvent } from "../content/designer/base";

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
    redraw();
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
    getValue(): any;
    setValue(val: any);
    setValid(val: boolean);
    setCallback(name: string, handler: (e: DesignerEvent<any>) => void);
    destroy();
}

export interface PageContentForm {
    path: PageContentPath;
    fields: Array<ContentFieldModel<any, any>>;
    values: { [key: string]: any };
}

export interface PageContentPath {
    parent: PageContentPath;
    name: string;
    title: string;
    index: number;
    modelPath: string;
}

export interface ContentFieldModel<TValue, TOptions> {
    type: string;
    name: string;
    title: string;
    options: TOptions;
    value: TValue;
    errors: string[];
    isRequired: boolean;
}