import { UIElement, DOM, ajaxRequest } from "brandup-ui";
import Page from "../pages/page";
import ContentPage from "../pages/content";
import iconBack from "../svg/toolbar-button-back.svg";
import iconList from "../svg/toolbar-button-list.svg";
import iconTree from "../svg/toolbar-button-tree.svg";
import iconWebsite from "../svg/toolbar-button-website.svg";
import { browserPage } from "../dialogs/page-browser";
import { listEditor } from "../dialogs/editors/list";

export class WebSiteToolbar extends UIElement {
    private __closeMenuFunc: (e: MouseEvent) => void;

    get typeName(): string { return "BrandUpPages.WebSiteToolbar"; }

    constructor(page: Page<any>) {
        super();

        var isContentPage = page instanceof ContentPage;
        var buttons: Array<HTMLElement> = [];

        if (isContentPage && (<ContentPage>page).model.parentPageId) {
            buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-back", title: "Перейти к родительской странице" }, iconBack));
        }

        buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-website", title: "Web-сайт" }, iconWebsite));
        buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-pages", title: "Страницы этого уровня" }, iconList));

        if (isContentPage) {
            buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-pages-child", title: "Дочерние страницы" }, iconTree));
        }

        var toolbarElem = DOM.tag("div", { class: "bp-elem bp-toolbar" }, buttons);

        toolbarElem.appendChild(DOM.tag("div", { class: "bp-toolbar-menu" }, [
            DOM.tag("a", { href: "", "data-command": "bp-editors" }, "Редакторы контента"),
            DOM.tag("a", { href: "", "data-command": "bp-recyclebin" }, "Корзина")
        ]))

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.registerCommand("bp-back", () => {
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

        this.registerCommand("bp-pages", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = (<ContentPage>page).model.parentPageId;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-pages-child", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = (<ContentPage>page).model.id;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-website", () => {
            if (!toolbarElem.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }

            document.body.addEventListener("click", this.__closeMenuFunc);
        });

        this.registerCommand("bp-editors", () => {
            toolbarElem.classList.remove("opened-menu");
            listEditor();
        });

        this.__closeMenuFunc = (e: MouseEvent) => {
            let target = <Element>e.target;
            if (!target.closest(".bp-toolbar-menu")) {
                toolbarElem.classList.remove("opened-menu");
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }
        };
    }

    destroy() {
        this.element.remove();

        super.destroy();
    }
}