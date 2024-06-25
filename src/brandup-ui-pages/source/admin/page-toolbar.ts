﻿import { UIElement } from "brandup-ui";
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
import "../styles.less";

export class PageToolbar extends UIElement {
    private __closeMenuFunc: (e: MouseEvent) => void;
    private __page: Page<PageModel>;
    private __isLoading: boolean;
    private __popupElem: HTMLElement;

    readonly isContentPage: boolean;

    get typeName(): string {
        return "BrandUpPages.PageToolbar";
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
        const contentElements: HTMLElement[] = Array.from(DOM.queryElements(document.body, "[content-root]"));

        const websiteMenuItems = document.createDocumentFragment();
        websiteMenuItems.append(DOM.tag("a", { href: "", command: "bp-content-types" }, [iconPlus, "Типы контента"]));
        websiteMenuItems.append(DOM.tag("a", { href: "", command: "bp-pages" }, [iconList, "Страницы этого уровня"]));

        let websiteMenu: HTMLElement;

        let toolbarButtons = [
            DOM.tag("button", { class: "page-toolbar-button", command: "bp-actions-website", title: "Действия" }, [
                iconList,
                websiteMenu = DOM.tag("menu", { class: "page-toolbar-menu", id: "website-toolbar-menu" })
            ]),
            DOM.tag("button", { class: "page-toolbar-button", command: "bp-actions-edit", title: "Редактировать контент страницы" }, [
                iconEdit,
                DOM.tag("menu", { class: "page-toolbar-menu edit-menu", id: "edit-toolbar-menu" }, contentElements.map(elem => {
                    const contentKey = elem.getAttribute("content-root");
                    if (!contentKey) throw "Not set content root value.";

                    const contentType = elem.getAttribute("content-type");
                    if (!contentType) throw "Not set content type value.";

                    return DOM.tag("a", { href: "", command: "bp-edit", dataset: { contentKey, contentType } }, [
                        contentKey, DOM.tag('span', null, contentKey),
                    ])
                }))
            ])
        ];
        
        // Если страница динамическая
        if (this.isContentPage) {
            websiteMenuItems.append(DOM.tag("a", { href: "", command: "bp-pages-child" }, [iconDown, "Дочерние страницы"]));
            
            if (this.__page.model.parentPageId) {
                websiteMenuItems.append(DOM.tag("a", { href: "", command: "bp-back" }, [iconBack, "Перейти к родительской странице"]));
            }

            const pageMenuItems = [
                DOM.tag("a", { href: "", command: "bp-seo" }, [iconSeo, "Индексирование страницы"])
            ];

            const status = this.__page.model.status?.toLowerCase();
            const published = status === "published";

            if (!published) 
                pageMenuItems.push(DOM.tag("a", { href: "", command: "bp-pages" }, [iconPublish, "Опубликовать"]),);

            toolbarButtons = toolbarButtons.slice(0, 1).concat([
                    DOM.tag ("button", { class: "page-status page-toolbar-button " + status }, [DOM.tag("span"),]),
                    (published ? null : DOM.tag("button", { class: "page-toolbar-button", command: "bp-publish", title: "Опубликовать" }, iconPublish)),
                ],
                toolbarButtons.slice(1),
                DOM.tag("button", { class: "page-toolbar-button", command: "bp-actions-page", title: "Действия над страницей" }, [
                    iconMore,
                    DOM.tag("menu", { class: "page-toolbar-menu", id: "page-toolbar-menu", title: "" }, pageMenuItems),
                ])
            );
        }
        websiteMenu.append(websiteMenuItems);

        const toolbarElem = DOM.tag("div", { class: "bp-elem page-toolbar" }, toolbarButtons);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
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

        this.registerCommand("bp-actions-edit", (elem) => {
            if (elem.closest(".page-toolbar-menu")) this.__closeMenu();

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

            const contentKey = context.target.dataset["contentKey"];
            const contentType = context.target.dataset["contentType"];
            if (!contentKey || !contentType)
                throw "Not set content edit parameters.";
                
            this.__page.website.request({
                url: "/brandup.pages/page/content/begin",
                urlParams: { key: contentKey, type: contentType },
                method: "POST",
                success: (response: AjaxResponse<BeginPageEditResult>) => {
                    this.__isLoading = false;

                    if (response.status !== 200) {
                        context.complate();
                        throw "Error begin content edit.";
                    }

                    if (response.data.currentDate) {
                        const popup = DOM.tag("div", { class: "page-toolbar-popup" }, [
                            DOM.tag("div", { class: "text" }, "Ранее вы не завершили редактирование этой страницы."),
                            DOM.tag("div", { class: "buttons" }, [
                                DOM.tag("button", { "data-command": "continue-edit", dataset: { editId: response.data.editId, contentKey } }, "Продолжить"),
                                DOM.tag("button", { "data-command": "restart-edit", dataset: { contentKey, contentType } }, "Начать заново")
                            ])
                        ])

                        this.element.appendChild(popup);

                        this.setPopup(popup);
                    }
                    else {
                        // редирект на страницу редактирования контента
                        this.__navToEdit(response.data.editId, contentKey);
                    }

                    context.complate();
                },
            });
        })

        this.registerCommand("continue-edit", (elem: HTMLElement) => {
            this.setPopup(null);

            const editId = elem.dataset["editId"];
            const contentKey = elem.dataset["contentKey"];
            this.__navToEdit(editId, contentKey);
        });

        this.registerCommand("restart-edit", (elem: HTMLElement) => {
            this.setPopup(null);
            const contentKey = elem.dataset["contentKey"];
            const contentType = elem.dataset["contentType"];

            this.__page.website.request({
                url: "/brandup.pages/page/content/begin",
                urlParams: { key: contentKey, type: contentType, force: "true" },
                method: "POST",
                success: (response: AjaxResponse<BeginPageEditResult>) => {
                    this.__isLoading = false;

                    if (response.status !== 200)
                        throw "Error begin content edit.";
                        
                    this.__navToEdit(response.data.editId, contentKey);
                }
            });
        });

        this.__closeMenuFunc = (e: MouseEvent) => {
            const target = e.target as Element;
            if (target.closest(".page-toolbar-menu")) return;

            const button = target.closest(`.page-toolbar-button`);
            if (button) {
                const menuName = button.getAttribute('data-command')?.replace('bp-actions-',"");
                this.__closeMenu(menuName);
            }
            else {
                this.element.classList.remove("opened-menu-website", "opened-menu-page", "opened-menu-edit");
            }
            document.body.removeEventListener("click", this.__closeMenuFunc);
            return;
        };
    }

    private __navToEdit(editId: string, contentKey: string) {
        const contentElem = DOM.queryElement(document.body, `[content-root='${contentKey}']`);

        contentElem.dataset["contentEditId"] = editId;

        this.__page.website.nav({ url: this.__page.buildUrl({ editid: editId }), replace: true });
    }

    private __closeMenu(menuName?: string) {
        let className = "";
        if (menuName) className = `opened-menu-${menuName}`;
        this.element.classList.forEach((value: string) => {
            if (value.indexOf('opened-menu-') >= 0 && value !== className)
                this.element.classList.remove(value);
        })
    }

    private __closePopupFunc (e: MouseEvent): void {
        const t = e.target as HTMLElement;

        if (!t.closest(".bp-toolbar-popup")) {
            this.__popupElem?.remove();
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
    editId: string;
    currentDate: string;
    url: string;
}