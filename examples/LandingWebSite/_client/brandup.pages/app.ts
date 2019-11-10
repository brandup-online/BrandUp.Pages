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
    private keyDownUpFunc: () => void;
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
        this.keyDownUpFunc = Utility.createDelegate(this, this.__onKeyDownUp);
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
        document.body.addEventListener("keydown", this.keyDownUpFunc, false);
        document.body.addEventListener("keyup", this.keyDownUpFunc, false);
        document.body.appendChild(this.__progressElem = DOM.tag("div", { class: "bp-page-loader" }));

        this.__renderPage(pageState, initNav.page, false, false);
    }
    load() { }
    destroy() {
        document.body.removeEventListener("click", this.linkClickFunc, false);
        document.body.removeEventListener("keydown", this.keyDownUpFunc, false);
        document.body.removeEventListener("keyup", this.keyDownUpFunc, false);
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
        var pushState = true;
        var targetElem: HTMLElement = null;
        if (Utility.isString(target))
            url = target;
        else {
            targetElem = <HTMLElement>target;
            if (targetElem.tagName === "A")
                url = targetElem.getAttribute("href");
            else if (targetElem.hasAttribute("data-href"))
                url = targetElem.getAttribute("data-href");
            else
                throw "Не удалось получить Url адрес для перехода.";

            if (targetElem.hasAttribute("data-url-replace"))
                pushState = false;
        }

        if (!url)
            url = location.href;

        this.nav({ url: url, pushState: pushState, scrollToTop: pushState });
    }
    nav(options: NavigationOptions) {
        var isCancelled = this.raiseEvent("pageNavigating", options);
        if (!isCancelled) {
            console.log("cancelled navigation");
            return;
        }

        let { url, hash, pushState, notRenderPage, scrollToTop } = options;
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

        this.__beginLoading();

        this.request({
            url: url,
            method: "POST",
            urlParams: { _nav: "" },
            type: "TEXT",
            data: this.navigation.state ? this.navigation.state : "",
            success: (data: NavigationModel, status: number, xhr: XMLHttpRequest) => {
                if (navSequence !== this.__navCounter)
                    return;

                switch (status) {
                    case 200: {
                        let pageAction = xhr.getResponseHeader("Page-Action");
                        if (pageAction) {
                            switch (pageAction) {
                                case "reset": {
                                    location.href = url;
                                    return;
                                }
                                default:
                                    throw "";
                            }
                        }

                        let redirectLocation = xhr.getResponseHeader("Page-Location");
                        if (redirectLocation) {
                            if (redirectLocation.startsWith("/"))
                                this.navigate(redirectLocation);
                            else
                                location.href = redirectLocation;
                            return;
                        }

                        var navUrl = data.url;
                        if (hash)
                            navUrl += "#" + hash;

                        if (this.__navigation.isAuthenticated != data.isAuthenticated) {
                            location.href = navUrl;
                            return;
                        }

                        this.__navigation = data;

                        var pageModel = data.page;
                        var navState: PageNavState = {
                            url: data.url,
                            title: pageModel.title,
                            path: data.path,
                            params: data.query,
                            hash: hash
                        };

                        if (navUrl == location.href)
                            pushState = false;

                        if (window.history && window.history.pushState) {
                            if (pushState)
                                window.history.pushState(navState, navState.title, navUrl);
                            else
                                window.history.replaceState(navState, navState.title, navUrl);
                        }
                        else {
                            location.href = navUrl;
                            return;
                        }

                        document.title = pageModel.title ? pageModel.title : "";

                        this.raiseEvent("pageNavigated", navState);

                        if (options.success)
                            options.success();

                        if (!notRenderPage)
                            this.__renderPage(navState, pageModel, true, scrollToTop);
                        else {
                            this.page.update(navState, pageModel);

                            this.__endLoading();
                        }

                        break;
                    }
                    default:
                        location.href = url;
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

        this.__loadPageScript(navState, pageModel, html ? html : "", false);
    }

    private __renderPage(pageState: PageNavState, pageModel: PageClientModel, needLoadContent: boolean, scrollToTop: boolean) {
        this.raiseEvent("pageLoading");

        if (scrollToTop)
            window.scrollTo({ left: 0, top: 0, behavior: "smooth" });

        if (needLoadContent)
            this.__loadContent(pageState, pageModel, scrollToTop);
        else
            this.__loadPageScript(pageState, pageModel, null, scrollToTop);
    }
    private __loadContent(pageState: PageNavState, pageModel: PageClientModel, scrollToTop: boolean) {
        var navSequence = this.__navCounter;

        this.request({
            url: pageState.url,
            urlParams: { _content: "" },
            success: (data: string, status: number, xhr: XMLHttpRequest) => {
                if (navSequence !== this.__navCounter)
                    return;

                switch (status) {
                    case 200: {
                        let redirectLocation = xhr.getResponseHeader("Page-Location");
                        if (redirectLocation) {
                            if (redirectLocation.startsWith("/"))
                                this.navigate(redirectLocation);
                            else
                                location.href = redirectLocation;
                            return;
                        }

                        this.__loadPageScript(pageState, pageModel, data ? data : "", scrollToTop);

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
    private __loadPageScript(pageState: PageNavState, pageModel: PageClientModel, contentHtml: string, scrollToTop: boolean) {
        var pageScript = pageModel.scriptName;
        if (!pageScript)
            pageScript = this.options.defaultPageScript;

        this.__builder.getPageType(pageScript)
            .then((pageType) => {
                if (this.page) {
                    this.page.destroy();
                    this.page = null;
                }

                if (contentHtml !== null) {
                    DOM.empty(this.__contentBodyElem);
                    this.__contentBodyElem.insertAdjacentHTML("afterbegin", contentHtml);
                    Application.nodeScriptReplace(this.__contentBodyElem);
                }

                this.__createPage(pageType.default, pageState, pageModel);
            })
            .catch(() => {
                //location.reload();
            });
    }
    private __createPage(pageType: any, pageState: PageNavState, pageModel: PageClientModel) {
        this.page = new pageType(this, pageState, pageModel, this.__contentBodyElem);

        this.__endLoading();

        this.raiseEvent("pageLoaded");
    }

    private __beginLoading() {
        this.__progressElem.style.width = "0%";
        this.__progressElem.classList.add("show");

        this.element.classList.remove("bp-state-loaded");
        this.element.classList.add("bp-state-loading");

        window.clearTimeout(this.__progressTimeout);
        window.clearInterval(this.__progressInterval);

        this.__progressStart = Date.now();
        var pw = 0;
        this.__progressInterval = window.setInterval(() => {
            this.__progressElem.style.width = pw + "%";
            pw++;
        }, 20);
    }
    private __endLoading() {
        document.body.classList.remove("bp-state-loading");
        document.body.classList.add("bp-state-loaded");

        var d = 300 - (Date.now() - this.__progressStart);
        if (d < 0)
            d = 0;

        this.__progressTimeout = window.setTimeout(() => {
            window.clearInterval(this.__progressInterval);
            this.__progressElem.style.width = "100%";

            this.__progressElem.classList.remove("show");

            this.__progressTimeout = window.setTimeout(() => {
                this.__progressElem.style.width = "0%";
            }, 200);
        }, d);
    }

    private _ctrlPressed: boolean = false;

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
                if (urlWithoutHash.toLowerCase() == this.__navigation.url.toLowerCase())
                    return;
            }
        }

        if (event.state) {
            var state = <PageNavState>event.state;
            this.nav({ url: state.url, hash: state.hash, pushState: false, scrollToTop: false });

            return;
        }
    }
    private __onHashChange(e: HashChangeEvent) {
        console.log("HashChange to " + e.newURL);
    }
    private __onClickAppLink(e: MouseEvent) {
        var elem = <HTMLElement>e.target;
        var ignore = false;
        while (true) {
            if (elem.hasAttribute("data-nav-ignore")) {
                ignore = true;
                break;
            }

            if (elem.classList.contains("applink"))
                break;
            if (elem === e.currentTarget)
                return;

            elem = elem.parentElement;
            if (elem === null)
                return true;
        }

        if (this._ctrlPressed)
            return true;

        if (elem.hasAttribute("target")) {
            let target = elem.getAttribute("target");
            if (target === "_blank")
                return true;
        }

        e.preventDefault();
        e.stopPropagation();
        e.returnValue = false;

        if (ignore)
            return false;

        this.navigate(elem);

        return false;
    }
    private __onKeyDownUp(e: KeyboardEvent) {
        this._ctrlPressed = e.ctrlKey;
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