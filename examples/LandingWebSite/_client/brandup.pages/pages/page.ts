import { UIElement, DOM, UIControl, AjaxQueue } from "brandup-ui";
import { PageClientModel, PageNavState, IPage, IApplication } from "../typings/website";

class Page<TModel extends PageClientModel> extends UIElement implements IPage {
    readonly app: IApplication;
    readonly nav: PageNavState;
    readonly model: TModel;
    readonly queue: AjaxQueue;
    private __destroyCallbacks: Array<() => void> = [];
    private __scripts: Array<UIElement> = [];

    constructor(app: IApplication, nav: PageNavState, model: TModel, element: HTMLElement) {
        super();

        this.app = app;
        this.nav = nav;
        this.model = model;
        this.queue = new AjaxQueue();
        this.setElement(element);

        if (this.model.cssClass) {
            document.body.classList.add(this.model.cssClass);
            this.attachDestroyFunc(() => { document.body.classList.remove(this.model.cssClass); });
        }

        this.renderWebsiteToolbar();

        this.refreshScripts();

        this.onRenderContent();
    }

    get typeName(): string { return "BrandUpPages.Page" }

    protected renderWebsiteToolbar() {
        if (this.app.navigation.enableAdministration) {
            import("../admin/website").then(d => {
                this.attachDestroyElement(new d.WebSiteToolbar(this));
            });
        }
    }
    protected onRenderContent() { }

    attachDestroyFunc(f: () => void) {
        this.__destroyCallbacks.push(f);
    }
    attachDestroyElement(elem: UIElement) {
        this.__destroyCallbacks.push(() => { elem.destroy(); });
    }

    refreshScripts() {
        var scriptElements = DOM.queryElements(this.element, "[content-script]");
        for (var i = 0; i < scriptElements.length; i++) {
            let elem = scriptElements.item(i);
            if (elem.hasAttribute("brandup-ui-element"))
                continue;

            let scriptName = elem.getAttribute("content-script");
            let s = this.app.script(scriptName);
            if (s) {
                s.then((t) => {
                    if (!this.__scripts)
                        return;

                    let uiElem: UIElement = new t.default(elem);
                    this.__scripts.push(uiElem);
                });
            }
        }
    }

    destroy() {
        if (this.__scripts != null) {
            this.__scripts.map((elem) => { elem.destroy(); });
            this.__scripts = null;
        }

        if (this.__destroyCallbacks != null) {
            this.__destroyCallbacks.map((f) => { f(); });
            this.__destroyCallbacks = null;
        }

        this.queue.destroy();
        
        super.destroy();
    }
}

export default Page;