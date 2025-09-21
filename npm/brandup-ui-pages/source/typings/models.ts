export interface PageCollectionModel {
    id: string;
    createdDate: string;
    pageId?: string;
    title: string;
    pageType: string;
    sort: "FirstOld" | "FirstNew";
    customSorting: boolean;
    pageUrl: string;
}

export interface PageModel {
    id: string;
    createdDate: string;
    title: string;
    status: "Draft" | "Published";
    url: string;
}

export interface PageTypeModel {
    name: string;
    title: string;
}

export interface ContentTypeModel {
    name: string;
    title: string;
}

export interface ContentModel {
    title: string;
    type: ContentTypeModel;
}

export interface PageEditorModel {
    id: string;
    email: string;
}

export interface Result {
    succeeded: boolean;
    errors: Array<string>;
}

export interface ValidationProblemDetails {
    type: string;
    title: string;
    status: number;
    detail: string;
    traceId: string;
    instance: string;
    errors: { [key: string]: Array<string> };
}