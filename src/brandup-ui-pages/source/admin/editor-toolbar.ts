﻿import { UIElement } from "brandup-ui";
import { PageDesigner } from "../content/designer/page";
import { editPage } from "../dialogs/pages/edit";
import { DOM } from "brandup-ui-dom";
import { Page, PageModel } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import "../styles.less";

export class EditorToolbar extends UIElement {
    private __designer: PageDesigner;
    private __isLoading = false;

    readonly __page: Page<PageModel>;

    get typeName(): string { return "BrandUpPages.EditorToolbar"; }

    constructor(page: Page<PageModel>, contentEditElem: HTMLElement) {
        super();
        this.__page = page;
        
        this.__designer = new PageDesigner(page, contentEditElem);

        this.__initLogic();
        this.__renderUI();
    }

    private __renderUI() {
        const toolbarElem = DOM.tag("div", { class: "bp-elem editor-toolbar" }, [
            DOM.tag("button", { class: "bp-button", command: "bp-commit", title:"Применить изменения" }, [saveIcon, "Сохранить"]),
            DOM.tag("button", { class: "bp-button secondary", command: "bp-discard", title:"Отменить изменения" }, [cancelIcon, "Отмена"]),
            DOM.tag("button", { class: "bp-button neutral right", command: "bp-content", title:"Показать контент" }, [editBlockIcon, "Контент"]),
        ]);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);
    }

    private __initLogic() {
        this.registerCommand("bp-content", () => {
            editPage(this.__designer.editId).then(() => {
                this.__page.website.app.reload();
            });
        });

        this.registerCommand("bp-commit", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.__page.website.request({
                url: "/brandup.pages/page/content/commit",
                urlParams: { editId: this.__designer.editId },
                method: "POST",
                success: (response) => {
                    if (response.status !== 200)
                        throw "Error commit content editing.";

                    this.__complateEdit();
                }
            }, true);
        });

        this.registerCommand("bp-discard", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.__page.website.request({
                url: "/brandup.pages/page/content/discard",
                urlParams: { editId: this.__designer.editId },
                method: "POST",
                success: (response) => {
                    if (response.status !== 200)
                        throw "Error discard content editing.";

                    this.__complateEdit();
                },
            });
        });
    }

    private __complateEdit() {
        delete this.__designer.contentElem.dataset["contentEditId"];

        const url = new URL(location.href);
        url.searchParams.delete("editid");

        this.__page.website.nav({ url: url.toString(), replace: true });
        this.__isLoading = false;
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