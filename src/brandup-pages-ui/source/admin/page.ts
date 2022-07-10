import { UIElement, DOM, AjaxResponse } from "brandup-ui";
import ContentPage from "../pages/content";
import { PageDesigner } from "../content/designer/page";
import { editPage } from "../dialogs/pages/edit";
import { publishPage } from "../dialogs/pages/publish";
import { seoPage } from "../dialogs/pages/seo";
import iconDiscard from "../svg/toolbar-button-discard.svg";
import iconEdit from "../svg/toolbar-button-edit.svg";
import iconPublish from "../svg/toolbar-button-publish.svg";
import iconSave from "../svg/toolbar-button-save.svg";
import iconSettings from "../svg/toolbar-button-settings.svg";
import iconSeo from "../svg/toolbar-button-seo.svg";

export class PageToolbar extends UIElement {
    private __designer: PageDesigner;
    //private __pageNavFunc: (e: CustomEvent) => void;

    get typeName(): string { return "BrandUpPages.PageToolbar"; }

    constructor(page: ContentPage) {
        super();

        //page.attachDestroyFunc(() => { this.destroy(); });

        const toolbarElem = DOM.tag("div", { class: "bp-elem bp-toolbar bp-toolbar-right" });
        let isLoading = false;

        if (page.model.editId) {
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-content", title: "Редактор контента" }, iconSettings));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-commit", title: "Применить изменения к странице" }, iconSave));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-discard", title: "Отменить изменения" }, iconDiscard));

            //let cancelNav = true;

            this.registerCommand("bp-content", () => {
                editPage(page.model.editId).then(() => {
                    page.website.app.reload();
                });
            });
            this.registerCommand("bp-commit", () => {
                if (isLoading)
                    return;
                isLoading = true;

                page.website.request({
                    url: "/brandup.pages/page/content/commit",
                    urlParams: { editId: page.model.editId },
                    method: "POST",
                    success: (response) => {
                        //cancelNav = false;

                        if (response.status !== 200)
                            throw "";

                        page.website.nav({ url: response.data, replace: true });
                        isLoading = false;
                    }
                }, true);
            });
            this.registerCommand("bp-discard", () => {
                if (isLoading)
                    return;
                isLoading = true;

                page.website.request({
                    url: "/brandup.pages/page/content/discard",
                    urlParams: { editId: page.model.editId },
                    method: "POST",
                    success: (response) => {
                        //cancelNav = false;

                        if (response.status !== 200)
                            throw "";

                        page.website.nav({ url: response.data, replace: true });
                        isLoading = false;
                    }
                });
            });

            this.__designer = new PageDesigner(page);

            //this.__pageNavFunc = (e: CustomEvent<NavigationOptions>) => {
            //    if (cancelNav && e.detail.pushState) {
            //        e.cancelBubble = true;
            //        e.stopPropagation();
            //        e.preventDefault();
            //    }
            //    else {
            //        e.cancelBubble = false;
            //    }

            //    cancelNav = true;
            //};

            //window.addEventListener("pageNavigating", this.__pageNavFunc, false);
        }
        else {
            if (page.model.status !== "Published") {
                toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-publish" }, iconPublish));

                this.registerCommand("bp-publish", () => {
                    publishPage(page.model.id).then(result => {
                        page.website.nav({ url: result.url, replace: true });
                    });
                });
            }

            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-seo", title: "Параметры индексирования страницы" }, iconSeo));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-edit", title: "Редактировать контент страницы" }, iconEdit));

            this.registerCommand("bp-edit", () => {
                if (isLoading)
                    return;
                isLoading = true;

                page.website.request({
                    url: "/brandup.pages/page/content/begin",
                    urlParams: { pageId: page.model.id },
                    method: "POST",
                    success: (response: AjaxResponse<BeginPageEditResult>) => {
                        isLoading = false;

                        if (response.status !== 200)
                            throw "";

                        if (response.data.currentDate) {
                            const popup = DOM.tag("div", { class: "bp-toolbar-popup" }, [
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
                            page.website.nav({ url: response.data.url, replace: true });
                    }
                });
            });
            this.registerCommand("bp-seo", () => {
                seoPage(page.model.id).then(() => {
                    page.website.app.reload();
                })
            });

            this.registerCommand("continue-edit", (elem: HTMLElement) => {
                this.setPopup(null);

                const url = elem.getAttribute("data-value");
                page.website.nav({ url: url, replace: true });
            });

            this.registerCommand("restart-edit", () => {
                this.setPopup(null);

                page.website.request({
                    url: "/brandup.pages/page/content/begin",
                    urlParams: { pageId: page.model.id, force: "true" },
                    method: "POST",
                    success: (response: AjaxResponse<BeginPageEditResult>) => {
                        isLoading = false;

                        if (response.status !== 200)
                            throw "";

                        page.website.nav({ url: response.data.url, replace: true });
                    }
                });
            });

            this.__closePopupFunc = (e: MouseEvent) => {
                const t = e.target as HTMLElement;

                if (!t.closest(".bp-toolbar-popup")) {
                    this.__popupElem.remove();
                    this.__popupElem = null;
                    document.body.removeEventListener("click", this.__closePopupFunc);
                }
            };
        }

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
    }

    private __popupElem: HTMLElement;
    private __closePopupFunc: (e: MouseEvent) => void;
    private setPopup(popup: HTMLElement) {
        if (this.__popupElem) {
            this.__popupElem.remove();
            this.__popupElem = null;
            document.body.removeEventListener("click", this.__closePopupFunc);
        }

        if (popup) {
            this.__popupElem = popup;
            document.body.addEventListener("click", this.__closePopupFunc);
        }
    }

    destroy() {
        if (this.__designer) {
            this.__designer.destroy();
            this.__designer = null;
        }

        this.element.remove();
        //window.removeEventListener("pageNavigating", this.__pageNavFunc, false);

        super.destroy();
    }
}

interface BeginPageEditResult {
    currentDate: string;
    url: string;
}