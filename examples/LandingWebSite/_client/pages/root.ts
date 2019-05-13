import "./styles.less";
import { DOM, UIElement, ajaxRequest } from "brandup-ui";
import { listPageCollection } from "./dialogs/page-collection-list";
import { publishPage } from "./dialogs/page-publish";
import { editPage } from "./dialogs/page-edit";
import { PageDesigner } from "./designer/page";
import ContentPage from "./pages/content";

class BrandUpPages {
    readonly page: ContentPage;

    constructor(page: ContentPage) {
        this.page = page;

        this.__renderToolbars();
    }
    
    static load(page: ContentPage) {
        new BrandUpPages(page);
    }

    private __renderToolbars() {
        if (this.page.model.editId) {
            new PageToolbar(this);
            new PageDesigner(this.page.model.editId);
        }
        else {
            new WebSiteToolbar(this);
        }
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
            listPageCollection(p.page.model.parentPageId);
        });

        this.registerCommand("brandup-pages-publish", () => {
            publishPage(p.page.model.id).then(result => {
                location.href = result.url;
            });
        });

        this.registerCommand("brandup-pages-edit", () => {
            p.page.app.request({
                urlParams: { handler: "BeginEdit" },
                method: "POST",
                success: (data: string, status: number) => {
                    if (status === 200)
                        p.page.app.nav({ url: data, pushState: false });
                    else
                        throw "";
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
            editPage(p.page.model.editId).then(() => {
                location.reload();
            });
        });

        this.registerCommand("brandup-pages-commit", () => {
            p.page.app.request({
                urlParams: { handler: "CommitEdit" },
                method: "POST",
                success: (data: string, status: number) => {
                    p.page.app.nav({ url: data, pushState: false });
                }
            });
        });

        this.registerCommand("brandup-pages-discard", () => {
            p.page.app.request({
                urlParams: { handler: "DiscardEdit" },
                method: "POST",
                success: (data: string, status: number) => {
                    p.page.app.nav({ url: data, pushState: false });
                }
            });
        });
    }
}

export default BrandUpPages;