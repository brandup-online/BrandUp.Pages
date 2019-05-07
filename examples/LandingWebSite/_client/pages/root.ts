import "./styles.less";
import { DOM, UIElement, ajaxRequest } from "brandup-ui";
import { listPageCollection } from "./dialogs/page-collection-list";
import { publishPage } from "./dialogs/page-publish";

class BrandUpPages extends UIElement {
    private __pageModel: PageNavigationModel;

    get typeName(): string { return "BrandUpPages.Toolbar"; }

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

        }
        else {
            var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar" }, [
                DOM.tag("button", { class: "brandup-pages-toolbar-button list", "data-command": "brandup-pages-collections" }),
                DOM.tag("button", { class: "brandup-pages-toolbar-button publish", "data-command": "brandup-pages-publish" }),
                DOM.tag("button", { class: "brandup-pages-toolbar-button edit", "data-command": "brandup-pages-edit" })
            ]);
            document.body.appendChild(toolbarElem);
            this.setElement(toolbarElem);
        }
    }

    private __registerCommands() {
        this.registerCommand("brandup-pages-collections", () => {
            listPageCollection(this.__pageModel.parentPageId);
        });

        this.registerCommand("brandup-pages-publish", () => {
            publishPage(this.__pageModel.id).then(result => {
                location.href = result.url;
            });
        });

        this.registerCommand("brandup-pages-edit", () => {
            ajaxRequest({
                url: `/brandup.pages/page/${this.__pageModel.id}/edit`,
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