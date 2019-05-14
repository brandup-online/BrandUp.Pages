import { UIElement } from "brandup-ui";
import { PageClientModel, PageNavState, IPage, IApplication } from "../typings/website";

class Page<TModel extends PageClientModel> extends UIElement implements IPage {
    readonly app: IApplication;
    readonly nav: PageNavState;
    readonly model: TModel;
    private __webSiteToolbar: UIElement;

    constructor(app: IApplication, nav: PageNavState, model: TModel, element: HTMLElement) {
        super();

        this.app = app;
        this.nav = nav;
        this.model = model;
        this.setElement(element);

        if (this.model.cssClass)
            document.body.classList.add(this.model.cssClass);
        
        this.renderWebsiteToolbar();

        this.onRenderContent();
    }

    get typeName(): string { return "BrandUpPages.Page" }

    protected renderWebsiteToolbar() {
        if (this.app.navigation.enableAdministration) {
            import("../admin/website").then(d => {
                this.__webSiteToolbar = new d.WebSiteToolbar(this);
            });
        }
    }
    protected onRenderContent() { }

    destroy() {
        if (this.__webSiteToolbar) {
            this.__webSiteToolbar.destroy();
            this.__webSiteToolbar = null;
        }

        if (this.model.cssClass)
            document.body.classList.remove(this.model.cssClass);

        super.destroy();
    }
}

export default Page;