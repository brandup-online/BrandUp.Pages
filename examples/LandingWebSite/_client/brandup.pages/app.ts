import { AppClientModel, NavigationModel, PageClientModel, PageNavState, IApplication, NavigationOptions } from "./typings/website";
import Page from "./pages/page";
import { UIElement, DOM, Utility, ajaxRequest, AjaxRequestOptions } from "brandup-ui";
import "./app.less";

export class Application<TModel extends AppClientModel> extends UIElement implements IApplication {
    private __navCounter: number = 0;
    private __navigation: NavigationModel;
    private __contentBodyElem: HTMLElement;
    readonly model: TModel;
    private options: AppSetupOptions;
    page: Page<PageClientModel> = null;
    private linkClickFunc: () => void;
    private __builder: ApplicationBuilder;
    private __requestVerificationToken: HTMLInputElement;
    private __progressElem: HTMLElement;
    private __progressInterval: number;
    private __progressTimeout: number;
    private __progressStart: number;

    protected constructor(model: TModel, options: AppSetupOptions) {
        super();

        this.model = model;
        this.options = options ? options : { defaultPageScript: "page" };

        if (!this.options.defaultPageScript)
            this.options.defaultPageScript = "page";

        this.__builder = new ApplicationBuilder();
        this.__builder.addPageType("page", () => import("./pages/page"));
        this.__builder.addPageType("content", () => import("./pages/content"));

        if (options.configure)
            options.configure(this.__builder);

        this.setElement(document.body);

        this.defineEvent("pageNavigating", { cancelable: true, bubbles: true });
        this.defineEvent("pageNavigated", { cancelable: false, bubbles: true });
        this.defineEvent("pageLoading", { cancelable: false, bubbles: true });
        this.defineEvent("pageLoaded", { cancelable: false, bubbles: true });
        this.defineEvent("pageContentLoaded", { cancelable: false, bubbles: true });

        this.linkClickFunc = Utility.createDelegate(this, this.__onClickAppLink);
    }

    get typeName(): string { return "Application" }
    get navigation(): NavigationModel { return this.__navigation; }

    init() {
        this.__contentBodyElem = document.getElementById("page-content");
        if (!this.__contentBodyElem)
            throw "Не найден элемент контента страницы.";

        this.__requestVerificationToken = <HTMLInputElement>DOM.getElementByName("__RequestVerificationToken");
        if (this.__requestVerificationToken == null)
            throw `Не найден элемент с именем __RequestVerificationToken.`;

        var initNav = this.model.nav;
        var pageState: PageNavState = {
            url: initNav.url,
            title: initNav.page.title,
            path: initNav.path,
            params: initNav.query,
            hash: location.hash ? location.hash.substr(1) : null
        };

        this.__navigation = initNav;
        
        if (window.history) {
            window.addEventListener("popstate", Utility.createDelegate(this, this.__onPopState));
            window.addEventListener("hashchange", Utility.createDelegate(this, this.__onHashChange));

            window.history.replaceState(pageState, pageState.title, pageState.hash ? pageState.url + "#" + pageState.hash : pageState.url);
        }

        document.body.addEventListener("click", this.linkClickFunc, false);
        document.body.appendChild(this.__progressElem = DOM.tag("div", { class: "bp-page-loader" }));

        this.__renderPage(pageState, initNav.page, false);
    }
    load() { }
    destroy() {
        document.body.removeEventListener("click", this.linkClickFunc, false);
    }

    request(options: AjaxRequestOptions) {
        if (!options.headers)
            options.headers = {};

        if (this.model.antiforgery && options.method !== "GET")
            options.headers[this.model.antiforgery.headerName] = this.__navigation.validationToken;

        ajaxRequest(options);
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
        this.nav({ url: null, pushState: false });
    }
    navigate(target: any) {
        if (!target)
            throw new Error("target not set");

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

        this.nav({ url: url, pushState: true });
    }
    nav(options: NavigationOptions) {
        var isCancelled = this.raiseEvent("pageNavigating", options);
        if (!isCancelled) {
            console.log("cancelled navigation");
            return;
        }

        let { url, hash, pushState } = options;
        if (!url) {
            url = location.href;
            if (url.lastIndexOf("#") > 0)
                url = url.substr(0, url.lastIndexOf("#"));
        }
        if (!hash)
            hash = null;
        else {
            if (hash.startsWith("#"))
                hash = hash.substr(1);
        }

        this.__navCounter++;
        var navSequence = this.__navCounter;

        this.element.classList.remove("bp-state-loaded");
        this.element.classList.add("bp-state-loading");

        window.clearTimeout(this.__progressTimeout);
        window.clearInterval(this.__progressInterval);

        this.__progressStart = Date.now();
        var pw = 0;
        this.__progressInterval = window.setInterval(() => {
            this.__progressElem.style.display = "block";
            this.__progressElem.style.width = pw + "%";
            pw++;
        }, 20);

        this.request({
            url: url,
            urlParams: { handler: "navigation" },
            type: "JSON",
            success: (data: NavigationModel, status: number) => {
                if (navSequence !== this.__navCounter)
                    return;

                switch (status) {
                    case 200: {
                        var navUrl = data.url;
                        if (hash)
                            navUrl += "#" + hash;

                        if (this.__navigation.isAuthenticated != data.isAuthenticated) {
                            location.href = navUrl;
                            return;
                        }

                        this.__navigation = data;

                        var pageModel = data.page;
                        var pageState: PageNavState = {
                            url: data.url,
                            title: pageModel.title,
                            path: data.path,
                            params: data.query,
                            hash: hash
                        };

                        if (navUrl == location.href)
                            pushState = false;

                        if (window.history && window.history.pushState) {
                            if (pushState) {
                                window.history.pushState(pageState, pageState.title, navUrl);
                            }
                            else
                                window.history.replaceState(pageState, pageState.title, navUrl);
                        }
                        else {
                            location.href = navUrl;
                            return;
                        }

                        document.title = pageModel.title ? pageModel.title : "";

                        this.raiseEvent("pageNavigated", pageState);

                        this.__renderPage(pageState, pageModel, true);

                        break;
                    }
                    case 404:
                    case 500: {
                        location.href = url;
                        break;
                    }
                    case 401 /* Unauthorized */: {
                        location.href = url;
                        break;
                    }
                    default:
                        throw new Error();
                }
            }
        });
    }

