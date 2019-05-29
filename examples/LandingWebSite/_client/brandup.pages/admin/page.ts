import { UIElement, DOM } from "brandup-ui";
import ContentPage from "../pages/content";
import { editPage } from "../dialogs/page-edit";
import { publishPage } from "../dialogs/page-publish";
import iconDiscard from "../svg/toolbar-button-discard.svg";
import iconEdit from "../svg/toolbar-button-edit.svg";
import iconPublish from "../svg/toolbar-button-publish.svg";
import iconSave from "../svg/toolbar-button-save.svg";
import iconSettings from "../svg/toolbar-button-settings.svg";
import iconSeo from "../svg/toolbar-button-seo.svg";
import { PageDesigner } from "../content/designer/page";

export class PageToolbar extends UIElement {
    private __designer: PageDesigner;

    get typeName(): string { return "BrandUpPages.PageToolbar"; }

    constructor(page: ContentPage) {
        super();

        page.attachDestroyFunc(() => { this.destroy(); }); 

        var toolbarElem = DOM.tag("div", { class: "bp-elem bp-toolbar bp-toolbar-right" });
        let isLoading = false;

        if (page.model.editId) {
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-content" }, iconSettings));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-commit" }, iconSave));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-discard" }, iconDiscard));

            let cancelNav = true;

            this.registerCommand("bp-content", () => {
                editPage(page.model.editId).then(() => {
                    page.app.reload();
                });
            });
            this.registerCommand("bp-commit", (elem: HTMLElement) => {
                if (isLoading)
                    return;
                isLoading = true;

                page.app.request({
                    urlParams: { handler: "CommitEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        cancelNav = false;
                        page.app.nav({ url: data, pushState: false });
                        isLoading = false;
                    }
                });
            });
            this.registerCommand("bp-discard", (elem: HTMLElement) => {
                if (isLoading)
                    return;
                isLoading = true;

                page.app.request({
                    urlParams: { handler: "DiscardEdit" },
                    method: "POST",
                    success: (data: string, status: number) => {
                        cancelNav = false;
                        page.app.nav({ url: data, pushState: false });
                        isLoading = false;
                    }
                });
            });

            this.__designer = new PageDesigner(page);

            window.addEventListener("pageNavigating", (e: CustomEvent) => {
                if (cancelNav) {
                    alert("cancel nav");
                    e.cancelBubble = true;
                    e.stopPropagation();
                    e.preventDefault();
                }
                else {
                    e.cancelBubble = false;
                }

                cancelNav = true;
            });
        }
        else {
            if (page.model.status !== "Published") {
                toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-publish" }, iconPublish));

                this.registerCommand("bp-publish", () => {
                    publishPage(page.model.id).then(result => {
                        page.app.nav({ url: result.url, pushState: false });
                    });
                });
            }

            //toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-seo" }, iconSeo));
            toolbarElem.appendChild(DOM.tag("button", { class: "bp-toolbar-button", "data-command": "bp-edit" }, iconEdit));

            this.registerCommand("bp-edit", () => {
                if (isLoading)
                    return;
                isLoading = true;

                page.app.request({
                    urlParams: { handler: "BeginEdit" },
                    method: "POST",
                    success: (data: BeginPageEditResult, status: number) => {
                        isLoading = false;

                        if (data.currentDate) {
                            let popup = DOM.tag("div", { class: "bp-toolbar-popup" }, [
                                DOM.tag("div", { class: "text" }, "Ранее вы не завершили редактирование этой страницы."),
                                DOM.tag("div", { class: "buttons" }, [
                                    DOM.tag("button", { "data-command": "continue-edit", "data-value": data.url }, "Продолжить"),
                                    DOM.tag("button", { "data-command": "restart-edit" }, "Начать заново")
                                ])
                            ])

                            this.element.appendChild(popup);

                            this.setPopup(popup);
                        }
                        else
                            page.app.nav({ url: data.url, pushState: false });
                    }
                });
            });

            this.registerCommand("continue-edit", (elem: HTMLElement) => {
                this.setPopup(null);

                let url = elem.getAttribute("data-value");
                page.app.nav({ url: url, pushState: false });
            });

            this.registerCommand("restart-edit", (elem: HTMLElement) => {
                this.setPopup(null);

                page.app.request({
                    urlParams: { handler: "BeginEdit", force: "true" },
                    method: "POST",
                    success: (data: BeginPageEditResult, status: number) => {
                        isLoading = false;
                        page.app.nav({ url: data.url, pushState: false });
                    }
                });
            });

            this.__closePopupFunc = (e: MouseEvent) => {
                let t = <HTMLElement>e.target;

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

        super.destroy();
    }
}

interface BeginPageEditResult {
    currentDate: string;
    url: string;
}