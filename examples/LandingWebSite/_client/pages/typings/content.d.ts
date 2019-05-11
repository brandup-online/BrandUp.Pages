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