    script(name: string): Promise<{ default: any }> {
        var scriptPromise = this.__builder.getScript(name);
        if (!scriptPromise)
            return;
        return scriptPromise;
    }
    renderPage(html: string) {
        var navState = this.page.nav;
        var pageModel = this.page.model;

        this.page.destroy();
        this.page = null;

        DOM.empty(this.__contentBodyElem);
        this.__contentBodyElem.insertAdjacentHTML("afterbegin", html ? html : "");
        Application.nodeScriptReplace(this.__contentBodyElem);

        this.__loadPageScript(navState, pageModel);
    }

    private __renderPage(pageState: PageNavState, pageModel: PageClientModel, needLoadContent: boolean) {
        if (this.page) {
            this.page.destroy();
            this.page = null;
        }

        this.raiseEvent("pageLoading");

        if (needLoadContent)
            this.__loadContent(pageState, pageModel);
        else
            this.__loadPageScript(pageState, pageModel);
    }
    private __loadContent(pageState: PageNavState, pageModel: PageClientModel) {
        var navSequence = this.__navCounter;

        this.request({
            url: pageState.url,
            urlParams: { _content: "" },
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

                        this.raiseEvent("pageContentLoaded");

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
        var pageScript = pageModel.scriptName;
        if (!pageScript)
            pageScript = this.options.defaultPageScript;

        this.__builder.getPageType(pageScript).then((pageType) => {
            this.__createPage(pageType.default, pageState, pageModel);
        });
    }
    private __createPage(pageType: any, pageState: PageNavState, pageModel: PageClientModel) {
        this.page = new pageType(this, pageState, pageModel, this.__contentBodyElem);
        
        document.body.classList.remove("bp-state-loading");
        document.body.classList.add("bp-state-loaded");

        var d = 300 - (Date.now() - this.__progressStart);
        if (d < 0)
            d = 0;

        this.__progressTimeout = window.setTimeout(() => {
            window.clearInterval(this.__progressInterval);
            this.__progressElem.style.width = "100%";

            this.__progressTimeout = window.setTimeout(() => {
                this.__progressElem.style.display = "none";
            }, 200);
        }, d);

        this.raiseEvent("pageLoaded");
    }

    private __onPopState(event: PopStateEvent) {
        event.preventDefault();

        var url = location.href;
        console.log("PopState: " + url);

        if (url.lastIndexOf("#") > 0) {
            let t = url.lastIndexOf("#");
            let urlHash = url.substr(t + 1);
            let urlWithoutHash = url.substr(0, t);

            if (!event.state) {
                console.log("PopState hash: " + urlHash);

                var pageState: PageNavState = {
                    url: url.substr(0, t),
                    title: this.__navigation.page.title,
                    path: this.__navigation.path,
                    params: this.__navigation.query,
                    hash: urlHash
                };

                window.history.replaceState(pageState, pageState.title, location.href);

                return;
            }
            else {
                var state = <PageNavState>event.state;
                if (urlWithoutHash.toLowerCase() == this.__navigation.url.toLowerCase())
                    return;
            }
        }

        if (event.state) {
            var state = <PageNavState>event.state;
            this.nav({ url: state.url, hash: state.hash, pushState: false });

            return;
        }
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

        if (elem.hasAttribute("target")) {
            let target = elem.getAttribute("target");
            if (target === "_blank")
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
            document.body.classList.add("bp-state-loading");

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

export interface AppSetupOptions {
    defaultPageScript?: string;
    configure?: (builder: ApplicationBuilder) => void;
}

export class ApplicationBuilder {
    private __pageTypes: { [key: string]: () => Promise<any> } = {};
    private __scripts: { [key: string]: () => Promise<any> } = {};

    addPageType(name: string, importFunc: () => Promise<any>) {
        this.__pageTypes[name.toLowerCase()] = importFunc;
    }
    getPageType(name: string): Promise<{ default: any }> {
        var f = this.__pageTypes[name.toLowerCase()];
        return f();
    }

    addScript(name: string, importFunc: () => Promise<any>) {
        this.__scripts[name.toLowerCase()] = importFunc;
    }
    getScript(name: string): Promise<{ default: any }> {
        var f = this.__scripts[name.toLowerCase()];
        return f();
    }
}