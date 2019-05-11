import "./styles.less";
import { DOM, UIElement, ajaxRequest } from "brandup-ui";
import { listPageCollection } from "./dialogs/page-collection-list";
import { publishPage } from "./dialogs/page-publish";
import { editPage } from "./dialogs/page-edit";

class BrandUpPages {
    private __pageModel: PageNavigationModel;

    get page(): PageNavigationModel { return this.__pageModel; }

    load() {
        ajaxRequest({
            urlParams: { handler: "navigate" },
            success: (data: PageNavigationModel, status: number) => {
                if (status !== 200)
                    throw "";

                this.__pageModel = data;

                this.__renderToolbars();
                this.__registerCommands();
            }
        });
    }

    private __renderToolbars() {
        if (this.__pageModel.editId) {
            new PageToolbar(this);
        }
        else {
            new WebSiteToolbar(this);
        }
    }

    private __registerCommands() {
    }
}

class WebSiteToolbar extends UIElement {
    get typeName(): string { return "BrandUpPages.WebSiteToolbar"; }

    constructor(p: BrandUpPages) {
        super();

        var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar" }, [
            DOM.tag("button", { class: "brandup-pages-toolbar-button list", "data-command": "brandup-pages-collections" }),
            DOM.tag("button", { class: "brandup-pages-toolbar-button publish", "data-command": "brandup-pages-publish" }),
            DOM.tag("button", { class: "brandup-pages-toolbar-button edit", "data-command": "brandup-pages-edit" })
        ]);
        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.registerCommand("brandup-pages-collections", () => {
            listPageCollection(p.page.parentPageId);
        });

        this.registerCommand("brandup-pages-publish", () => {
            publishPage(p.page.id).then(result => {
                location.href = result.url;
            });
        });

        this.registerCommand("brandup-pages-edit", () => {
            ajaxRequest({
                urlParams: { handler: "BeginEdit" },
                method: "POST",
                success: (data: string, status: number) => {
                    location.href = data;
                }
            });
        });
    }
}

class PageToolbar extends UIElement {
    get typeName(): string { return "BrandUpPages.PageToolbar"; }

    constructor(p: BrandUpPages) {
        super();

        var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar" }, [
            DOM.tag("button", { class: "brandup-pages-toolbar-button edit", "data-command": "brandup-pages-edit" }),
            DOM.tag("button", { class: "brandup-pages-toolbar-button save", "data-command": "brandup-pages-commit" }),
            DOM.tag("button", { class: "brandup-pages-toolbar-button discard", "data-command": "brandup-pages-discard" })
        ]);
        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.registerCommand("brandup-pages-edit", () => {
            editPage(p.page.editId).then(() => {
                location.reload();
            });
        });

        this.registerCommand("brandup-pages-commit", () => {
            ajaxRequest({
                urlParams: { handler: "CommitEdit" },
                method: "POST",
                success: (data: string, status: number) => {
                    location.href = data;
                }
            });
        });

        this.registerCommand("brandup-pages-discard", () => {
            ajaxRequest({
                urlParams: { handler: "DiscardEdit" },
                method: "POST",
                success: (data: string, status: number) => {
                    location.href = data;
                }
            });
        });
    }
}

var current: BrandUpPages = null;
const initialize = () => {
    if (!current && document.readyState === "complete") {
        current = new BrandUpPages();
    }
}

document.addEventListener("readystatechange", initialize);
initialize();

window.addEventListener("load", () => {
    current.load();
});