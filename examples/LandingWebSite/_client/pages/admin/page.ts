import { UIElement, DOM } from "brandup-ui";
import ContentPage from "../pages/content";
import { editPage } from "../dialogs/page-edit";
import { publishPage } from "../dialogs/page-publish";

export class PageToolbar extends UIElement {
    get typeName(): string { return "BrandUpPages.PageToolbar"; }

    constructor(page: ContentPage) {
        super();

        var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar brandup-pages-toolbar-right" });

        if (page.model.editId) {
            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button edit", "data-command": "brandup-pages-content" }));
            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button save", "data-command": "brandup-pages-commit" }));
            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button discard", "data-command": "brandup-pages-discard" }));

            this.registerCommand("brandup-pages-content", () => {
                editPage(page.model.editId).then(() => {
                    page.app.reload();
                });
            });
            this.registerCommand("brandup-pages-commit", () => {
                page.app.request({
                    urlParams: { handler: "CommitEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        page.app.nav({ url: data, pushState: false });
                    }
                });
            });
            this.registerCommand("brandup-pages-discard", () => {
                page.app.request({
                    urlParams: { handler: "DiscardEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        page.app.nav({ url: data, pushState: false });
                    }
                });
            });
        }
        else {
            if (page.model.status !== "Published") {
                toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button publish", "data-command": "brandup-pages-publish" }));

                this.registerCommand("brandup-pages-publish", () => {
                    publishPage(page.model.id).then(result => {
                        location.href = result.url;
                    });
                });
            }

            toolbarElem.appendChild(DOM.tag("button", { class: "brandup-pages-toolbar-button edit", "data-command": "brandup-pages-edit" }));
            this.registerCommand("brandup-pages-edit", () => {
                page.app.request({
                    urlParams: { handler: "BeginEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        if (status === 200)
                            page.app.nav({ url: data, pushState: false });
                        else
                            throw "";
                    }
                });
            });
        }
        
        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
    }

    destroy() {
        this.element.remove();

        super.destroy();
    }
}