import { UIElement, DOM, AjaxQueue } from "brandup-ui";
import { PageClientModel, PageNavState } from "./typings/website";

export class Page<TModel extends PageClientModel> extends UIElement {
    readonly nav: PageNavState;
    readonly model: TModel;

    constructor(nav: PageNavState, model: TModel, element: HTMLElement) {
        super();

        this.nav = nav;
        this.model = model;
        this.setElement(element);

        this.onRenderContent();
    }

    get typeName(): string { return "Page" }
    
    protected onRenderContent() { }
}