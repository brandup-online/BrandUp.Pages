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
        const contentElements: HTMLElement[] = Array.from(DOM.queryElements(document.body, "[data-content-root]"));

        const websiteMenuItems = document.createDocumentFragment();
        websiteMenuItems.append(DOM.tag("a", { href: "", command: "bp-content-types" }, [iconPlus, "Типы контента"]));
        websiteMenuItems.append(DOM.tag("a", { href: "", command: "bp-pages" }, [iconList, "Страницы этого уровня"]));

        let websiteMenu: HTMLElement;

        let toolbarButtons = [
            DOM.tag("div", {class: "first-button"}, [
                DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Действия" }, iconList),
                websiteMenu = DOM.tag("menu", { class: "bp-page-toolbar-menu" }),
            ]),
            DOM.tag("div", null, [
                DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Редактировать контент страницы" }, iconEdit),
                DOM.tag("menu", { class: "bp-page-toolbar-menu edit-menu" }, contentElements.map(elem => {
                    const contentKey = elem.getAttribute("data-content-root");
                    if (!contentKey) throw "Not set content root value.";
    
                    const contentType = elem.getAttribute("data-content-type");
                    if (!contentType) throw "Not set content type value.";

                    const contentTitle = elem.getAttribute("data-content-title");
                    if (!contentType) throw "Not set content type value.";

                    return DOM.tag("a", { href: "", command: "bp-edit", dataset: { contentKey, contentType } }, [
                        contentTitle, DOM.tag('span', null, contentKey),
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
                pageMenuItems.push(DOM.tag("a", { href: "", command: "bp-pages" }, [iconPublish, "Опубликовать"]));

                toolbarButtons = toolbarButtons.slice(0, 1).concat([
                    DOM.tag("div", null, DOM.tag("button", { class: "bp-page-toolbar-button page-status " + status }, [DOM.tag("span"),])),
                    (published ? null : DOM.tag("div", null, DOM.tag("button", { class: "bp-page-toolbar-button", command: "bp-publish", title: "Опубликовать" }, iconPublish))),
                ],
                toolbarButtons.slice(1),
                DOM.tag("div", null, [
                    DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Действия над страницей" }, iconMore),
                    DOM.tag("menu", { class: "bp-page-toolbar-menu", title: "" }, pageMenuItems),
                ]),
            );
        }
        websiteMenu.append(websiteMenuItems);

        const toolbarElem = DOM.tag("div", { class: "bp-elem bp-page-toolbar" }, toolbarButtons);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
    }

    private __initLogic() {
        this.registerCommand("show-menu", (elem) => {
            if (!elem.classList.toggle("active")) {
                document.body.removeEventListener("click", this.__closeMenuFunc);
                return;
            }

            document.body.addEventListener("click", this.__closeMenuFunc);
        });

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

        const published = this.__page.model.status?.toLowerCase() === "published";

        if (!published)
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
                        const popup = DOM.tag("div", { class: "bp-page-toolbar-popup" }, [
                            DOM.tag("div", { class: "text" }, "Ранее вы не завершили редактирование этой страницы."),
                            DOM.tag("div", { class: "buttons" }, [
                                DOM.tag("button", { command: "continue-edit", dataset: { editId: response.data.editId, contentKey, content: JSON.stringify(response.data.content) } }, "Продолжить"), // TODO подумать над передачей контента в команду
                                DOM.tag("button", { command: "restart-edit", dataset: { contentKey, contentType } }, "Начать заново")
                            ])
                        ])

                        this.element.appendChild(popup);

                        this.setPopup(popup);
                    }
                    else {
                        // редирект на страницу редактирования контента
                        this.__navToEdit(response.data.editId, contentKey, response.data.content);
                    }

                    context.complate();
                },
            });
        })

        this.registerCommand("continue-edit", (elem: HTMLElement) => {
            this.setPopup(null);

            const editId = elem.dataset["editId"];
            const contentKey = elem.dataset["contentKey"];
            const content = JSON.parse(elem.dataset.content);
            this.__navToEdit(editId, contentKey, content);
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
                        
                    this.__navToEdit(response.data.editId, contentKey, response.data.content);
                }
            });
        });

        this.__closeMenuFunc = (e: MouseEvent) => {
            const target = e.target as Element;
            if (target.closest(".bp-page-toolbar-menu")) return;

            const button = target.closest(`.bp-page-toolbar-button`);
            const buttons = DOM.queryElements(this.element, '.bp-page-toolbar-button');
            buttons.forEach(item => {
                if (button && item === button) return; 
                else item.classList.remove("active");
            })
            document.body.removeEventListener("click", this.__closeMenuFunc);
        };
    }

    private __navToEdit(editId: string, contentKey: string, content: IContentModel[]) {
        const contentElem = DOM.queryElement(document.body, `[data-content-root='${contentKey}']`);

        contentElem.dataset["contentEditId"] = editId;

        this.__page.website.nav({ url: this.__page.buildUrl({ editid: editId }), replace: true, context: { content } });
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
    content: IContentModel[];
}

export interface IContentModel {
    parent: string;
    path: string;
    index: number;
    typeName: string;
    typeTitle: string;
    fields: IField[];
}

export interface IField {
    type: string;
    name: string;
    title: string;
    isRequired: boolean;
    value: object;
}