import { AjaxRequest } from "brandup-ui-ajax";

export interface IContentEditor {
    readonly editId: string;
    get content(): IContent;
    navigate(path: string): IContent;
    api(request: AjaxRequest);
    destroy();
}

export interface IContentHost {
    get editor(): IContentEditor;
    get isList(): boolean;
    attach(content: IContent);
}

export interface IContent {
    get host(): IContentHost;
    readonly path: string;
    readonly index: number;
    readonly container?: HTMLElement;

    get fields(): ReadonlyMap<string, IFieldProvider>;
    getField(name: string): IFieldProvider;
}

export interface IFieldProvider {
    readonly content: IContent;
    readonly name: string;
    readonly title: string;
    readonly options: any;
    readonly isRequired: boolean;
    readonly valueElem: HTMLElement;
    
    getValue(): any;
    hasValue(): boolean;
    destroy();
}

export interface IFieldDesigner {
    provider: IFieldProvider;
    element: HTMLElement;

    destroy();
    setErrors(errors: string[]);
}



// require refactoring

export interface IContentForm {
    readonly modelPath: string;

    navigate(modelPath: string);
    getField(name: string): IContentField;
}

export interface IContentField {
    readonly name: string;

    setValue(value: any);
    hasValue(): boolean;
    setErrors(errors: Array<string>);
    render(containr: HTMLElement);
    destroy();
}