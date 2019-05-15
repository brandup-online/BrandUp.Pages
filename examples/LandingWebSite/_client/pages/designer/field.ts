import { UIElement, AjaxRequestOptions } from "brandup-ui";
import { IContentFieldDesigner, IPageDesigner } from "../typings/content";
import "./field.less";

export abstract class FieldDesigner<TOptions> extends UIElement implements IContentFieldDesigner {
    readonly page: IPageDesigner;
    readonly options: TOptions;
    readonly path: string;
    readonly name: string;
    readonly fullPath: string;

    constructor(page: IPageDesigner, elem: HTMLElement, options: TOptions) {
        super();

        this.page = page;
        this.options = options;
        this.path = elem.getAttribute("content-path");
        this.name = this.fullPath = elem.getAttribute("content-field");

        if (this.path)
            this.fullPath = this.path + "." + this.fullPath;

        this.setElement(elem);

        elem.classList.add("field-designer");

        this.onRender(elem);
    }

    protected abstract onRender(elem: HTMLElement);
    request(options: AjaxRequestOptions) {
        if (!options.urlParams)
            options.urlParams = {};

        options.urlParams["editId"] = this.page.editId;
        options.urlParams["path"] = this.path;
        options.urlParams["field"] = this.name;
        
        this.page.queue.request(options);
    }
    
    abstract hasValue(): boolean;
}