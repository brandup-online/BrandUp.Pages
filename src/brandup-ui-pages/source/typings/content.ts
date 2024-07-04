import { Content } from "../content/content";

export interface IContentForm {
    readonly modelPath: string;

    navigate(modelPath: string);
    getField(name: string): IContentField;
}

export interface IContentFieldProvider {
    readonly editor: IPageDesigner;
    readonly content: IContent;
    readonly name: string;
    readonly title: string;
    readonly options: any;
    readonly isRequired: boolean;
    readonly valueElem: HTMLElement;

    renderDesigner();
    getValue();
    destroy();
}

export interface IModelFieldProvider extends IContentFieldProvider {
}

export interface IContentField {
    readonly name: string;
    
    setValue(value: any);
    hasValue(): boolean;
    setErrors(errors: Array<string>);
    render(containr: HTMLElement);
    destroy();
}

export interface IPageDesigner {
    readonly editId: string;
    get content(): Content;
    destroy();
}

export interface IContentFieldDesigner {
    provider: IContentFieldProvider;
    element: HTMLElement;
    
    destroy();
    setErrors(errors: string[]);
}

export interface IContent {
    readonly editor: IPageDesigner;
    readonly path: string;
    readonly index: number;
    readonly container?: HTMLElement;

    getField(name: string): IContentFieldProvider;
}

export interface IParentContent {
    map(content: IContent);
}