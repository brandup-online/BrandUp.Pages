export interface IContentForm {
    readonly modelPath: string;

    navigate(modelPath: string);
    getField(name: string): IContentField;
}

export interface IContentFieldProvider {
    readonly name: string;
    readonly title: string;
    readonly options: any;
    readonly isRequired: boolean;
    readonly valueElem: HTMLElement;

    renderDesigner();
    getValue();
    destroy();
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
    destroy();
}

export interface IContentFieldDesigner {
    provider: IContentFieldProvider;
    element: HTMLElement;
    
    destroy();
}