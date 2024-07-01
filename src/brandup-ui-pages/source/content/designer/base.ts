import { UIElement } from "brandup-ui";
import { AjaxRequest } from "brandup-ui-ajax";
import { IContentFieldDesigner, IPageDesigner } from "../../typings/content";
import "./base.less";

export abstract class FieldDesigner<TOptions> extends UIElement implements IContentFieldDesigner {
    readonly page: IPageDesigner;
    readonly options: TOptions;
    readonly path: string;
    readonly name: string;
    readonly fullPath: string;
    protected eventCallbacks: { [keys: string]: ((e: DesignerEvent<any>) => void)[] } = {};

    constructor(page: IPageDesigner, elem: HTMLElement, options: TOptions) {
        super();

        this.page = page;
        this.options = options;
        this.path = elem.getAttribute("data-content-field-path");
        this.name = this.fullPath = elem.getAttribute("data-content-field-name");

        if (this.path)
            this.fullPath = this.path + "." + this.fullPath;

        this.setElement(elem);

        elem.classList.add("field-designer");

        this.onRender(elem);
    }

    setCallback(name: string, handler: (e: DesignerEvent<any>) => void) {
        if (!this.eventCallbacks[name]) this.eventCallbacks[name] = [];
        this.eventCallbacks[name].push(handler);
    }

    protected triggerCallback(name: string, e: DesignerEvent<any>) {
        if (this.eventCallbacks[name])
            this.eventCallbacks[name].forEach(hook => hook(e));
    }

    removeCallback(name:string) {
        delete this.eventCallbacks[name];
    }

    protected abstract onRender(elem: HTMLElement);
    request(options: AjaxRequest) {
        if (!options.urlParams)
            options.urlParams = {};

        options.urlParams["editId"] = this.page.editId;
        options.urlParams["path"] = this.path;
        options.urlParams["field"] = this.name;

        this.page.queue.push(options);
    }

    setValid(val: boolean) {
        val ? this.element.classList.remove("invalid") : this.element.classList.add("invalid");
    }

    abstract hasValue(): boolean;

    destroy() {
        this.element.classList.remove("field-designer");
        super.destroy();
    }
}

export interface DesignerEvent<TValue> {
    value: TValue;
    target: HTMLElement;
}
