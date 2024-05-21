import { UIElement } from "brandup-ui";
import ContentPage from "../pages/content";
import { browserPage } from "../dialogs/pages/browser";
import iconBack from "../svg/toolbar-button-back.svg";
import iconTree from "../svg/toolbar-button-tree.svg";
import iconWebsite from "../svg/toolbar-button-website.svg";
import iconPublish from "../svg/toolbar-button-publish.svg";
import iconSeo from "../svg/toolbar-button-seo.svg";
import { listContentType } from "../dialogs/content-types/list";
import { Page, PageModel } from "brandup-ui-website";
import { DOM } from "brandup-ui-dom";
import { ajaxRequest, AjaxResponse } from "brandup-ui-ajax";
import iconList from "../svg/new/menu.svg";
import iconEdit from "../svg/new/edit.svg";
import iconPlus from "../svg/new/plus.svg";
import iconStructure from "../svg/new/structure.svg";
import { publishPage } from "../dialogs/pages/publish";
import { seoPage } from "../dialogs/pages/seo";

export class WebSiteToolbar extends UIElement {
    private __closeMenuFunc: (e: MouseEvent) => void;
    private __page: Page<PageModel>;
    private __isLoading: boolean;
    private __popupElem: HTMLElement;

    readonly isContentPage: boolean;

    get typeName(): string {
        return "BrandUpPages.WebSiteToolbar";
    }

    constructor(page: Page<PageModel>) {
        super();

        this.__page = page;
        this.__isLoading = false;
        document.body.classList.add("bp-state-toolbars");

        this.isContentPage = page instanceof ContentPage;

        this.__renderUI();
        this.__initLogic();
    }

    private __renderUI() {
        const menuItems = [
            // DOM.tag("a", { href: "", command: "bp-content-types" }, [iconPlus,"Добавить страницу"]),
            // DOM.tag("a", { href: "", command: "bp-page-types" }, [iconStructure,"Структура сайта"]),
            DOM.tag("a", { href: "", command: "bp-content-types" }, [iconWebsite, "Типы контента"]),
            DOM.tag("a", { href: "", command: "bp-seo" }, [iconSeo, "Индексирование страницы"]),
            DOM.tag("a", { href: "", command: "bp-pages" }, [iconList, "Страницы этого уровня"]),
        ];

        if (this.isContentPage && this.__page.model.parentPageId) {
            menuItems.push(DOM.tag("a", { href: "", command: "bp-back" }, [iconBack, "Перейти к родительской странице"]));
        }

        if (this.__page.model.status !== "Published") {
            menuItems.push(DOM.tag("a", { href: "", command: "bp-publish" }, [iconPublish, "Опубликовать"]));

            this.registerCommand("bp-publish", () => {
                publishPage(this.__page.model.id).then(result => {
                    this.__page.website.nav({ url: result.url, replace: true });
                });
            });
        }

        if (this.isContentPage) {
            menuItems.push(DOM.tag("a", { href: "", command: "bp-pages-child" }, [iconTree, "Дочерние страницы"]));
        }

        const widgetElem = DOM.tag("div", { class: "bp-elem bp-widget" }, [
            DOM.tag("button", { class: "bp-widget-button left", command: "bp-actions", title: "Действия" }, iconList),
            DOM.tag("button", { class: "bp-widget-button right", command: "bp-edit", title: "Редактировать контент страницы" }, iconEdit),
            DOM.tag("menu", { class: "bp-widget-menu" }, menuItems)
        ]);

        document.body.appendChild(widgetElem);
        this.setElement(widgetElem);
    }

    private __initLogic() {
        this.registerCommand("bp-back", () => {
            let parentPageId: string = null;
            if (this.isContentPage)
                parentPageId = this.__page.model.parentPageId;
            if (parentPageId) {
                ajaxRequest({
                    url: `/brandup.pages/page/${parentPageId}`,
                    success: (response: AjaxResponse<PageModel>) => {
                        this.__page.website.nav({ url: response.data.url });
                    }
                });
            }
        });

        this.registerCommand("bp-pages", () => {
            let parentPageId: string = null;
            if (this.isContentPage)
                parentPageId = this.__page.model.parentPageId;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-pages-child", () => {
            let parentPageId: string = null;
            if (this.isContentPage)
                parentPageId = this.__page.model.id;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-content-types", () => {
            this.element.classList.remove("opened-menu");
            listContentType();
        });

        this.registerCommand("bp-seo", () => {
            seoPage(this.__page.model.id).then(() => {
                this.__page.website.app.reload();
            });
        });

        this.registerCommand("bp-actions", () => {
            if (!this.element.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }

            document.body.addEventListener("click", this.__closeMenuFunc);
        });

        this.registerCommand("bp-edit", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.__page.website.request({
                url: "/brandup.pages/page/content/begin",
                urlParams: { pageId: this.__page.model.id },
                method: "POST",
                success: (response: AjaxResponse<BeginPageEditResult>) => {
                    this.__isLoading = false;

                    if (response.status !== 200)
                        throw "";

                    if (response.data.currentDate) {
                        const popup = DOM.tag("div", { class: "bp-widget-popup" }, [
                            DOM.tag("div", { class: "text" }, "Ранее вы не завершили редактирование этой страницы."),
                            DOM.tag("div", { class: "buttons" }, [
                                DOM.tag("button", { "data-command": "continue-edit", "data-value": response.data.url }, "Продолжить"),
                                DOM.tag("button", { "data-command": "restart-edit" }, "Начать заново")
                            ])
                        ])

                        this.element.appendChild(popup);

                        this.setPopup(popup);
                    }
                    else
                        this.__page.website.nav({ url: response.data.url, replace: true });
                }
            });
        });

        this.registerCommand("continue-edit", (elem: HTMLElement) => {
            this.setPopup(null);

            const url = elem.getAttribute("data-value");
            this.__page.website.nav({ url: url, replace: true });
        });

        this.registerCommand("restart-edit", () => {
            this.setPopup(null);

            this.__page.website.request({
                url: "/brandup.pages/page/content/begin",
                urlParams: { pageId: this.__page.model.id, force: "true" },
                method: "POST",
                success: (response: AjaxResponse<BeginPageEditResult>) => {
                    this.__isLoading = false;

                    if (response.status !== 200)
                        throw "";

                    this.__page.website.nav({ url: response.data.url, replace: true });
                }
            });
        });

        this.__closeMenuFunc = (e: MouseEvent) => {
            const target = e.target as Element;
            if (!target.closest(".bp-widget-menu")) {
                e.stopImmediatePropagation();
                this.element.classList.remove("opened-menu");
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }
        };
    }

    private __closePopupFunc (e: MouseEvent): void {
        const t = e.target as HTMLElement;

        if (!t.closest(".bp-toolbar-popup")) {
            this.__popupElem.remove();
            this.__popupElem = null;
            document.body.removeEventListener("click", this.__closePopupFunc);
        }
    };

    private setPopup(popup: HTMLElement) {
        if (this.__popupElem) {
            document.body.removeEventListener("click", this.__closePopupFunc);
        }

        if (popup) {
            this.__popupElem = popup;
            document.body.addEventListener("click", this.__closePopupFunc);
        }
    }

    destroy() {
        this.element.remove();

        super.destroy();
    }
}

export interface BeginPageEditResult {
    currentDate: string;
    url: string;
}