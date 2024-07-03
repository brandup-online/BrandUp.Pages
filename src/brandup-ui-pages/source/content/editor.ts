import { DOM } from "brandup-ui-dom";
import { IPageDesigner, IContentFieldDesigner } from "../typings/content";
import { AjaxQueue, AjaxResponse } from "brandup-ui-ajax";
import { Page } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import { editPage } from "../dialogs/pages/edit";
import { UIElement } from "brandup-ui";
import { Content } from "./content";
import { ContentModel } from "../typings/models";
import { FieldProvider } from "./provider/base";
import { ModelFieldProvider } from "./provider/model";

export class Editor extends UIElement implements IPageDesigner {
    readonly page: Page;
    readonly contentElem: HTMLElement;
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __contentItems: Map<string, Content>;
    private __modelFields: {[key: string]: FieldProvider<any, any>} = {};
    private __accentedField: IContentFieldDesigner = null;
    private __isLoading = false;

    get typeName(): string { return "BrandUpPages.Editor"; }

    constructor(page: Page, contentElem: HTMLElement) {
        super();
              
        this.page = page;
        this.contentElem = contentElem;
        this.contentElem.classList.add("root-designer");
        this.editId = contentElem.dataset["contentEditId"];

        this.queue = new AjaxQueue();

        this.__renderToolbar();
        this.__fetchContent();
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

    private __fetchContent() {
        this.__isLoading = true;
        this.queue.push({
            url: "/brandup.pages/page/content",
            urlParams: { editId: this.editId },
            method: "GET",
            success: (response: AjaxResponse) => {
                this.__isLoading = false;

                if (response.status !== 200) {
                    throw "Error get content.";
                }

                const contents: ContentModel[] = response.data;
                this.__contentItems = new Map();
                this.__renderContent(this.contentElem, contents);
            },
        });
    }

    private __renderContent(rootElem: HTMLElement, contents: ContentModel[]) {
        const contentElemMap = new Map<string, HTMLElement>();
        const contentFieldElemMap = new Map<string, Map<string, HTMLElement>>();
        
        contentElemMap.set(contents[0].path, rootElem);
        DOM.queryElements(rootElem, "[data-content-path]").forEach(elem => contentElemMap.set(elem.dataset.contentPath, elem));
        DOM.queryElements(rootElem, "[data-content-field-path][data-content-field-name]").forEach(elem => {
            const fieldPath = elem.dataset.contentFieldPath;
            const fieldName = elem.dataset.contentFieldName;
            if (!contentFieldElemMap.has(fieldPath)) {
                contentFieldElemMap.set(fieldPath, new Map());
            }
            contentFieldElemMap.get(fieldPath).set(fieldName, elem);
        });
        
        for (const contentModelItem of contents) {
            const contentElem = contentElemMap.get(contentModelItem.path);
            const fieldElems = contentFieldElemMap.get(contentModelItem.path);

            const parentField = this.__modelFields[this.__trimdPath(contentModelItem.path)] || null;
            const contentItem = new Content(this, parentField, contentModelItem, contentElem, fieldElems);
            if (parentField) (parentField as ModelFieldProvider).insertContent(contentItem);

            contentItem.getFields().forEach(field => {
                if (field.isModelField)
                    this.__modelFields[this.__buildPath([contentModelItem.path, field.name])] = field;
            });
            this.__contentItems.set(contentModelItem.path, contentItem);
        }
    }

    createContent(modelpath: string, container: HTMLElement = null, callback?: () => void) {
        this.__isLoading = true;
        this.queue.push({
            url: "/brandup.pages/page/content",
            urlParams: { editId: this.editId, path: modelpath },
            method: "GET",
            success: (response: AjaxResponse) => {
                this.__isLoading = false;
                
                if (response.status !== 200) {
                    throw "Error get content.";
                }
                
                const contents: ContentModel[] = response.data;
                this.__renderContent(container, contents);
                
                if (callback) callback();
            } 
        });
    }

    private __buildPath(paths: string[]) {
        let result = "";
        paths.forEach((path, index) => {
            if (path === "") return;
            if (index === paths.length - 1) return result += path;
            result += path + ".";
        })
        return result;
    }

    private __trimdPath(path: string) {
        const paths: string[] = path.split(".");
        paths[paths.length-1] = paths[paths.length-1].replace(/\W|\d/gm, "");
        return this.__buildPath(paths);
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
    
    private __initLogic() {
        this.registerCommand("bp-content", () => {
            editPage(this, "");
        });

        this.registerCommand("bp-commit", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.queue.push({
                url: "/brandup.pages/page/content/commit",
                urlParams: { editId: this.editId },
                method: "POST",
                success: (response) => {
                    if (response.status !== 200)
                        throw "Error commit content editing."; // TODO получаем список ошибок и рисуем модалку
                    if (response.data.isSuccess)
                        this.__complateEdit();
                }
            });
        });

        this.registerCommand("bp-discard", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.queue.push({
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