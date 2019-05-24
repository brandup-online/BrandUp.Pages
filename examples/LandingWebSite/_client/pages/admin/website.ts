import { UIElement, DOM, ajaxRequest } from "brandup-ui";
import Page from "../pages/page";
import ContentPage from "../pages/content";
import iconBack from "../svg/toolbar-button-back.svg";
import iconList from "../svg/toolbar-button-list.svg";
import iconTree from "../svg/toolbar-button-tree.svg";
import { browserPage } from "../dialogs/page-browser";

export class WebSiteToolbar extends UIElement {
    get typeName(): string { return "BrandUpPages.WebSiteToolbar"; }

    constructor(page: Page<any>) {
        super();

        var isContentPage = page instanceof ContentPage;
        var buttons: Array<HTMLElement> = [];

        if (isContentPage && (<ContentPage>page).model.parentPageId) {
            buttons.push(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-back" }, iconBack));
        }

        buttons.push(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-collections" }, iconList));

        if (isContentPage) {
            buttons.push(DOM.tag("button", { class: "brandup-pages-toolbar-button", "data-command": "brandup-pages-collections2" }, iconTree));
        }

        var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar" }, buttons);
        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.registerCommand("brandup-pages-back", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = (<ContentPage>page).model.parentPageId;
            if (parentPageId) {
                ajaxRequest({
                    url: `/brandup.pages/page/${parentPageId}`,
                    success: (pageModel: PageModel) => {
                        page.app.navigate(pageModel.url);
                    }
                });
            }
        });

        this.registerCommand("brandup-pages-collections", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = (<ContentPage>page).model.parentPageId;
            browserPage(parentPageId);
        });

        this.registerCommand("brandup-pages-collections2", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = (<ContentPage>page).model.id;
            browserPage(parentPageId);
        });
    }

    destroy() {
        this.element.remove();

        super.destroy();
    }
}