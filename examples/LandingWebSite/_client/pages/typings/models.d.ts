interface PageCollectionModel {
    id: string;
    createdDate: string;
    pageId?: string;
    title: string;
    pageType: string;
    sort: "FirstOld" | "FirstNew";
}

interface PageModel {
    id: string;
    createdDate: string;
    title: string;
    status: "Draft" | "Published";
    url: string;
}

interface PageTypeModel {
    name: string;
    title: string;
}

interface Result {
    succeeded: boolean;
    errors: Array<string>;
}

interface PageNavigationModel {
    id: string;
    parentPageId: string;
    title: string;
    status: "Draft" | "Published";
    url: string;
    editId: string;
}

interface ValidationProblemDetails {
    type: string;
    title: string;
    status: number;
    detail: string;
    traceId: string;
    instance: string;
    errors: { [key: string]: Array<string> };
}