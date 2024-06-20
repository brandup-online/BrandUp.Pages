import { UIElement } from "brandup-ui";
import ContentPage from "../pages/content";
import { browserPage } from "../dialogs/pages/browser";
import iconBack from "../svg/toolbar-button-back.svg";
import iconDown from "../svg/new/arrow-down.svg";
import iconPublish from "../svg/new/upload.svg";
import iconSeo from "../svg/new/increase.svg";
import { listContentType } from "../dialogs/content-types/list";
import { Page, PageModel } from "brandup-ui-website";
import { DOM } from "brandup-ui-dom";
import { ajaxRequest, AjaxResponse } from "brandup-ui-ajax";
import iconList from "../svg/new/menu.svg";
import iconEdit from "../svg/new/edit.svg";
import iconMore from "../svg/new/more.svg";
import iconPlus from "../svg/new/plus.svg";
import iconStructure from "../svg/new/structure.svg";
import { publishPage } from "../dialogs/pages/publish";
import { seoPage } from "../dialogs/pages/seo";

export const statickContentMock = [ // TODO имитация ответа от сервера. Удалить после появления api.
    { title: 'Текущая страница', key: '1' },
    { title: 'Блок 1', key: '2' },
    { title: 'Блок 2', key: '3' },
    { title: 'Блок 3', key: '4' },
    { title: 'Блок 4', key: '5' },
    { title: 'Блок 5', key: '6' },
    { title: 'Блок 6', key: '7' },
    { title: 'Блок 7', key: '8' },
]

export class EditorWidget extends UIElement {
    private __closeMenuFunc: (e: MouseEvent) => void;
    private __page: Page<PageModel>;
    private __isLoading: boolean;
    private __popupElem: HTMLElement;
    private __editMenuElem: HTMLElement;

    readonly isContentPage: boolean;

    get typeName(): string {
        return "BrandUpPages.EditorWidget";
    }

    constructor(page: Page<PageModel>) {
        super();
        statickContentMock[0].key = page.model.id || statickContentMock[0].key;

        this.__page = page;
        this.__isLoading = false;
        document.body.classList.add("bp-state-toolbars");

        this.isContentPage = page instanceof ContentPage;

        this.__renderUI();
        this.__initLogic();
    }

    private __renderUI() {
        const isDynamic = !!this.__page.model.id; //TODO Определяем тип статичная страница или нет. Поменять по готовности бека
        const websiteMenuItems = [
            DOM.tag("a", { href: "", command: "bp-content-types" }, [iconPlus, "Типы контента"]),
            DOM.tag("a", { href: "", command: "bp-pages" }, [iconList, "Страницы этого уровня"]),
        ]

        if (this.isContentPage && this.__page.model.parentPageId) {
            websiteMenuItems.push(DOM.tag("a", { href: "", command: "bp-back" }, [iconBack, "Перейти к родительской странице"]));
        }

        if (this.isContentPage) {
            websiteMenuItems.push(DOM.tag("a", { href: "", command: "bp-pages-child" }, [iconDown, "Дочерние страницы"]));
        }

        let widgetButtons = [
            DOM.tag("button", { class: "bp-widget-button", command: "bp-actions-website", title: "Действия" }, [
                iconList,
                DOM.tag("menu", { class: "bp-widget-menu", id: "website-widget-menu", title: "" }, websiteMenuItems),
            ]),
            DOM.tag("button", { class: "bp-widget-button", command: "bp-actions-edit", title: "Редактировать контент страницы" }, [
                iconEdit,
                this.__editMenuElem = DOM.tag("menu", { class: "bp-widget-menu edit-menu", id: "edit-widget-menu", title: "" }),
            ]),
        ]
        
        if (isDynamic) {
            const pageMenuItems = [
                DOM.tag("a", { href: "", command: "bp-seo" }, [iconSeo, "Индексирование страницы"]),
                DOM.tag("a", { href: "", command: "bp-actions-edit" }, [iconEdit, "Редактировать страницу"]),
            ];
            const status = this.__page.model.status?.toLowerCase();
            const published = status === "published";

            if (!published) 
                pageMenuItems.push(DOM.tag("a", { href: "", command: "bp-pages" }, [iconPublish, "Опубликовать"]),);

            widgetButtons = widgetButtons.slice(0, 1).concat([
                    DOM.tag ("button", { class: "page-status bp-widget-button " + status }, [DOM.tag("span"),]),
                    (published ? null : DOM.tag("button", { class: "bp-widget-button", command: "bp-publish", title: "Опубликовать" }, iconPublish)),
                ],
                widgetButtons.slice(1),
                DOM.tag("button", { class: "bp-widget-button", command: "bp-actions-page", title: "Действия над страницей" }, [
                    iconMore,
                    DOM.tag("menu", { class: "bp-widget-menu", id: "page-widget-menu", title: "" }, pageMenuItems),
                ])
            );
        }

        const widgetElem = DOM.tag("div", { class: "bp-elem bp-widget" }, widgetButtons);

        const fragment = document.createDocumentFragment();
        statickContentMock.forEach(item=> fragment.appendChild(DOM.tag("a", { href: "", "data-key": item.key, command: "bp-edit" }, [
            item.title, DOM.tag('span', null, item.key),
        ])));
        
        this.__editMenuElem.appendChild(fragment);

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

        if(this.__page.model.status !== "Published")
            this.registerCommand("bp-publish", () => {
                publishPage(this.__page.model.id).then(result => {
                    this.__page.website.nav({ url: result.url, replace: true });
                });
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

        this.registerCommand("bp-actions-website", () => {
            if (!this.element.classList.toggle("opened-menu-website")) {
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }

            document.body.addEventListener("click", this.__closeMenuFunc);
        });

        this.registerCommand("bp-actions-page", () => {
            if (!this.element.classList.toggle("opened-menu-page")) {
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }

            document.body.addEventListener("click", this.__closeMenuFunc);
        });

        this.registerCommand("bp-actions-edit", () => {
            if (!this.element.classList.toggle("opened-menu-edit")) {
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }
            document.body.addEventListener("click", this.__closeMenuFunc);
        });

        this.registerAsyncCommand("bp-edit", (context)=> {
            if (this.__isLoading)
                return;
            this.__isLoading = true;
            const key = context.target.getAttribute("data-key");

            this.__page.website.request({
                url: "/brandup.pages/page/content/begin",
                urlParams: { pageId: key }, // TODO Поменять pageId на другой параметр под api
                method: "POST",
                success: (response: AjaxResponse<BeginPageEditResult>) => {
                    this.__isLoading = false;

                    if (response.status !== 200) {
                        context.complate();
                        throw "";
                    }

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
                    context.complate();
                },
            });
        })

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
            if (target.closest(".bp-widget-menu")) return;

            const button = target.closest(`.bp-widget-button`);
            if (button) {
                const menuName = button.getAttribute('data-command')?.replace('bp-actions-',"");
                this.element.classList.forEach((value: string) => {
                    if (value.indexOf('opened-menu-') >= 0 && value !== `opened-menu-${menuName}`)
                        this.element.classList.remove(value);
                })
            }
            else {
                this.element.classList.remove("opened-menu-website", "opened-menu-page", "opened-menu-edit");
            }
            document.body.removeEventListener("click", this.__closeMenuFunc);
            return;
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
        document.body.removeEventListener("click", this.__closeMenuFunc);
        document.body.removeEventListener("click", this.__closePopupFunc);
        this.element.remove();

        super.destroy();
    }
}

export enum PageStatus {
    "published" = "Опубликовано",
    "draft" = "Черновик",
}

export interface BeginPageEditResult {
    currentDate: string;
    url: string;
}