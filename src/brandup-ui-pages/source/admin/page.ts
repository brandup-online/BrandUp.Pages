import { UIElement } from "brandup-ui";
import ContentPage from "../pages/content";
import { PageDesigner } from "../content/designer/page";
import { editPage } from "../dialogs/pages/edit";
import { DOM } from "brandup-ui-dom";
import { Page, PageModel } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";

export class PageToolbar extends UIElement {
    private __designer: PageDesigner;
    private __isLoading = false;

    readonly __page: Page<PageModel>;

    get typeName(): string { return "BrandUpPages.PageToolbar"; }

    constructor(page: ContentPage) {
        super();
        this.__page = page;
        this.__designer = new PageDesigner(page);

        this.__initLogic();
        this.__renderUI();
    }

    private __renderUI() {
        const toolbarElem = DOM.tag("div", { class: "bp-elem bp-toolbar" }, [
            DOM.tag("button", { class: "bp-button", command: "bp-commit" }, "Сохранить"),
            DOM.tag("button", { class: "bp-button secondary red", command: "bp-discard", title: "Редактор контента" }, "Отмена"),
            DOM.tag("button", { class: "bp-button neutral right", command: "bp-content" }, [editBlockIcon, "Контент"]),
        ]);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
    }

    private __initLogic() {
        this.registerCommand("bp-content", () => {
            editPage(this.__page.model.editId).then(() => {
                this.__page.website.app.reload();
            });
        });

        this.registerCommand("bp-commit", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.__page.website.request({
                url: "/brandup.pages/page/content/commit",
                urlParams: { editId: this.__page.model.editId },
                method: "POST",
                success: (response) => {
                    //cancelNav = false;

                    if (response.status !== 200)
                        throw "";

                    this.__page.website.nav({ url: response.data, replace: true });
                    this.__isLoading = false;
                }
            }, true);
        });

        this.registerCommand("bp-discard", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.__page.website.request({
                url: "/brandup.pages/page/content/discard",
                urlParams: { editId: this.__page.model.editId },
                method: "POST",
                success: (response) => {
                    //cancelNav = false;

                    if (response.status !== 200)
                        throw "";

                    this.__page.website.nav({ url: response.data, replace: true });
                    this.__isLoading = false;
                },
            });
        });
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