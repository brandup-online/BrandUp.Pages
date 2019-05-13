import { AppClientModel, NavigationModel, PageClientModel, AppSetupOptions, PageNavState } from "./typings/website";
import { Page } from "./page";
import { UIElement, DOM, Utility, ajaxRequest } from "brandup-ui";

export class Application<TModel extends AppClientModel> extends UIElement implements IApplication {
    private __navCounter: number = 0;
    private __navigation: NavigationModel;
    private __contentBodyElem: HTMLElement;
    readonly model: TModel;
    private options: AppSetupOptions;
    page: Page<PageClientModel> = null;
    private linkClickFunc: () => void;

    protected constructor(model: TModel, options: AppSetupOptions) {
        super();

        this.model = model;
        this.options = options ? options : { onCreatePage: null };

        this.setElement(document.body);

        this.linkClickFunc = Utility.createDelegate(this, this.__onClickAppLink);
    }

    get typeName(): string { return "Application" }

    init() {
        this.__contentBodyElem = document.getElementById("page-content");
        if (!this.__contentBodyElem)
            throw "Не найден элемент контента страницы.";

        var initNav = this.model.nav;
        var pageState: PageNavState = {
            url: initNav.url,
            title: initNav.page.title
        };

        this.__navigation = initNav;
        
        if (window.history) {
            window.addEventListener("popstate", Utility.createDelegate(this, this.__onPopState));
            window.addEventListener("hashchange", Utility.createDelegate(this, this.__onHashChange));

            window.history.replaceState(pageState, pageState.title, pageState.url);
        }

        document.body.addEventListener("click", this.linkClickFunc, false);

        this.__renderPage(pageState, initNav.page, false);
    }
    load() {

    }
    destroy() {
        document.body.removeEventListener("click", this.linkClickFunc, false);
    }

    uri(path?: string, queryParams?: { [key: string]: string; }): string {
        var url = this.model.baseUrl;
        if (path)
        //    url += "/";
        //else
        {
            if (path.substr(0, 1) == "/")
                path = path.substr(1);
            url += path;
        }

        if (queryParams) {
            var query = "";
            var i = 0;
            for (var key in queryParams) {
                var value = queryParams[key];
                if (value === null || typeof value === "undefined")
                    continue;

                if (i > 0)
                    query += "&";

                query += key;

                if (value)
                    query += "=" + value;

                i++;
            }

            if (query)
                url += "?" + query;
        }

        return url;
    }

    reload() {
        location.reload();
    }
    navigate(target: any) {
        if (!target) {
            throw new Error("target not set");
        }

        var url: string = null;
        var targetElem: HTMLElement = null;
        if (Utility.isString(target))
            url = target;
        else {
            targetElem = <HTMLElement>target;
            if (targetElem.tagName === "A")
                url = targetElem.getAttribute("href");
            else
                throw "Не удалось получить Url адрес для перехода.";
        }

        if (!url)
            url = this.model.baseUrl;

        this.__refreshNavigation({ url: url, pushState: true });
    }
    nav(options: NavigationOptions) {
        this.__refreshNavigation(options);
    }

    reRenderPage(content: string) {
        if (!this.page)
            throw "";

        var pageModel = this.page.model;
        var navState = this.page.nav;

        this.page.destroy();
        this.page = null;

        DOM.empty(this.__contentBodyElem);
        this.__contentBodyElem.insertAdjacentHTML("afterbegin", content);

        this.__renderPage(navState, pageModel, false);
    }

