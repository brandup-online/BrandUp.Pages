import { UIElement } from "@brandup/ui";
import { AjaxRequest } from "@brandup/ui-ajax";
import { IContentFieldDesigner, IPageDesigner } from "../../typings/content";
import "./base.less";

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

    protected abstract onRender(elem: HTMLElement): void;
    request(options: AjaxRequest) {
        if (!options.query)
            options.query = {};

        options.query["editId"] = this.page.editId;
        options.query["path"] = this.path;
        options.query["field"] = this.name;

        this.page.queue.push(options);
    }

    abstract hasValue(): boolean;
}