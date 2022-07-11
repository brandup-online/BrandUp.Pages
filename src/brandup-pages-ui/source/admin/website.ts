import { UIElement } from "brandup-ui";
import ContentPage from "../pages/content";
import { browserPage } from "../dialogs/pages/browser";
import iconBack from "../svg/toolbar-button-back.svg";
import iconList from "../svg/toolbar-button-list.svg";
import iconTree from "../svg/toolbar-button-tree.svg";
import iconWebsite from "../svg/toolbar-button-website.svg";
import { listContentType } from "../dialogs/content-types/list";
import { Page, PageModel } from "brandup-ui-website";
import { DOM } from "brandup-ui-dom";
import { ajaxRequest, AjaxResponse } from "brandup-ui-ajax";

export class WebSiteToolbar extends UIElement {
    private __closeMenuFunc: (e: MouseEvent) => void;

    get typeName(): string { return "BrandUpPages.WebSiteToolbar"; }

    constructor(page: Page<PageModel>) {
        super();

        document.body.classList.add("bp-state-toolbars");

        const isContentPage = page instanceof ContentPage;
        const buttons: Array<HTMLElement> = [];

        if (isContentPage && page.model.parentPageId) {
            buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-back", title: "Перейти к родительской странице" }, iconBack));
        }

        buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-website", title: "Web-сайт" }, iconWebsite));
        buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-pages", title: "Страницы этого уровня" }, iconList));

        if (isContentPage) {
            buttons.push(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-pages-child", title: "Дочерние страницы" }, iconTree));
        }

        const toolbarElem = DOM.tag("div", { class: "bp-elem bp-toolbar" }, buttons);

        toolbarElem.appendChild(DOM.tag("div", { class: "bp-toolbar-menu" }, [
            DOM.tag("a", { href: "", "data-command": "bp-content-types" }, "Типы контента"),
            //DOM.tag("a", { href: "", "data-command": "bp-page-types" }, "Типы страниц"),
            //DOM.tag("a", { href: "", "data-command": "bp-recyclebin" }, "Корзина")
        ]))

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.registerCommand("bp-back", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = page.model.parentPageId;
            if (parentPageId) {
                ajaxRequest({
                    url: `/brandup.pages/page/${parentPageId}`,
                    success: (response: AjaxResponse<PageModel>) => {
                        page.website.nav({ url: response.data.url });
                    }
                });
            }
        });

        this.registerCommand("bp-pages", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = page.model.parentPageId;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-pages-child", () => {
            let parentPageId: string = null;
            if (isContentPage)
                parentPageId = page.model.id;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-website", () => {
            if (!toolbarElem.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }

            document.body.addEventListener("click", this.__closeMenuFunc);
        });

        this.registerCommand("bp-content-types", () => {
            toolbarElem.classList.remove("opened-menu");
            listContentType();
        });

        this.__closeMenuFunc = (e: MouseEvent) => {
            const target = e.target as Element;
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