    private __refreshNavigation(options: NavigationOptions) {
        let { url, pushState } = options;

        this.__navCounter++;
        var navSequence = this.__navCounter;

        this.element.classList.remove("app-state-loaded");
        this.element.classList.add("app-state-loading");

        this.element.dispatchEvent(new CustomEvent("cmsnavigateing", { cancelable: false, bubbles: false, detail: options }));

        ajaxRequest({
            url: url,
            urlParams: { handler: "navigation" },
            type: "JSON",
            success: (data: NavigationModel, status: number) => {
                if (navSequence !== this.__navCounter)
                    return;
                
                switch (status) {
                    case 200: {
                        if (this.__navigation.isAuthenticated != data.isAuthenticated) {
                            location.href = data.url;
                            return;
                        }

                        this.__navigation = data;

                        var pageModel = data.page;
                        var pageState: PageNavState = {
                            url: data.url,
                            title: pageModel.title
                        };

                        if (window.history && window.history.pushState) {
                            if (pushState)
                                window.history.pushState(pageState, pageState.title, data.url);
                            else
                                window.history.replaceState(pageState, pageState.title, data.url);
                        }
                        else {
                            location.href = data.url;
                            return;
                        }

                        document.title = pageModel.title ? pageModel.title : "";

                        this.element.dispatchEvent(new CustomEvent("cmsnavigated", { cancelable: false, bubbles: false, detail: pageState }));

                        this.__renderPage(pageState, pageModel, true);

                        break;
                    }
                    case 404:
                    case 500: {
                        location.href = options.url;
                        break;
                    }
                    case 401 /* Unauthorized */: {
                        location.href = options.url;
                        break;
                    }
                    default:
                        throw new Error();
                }
            }
        });
    }
    private __renderPage(pageState: PageNavState, pageModel: PageClientModel, needLoadContent: boolean) {
        if (this.page) {
            if (this.page.model.cssClass)
                document.body.classList.remove(this.page.model.cssClass);

            this.page.destroy();
            this.page = null;
        }

        if (needLoadContent)
            this.__loadContent(pageState, pageModel);
        else
            this.__loadPageScript(pageState, pageModel);
    }
    private __loadContent(pageState: PageNavState, pageModel: PageClientModel) {
        var navSequence = this.__navCounter;

        ajaxRequest({
            url: pageState.url,
            urlParams: { handler: "content" },
            success: (data: string, status: number) => {
                if (navSequence !== this.__navCounter)
                    return;

                switch (status) {
                    case 200: {
                        window.scrollTo(0, 0);

                        DOM.empty(this.__contentBodyElem);
                        this.__contentBodyElem.insertAdjacentHTML("afterbegin", data ? data : "");
                        Application.nodeScriptReplace(this.__contentBodyElem);

                        this.__loadPageScript(pageState, pageModel);

                        break;
                    }
                    case 404:
                    case 500:
                    case 401: {
                        location.reload();
                        break;
                    }
                    default:
                        throw new Error();
                }
            }
        });
    }
    private __loadPageScript(pageState: PageNavState, pageModel: PageClientModel) {
        if (pageModel.cssClass)
            document.body.classList.add(pageModel.cssClass);

        var pageType: any = Page;

        if (pageModel.scriptName) {
            if (this.options.onCreatePage) {
                this.options.onCreatePage(pageModel.scriptName).then((pageType) => {
                    this.__createPage(pageType.default, pageState, pageModel);
                });
                return;
            }
            else
                throw "";
        }

        this.__createPage(pageType, pageState, pageModel);
    }
    private __createPage(pageType: any, pageState: PageNavState, pageModel: PageClientModel) {
        this.page = new pageType(pageState, pageModel, this.__contentBodyElem);

        document.body.classList.remove("app-state-loading");
        document.body.classList.add("app-state-loaded");
    }

    private __onPopState(event: PopStateEvent) {
        event.preventDefault();

        console.log("PopState " + location.href);

        if (event.state) {
            var state = <PageNavState>event.state;
            this.__refreshNavigation({ url: state.url, pushState: false });

            return;
        }

        var initNav = this.model.nav;
        var pageState: PageNavState = {
            url: initNav.url,
            title: initNav.page.title
        };
        window.history.replaceState(pageState, pageState.title, initNav.url);
    }
    private __onHashChange(e: HashChangeEvent) {
        console.log("HashChange to " + e.newURL);
    }
    private __onClickAppLink(e: MouseEvent) {
        var elem = <HTMLElement>e.target;
        while (true) {
            if (elem.classList.contains("applink"))
                break;
            if (elem === e.currentTarget)
                return;

            elem = elem.parentElement;
            if (elem === null)
                return true;
        }

        e.preventDefault();
        e.stopPropagation();
        e.returnValue = false;

        this.navigate(elem);

        return false;
    }

    static nodeScriptReplace(node: Node) {
        if ((<Element>node).tagName === "SCRIPT") {
            let script = document.createElement("script");
            script.text = (<Element>node).innerHTML;
            for (let i = (<Element>node).attributes.length - 1; i >= 0; i--)
                script.setAttribute((<Element>node).attributes[i].name, (<Element>node).attributes[i].value);
            node.parentNode.replaceChild(script, node);
        }
        else {
            let i = 0;
            let children = node.childNodes;
            while (i < children.length)
                Application.nodeScriptReplace(children[i++]);
        }

        return node;
    }
    static setup<TModel extends AppClientModel>(options: AppSetupOptions, init?: (app: Application<TModel>) => void) {
        if (window.hasOwnProperty("app")) {
            let app = <Application<TModel>>window["app"];
            app.destroy();

            delete window["app"];
        }

        if (window.hasOwnProperty("appInitOptions")) {
            document.body.classList.add("app-state-loading");

            let appModel = <TModel>window["appInitOptions"];
            let app = new Application<TModel>(appModel, options);
            window["app"] = app;

            document.addEventListener("readystatechange", () => {
                if (document.readyState === "complete") {
                    app.init();
                    if (init)
                        init(app);
                }
            });

            window.addEventListener("load", () => {
                app.load();
            });

            return app;
        }

        return null;
    }
}