import { DOM } from "brandup-ui-dom";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../typings/content";
import { AjaxQueue, AjaxResponse } from "brandup-ui-ajax";
import { Page } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import { editPage } from "../dialogs/pages/edit";
import { UIElement } from "brandup-ui";
import { IContentModel } from "../admin/page-toolbar";
import { Content } from "./content";

export class Editor extends UIElement implements IPageDesigner {
    readonly page: Page;
    readonly contentElem: HTMLElement;
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __contentItems: Map<string, Content>;
    private __accentedField: IContentFieldDesigner = null;
    private __isLoading = false;

    get typeName(): string { return "BrandUpPages.Editor"; }

    constructor(page: Page, contentElem: HTMLElement, content: IContentModel[]) {
        super();
              
        this.page = page;
        this.contentElem = contentElem;
        this.contentElem.classList.add("root-designer");
        this.editId = contentElem.dataset["contentEditId"];

        this.queue = new AjaxQueue();

        this.__renderToolbar();
        this.__renderContent();
        this.__initLogic();

        document.body.classList.add("bp-state-design");
    }

    private __renderToolbar() {
        const toolbarElem = DOM.tag("div", { class: "bp-elem editor-toolbar" }, [
            DOM.tag("button", { class: "bp-button", command: "bp-commit", title:"Применить изменения" }, [saveIcon, "Сохранить"]),
            DOM.tag("button", { class: "bp-button secondary", command: "bp-discard", title:"Отменить изменения" }, [cancelIcon, "Отмена"]),
            DOM.tag("button", { class: "bp-button neutral right", command: "bp-content", title:"Показать контент" }, [editBlockIcon, "Контент"]),
        ]);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem)
    }

    private __renderContent() {
        this.page.website.request({
            url: "/brandup.pages/page/content/content",
            urlParams: { editId: this.editId },
            method: "GET",
            success: (response: AjaxResponse) => {
                this.__isLoading = false;

                if (response.status !== 200) {
                    throw "Error get content.";
                }

                const content: IContentModel[] = response.data;
                this.__contentItems = new Map();
        
                const contentPathMap = new Map<string, HTMLElement>();
                const contentFieldsMap = new Map<string, Map<string, HTMLElement>>();
                
                contentPathMap.set("", this.contentElem);
                DOM.queryElements(this.contentElem, "[data-content-path]").forEach(elem => contentPathMap.set(elem.dataset.contentPath, elem));
                DOM.queryElements(this.contentElem, "[data-content-field-path][data-content-field-name]").forEach(elem => {
                    const fieldPath = elem.dataset.contentFieldPath;
                    const fieldName = elem.dataset.contentFieldName;
                    if (!contentFieldsMap.has(fieldPath)) {
                        contentFieldsMap.set(fieldPath, new Map());
                    }
                    contentFieldsMap.get(fieldPath).set(fieldName, elem);
                });
                
                for (const contentItem of content) {
                    const contentPathElem = contentPathMap.get(contentItem.path);
                    if (!contentPathElem) continue;
                    this.__contentItems.set(contentItem.path, new Content(this, contentItem, contentPathElem, contentFieldsMap));                    
                }
            },
        });
    }

    removeContentItem(path: string) {
        this.__contentItems.forEach((item, key) => {
            if (key.startsWith(path)) {
                item.destroy();
                this.__contentItems.delete(key);
            }
        });
    }

    getContentItem(path: string) {
        return this.__contentItems.get(path);
    }

    redraw () { // Временный публичный метод для ModelDesigner
        this.page.website.nav({ url: this.page.buildUrl({ editid: this.editId }), replace: true });
    }

    private __initLogic() {
        this.registerCommand("bp-content", () => {
            editPage(this, "").then(() => {
                this.page.website.app.reload();
            });
        });

        this.registerCommand("bp-commit", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.page.website.request({
                url: "/brandup.pages/page/content/commit",
                urlParams: { editId: this.editId },
                method: "POST",
                success: (response) => {
                    if (response.status !== 200)
                        throw "Error commit content editing."; // TODO получаем список ошибок и рисуем модалку
                    if (response.data.isSuccess)
                        this.__complateEdit();
                }
            }, true);
        });

        this.registerCommand("bp-discard", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.page.website.request({
                url: "/brandup.pages/page/content/discard",
                urlParams: { editId: this.editId },
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
        delete this.contentElem.dataset["contentEditId"];
        
        const url = new URL(location.href);
        url.searchParams.delete("editid");
        
        this.page.website.nav({ url: url.toString(), replace: true });
        this.__isLoading = false;
    }

    destroy() {
        this.__contentItems.forEach(item => item.destroy());

        this.queue.destroy();

        this.contentElem.classList.remove("page-designer");

        document.body.classList.remove("bp-state-design");

        this.element.remove();

        super.destroy();
    }
}