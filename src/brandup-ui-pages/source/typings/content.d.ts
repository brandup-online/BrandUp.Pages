// Contract for content API

export interface BeginContentEditResult {
    editId: string;
    exist: boolean;
}

export interface GetContentEditResult {
    contentKey: string;
    path: string;
    contents: Array<ContentModel>;
}

export interface ContentModel {
    parentPath: string;
    parentField: string;
    path: string;
    index: number;
    typeName: string;
    typeTitle: string;
    fields: ContentFieldModel[];
}

export interface ValidationContentModel extends ContentModel {
    errors: string[];
}

export interface ContentFieldModel {
    type: string;
    name: string;
    title: string;
    options: any;
    isRequired: boolean;
    value: any;
    errors: string[];
}

export interface FieldValueResult {
    value: any;
    errors: Array<string>;
}

export interface IFieldValueElement {
    element: HTMLElement;

    onChange: (value: any) => void;
    setValue: (value: any) => void;
    getValue: () => any;
    destroy: () => void;
}