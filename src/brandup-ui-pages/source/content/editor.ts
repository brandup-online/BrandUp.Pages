import { DOM } from "brandup-ui-dom";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../typings/content";
import { AjaxQueue, AjaxResponse } from "brandup-ui-ajax";
import { Page } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import { editPage } from "../dialogs/pages/edit";
import { UIElement } from "brandup-ui";
import { BeginPageEditResult, IContentModel } from "../admin/page-toolbar";
import { FieldProvider } from "./provider/base";
import { HtmlFieldProvider } from "./provider/html";
import { ImageFieldProvider } from "./provider/image";
import { ModelFieldProvider } from "./provider/model";
import { TextFieldProvider } from "./provider/text";
import { HyperlinkFieldProvider } from "./provider/hyperlink";
import { ModelDesigner } from "./designer/model";
import { PageBlocksDesigner } from "./designer/page-blocks";

export class Editor extends UIElement implements IPageDesigner {
    readonly page: Page;
    readonly contentElem: HTMLElement;
    private content: IContentModel[];
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __fields: { [key: string]: IContentFieldDesigner } = {};
    private __contentItems: Content[] = [];
    private __accentedField: IContentFieldDesigner = null;
    private __isLoading = false;

    get typeName(): string { return "BrandUpPages.Editor"; }

    constructor(page: Page, contentElem: HTMLElement, content: IContentModel[]) {
        super();
              
        this.page = page;
        this.contentElem = contentElem;
        this.content = content;
        this.contentElem.classList.add("root-designer");
        this.editId = contentElem.dataset["contentEditId"];

        this.queue = new AjaxQueue();

        this.__renderToolbar();
        this.__renderContent();
        this.__initLogic();

        document.body.classList.add("bp-state-design");
    }

    accentField(field: IContentFieldDesigner) {
        if (this.__accentedField)
            throw "";

        for (const key in this.__fields) {
            const f = this.__fields[key];
            if (f === field)
                continue;
            f?.element?.classList.add("hide-ui");
        }

        this.__accentedField = field;
    }
    clearAccent() {
        if (this.__accentedField) {

            for (const key in this.__fields) {
                const f = this.__fields[key];
                f?.element.classList.remove("hide-ui");
            }

            this.__accentedField = null;
        }
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

                this.content = response.data.content;

                // this.__contentItems.forEach(item => item.destroy());
        
                this.__fields = {};
                this.__contentItems = [];
        
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

                const modelFields = new Map<string, ModelFieldProvider>();
                
                for (const contentItem of this.content) {
                    const fields = new Map<string, FieldProvider>();
                    contentItem.fields.forEach((item: ContentFieldModel) => {
                        const contentPathElem = contentFieldsMap.get(contentItem.path)?.get(item.name);
                        let type = contentPathElem.getAttribute("data-content-designer");
                        const field = this.__getFieldInstance(type);
                        if (!contentPathElem) return;
                        
                        if (field === ModelFieldProvider) {
                            const fieldProvider = new field(this, item, contentPathElem, type === "page-blocks" ? PageBlocksDesigner : ModelDesigner)
                            fields.set(item.name, fieldProvider);
                            modelFields.set((contentItem.path + item.name).replace(".", ""), fieldProvider as ModelFieldProvider);
                        }
                        else
                            fields.set(item.name, new field(this, item, contentPathElem));
                    });
                    const contentPathElem = contentPathMap.get(contentItem.path);
                    if (!contentPathElem) continue;
                    const content = new Content(this, contentItem, contentPathElem, fields);
                    const path = contentItem.path.split(".");
                    path[path.length-1] = path[path.length-1].replace(/\W|\d/g, "");
                    content.parent = modelFields.get(path.reduce((acc, value) => acc += value));
                    
                    content.renderDesigners();
                    this.__fields = { ...this.__fields, ...content.getDesigers() };
                    this.__contentItems.push(content);
                }
                modelFields.forEach(model => model.items = this.__contentItems.filter(content => content.parent === model));
                this.page.refreshScripts();
            },
        });


    }

    private __getFieldInstance(type: string) {
        switch (type) {
            case "text":
                return TextFieldProvider;
            case "hyperlink":
                return HyperlinkFieldProvider;
            case "html":
                return HtmlFieldProvider;
            case "image":
                return ImageFieldProvider;
            case "model":
            case "page-blocks":
                return ModelFieldProvider;
            default:
                throw new Error("field type not found");
        }
    }

    redraw () { // Временный публичный метод для ModelDesigner
        // this.page.website.nav({ url: this.page.buildUrl({ editid: this.editId }), replace: true });
    }

    private __initLogic() {
        this.registerCommand("bp-content", () => {
            editPage(this.editId).then(() => {
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
        for (const key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = null;

        this.queue.destroy();

        this.contentElem.classList.remove("page-designer");

        document.body.classList.remove("bp-state-design");

        this.element.remove();

        super.destroy();
    }
}

export class Content {
    private __fields: Map<string, FieldProvider>;
    private __container: HTMLElement;
    private __editor: Editor;
    private __parent: ModelFieldProvider | null;
    set parent(field: ModelFieldProvider) {this.__parent = field}
    get parent() { return this.__parent }
    get containerDataset() { return this.__container.dataset };

    constructor(editor: Editor, model: any, container: HTMLElement = null, fields: Map<string, FieldProvider> = new Map()) {
        this.__container = container;
        this.__editor = editor;
        this.__fields = fields;
    }

    getDesigers() {
        const result = {};
        this.__fields.forEach(field => { if (field.designer) result[field.designer.fullPath] = field.designer });
        return result;
    }

    renderDesigners() {
        this.__fields.forEach(field => field.renderDesigner());
    }

    redraw () {
        this.renderDesigners();
    }

    destroy() {
        this.__fields.forEach(field => field.destroy());
    }
}