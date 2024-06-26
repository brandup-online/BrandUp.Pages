import { DOM } from "brandup-ui-dom";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../typings/content";
import { TextDesigner } from "./designer/text";
import { HtmlDesigner } from "./designer/html";
import { ModelDesigner } from "./designer/model";
import { ImageDesigner } from "./designer/image";
import { PageBlocksDesigner } from "./designer/page-blocks";
import { AjaxQueue } from "brandup-ui-ajax";
import { Page } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import "./editor.less";
import { editPage } from "../dialogs/pages/edit";
import { UIElement } from "brandup-ui";
import { IContentModel } from "../admin/page-toolbar";

export class Editor extends UIElement implements IPageDesigner {
    readonly page: Page;
    readonly contentElem: HTMLElement;
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __fields: { [key: string]: IContentFieldDesigner } = {};
    private __accentedField: IContentFieldDesigner = null;
    private __isLoading = false;

    get typeName(): string { return "BrandUpPages.Editor"; }

    constructor(page: Page, contentElem: HTMLElement, content: IContentModel[]) {
        super();
        
        console.log("🚀 ~ Editor ~ constructor ~ content:", content)
              
        this.page = page;
        this.contentElem = contentElem;
        this.contentElem.classList.add("root-designer");
        this.editId = contentElem.dataset["contentEditId"];

        this.queue = new AjaxQueue();

        const contentPathMap = new Map();
        contentPathMap.set("", contentElem);
        
        DOM.queryElements(contentElem, "[data-content-path]").forEach(elem => contentPathMap.set(elem.getAttribute("data-content-path"), elem));
        const contentFieldElements = DOM.queryElements(contentElem, "[data-content-field-path][data-content-field-name]");
        
        console.log("🚀 ~ Editor ~ constructor ~ contentPathMap:", contentPathMap)
        for (const contentItem of content) {
            
        }



        this.__renderToolbar();
        this.__renderDesigner();
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
            f.element.classList.add("hide-ui");
        }

        this.__accentedField = field;
    }
    clearAccent() {
        if (this.__accentedField) {

            for (const key in this.__fields) {
                const f = this.__fields[key];
                f.element.classList.remove("hide-ui");
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

    private __renderDesigner() {
        const fieldElements = DOM.queryElements(this.contentElem, "[content-field]");
        for (let i = 0; i < fieldElements.length; i++) {
            const fieldElem = fieldElements.item(i);
            if (!fieldElem.hasAttribute("content-field-model") || !fieldElem.hasAttribute("data-content-designer") || fieldElem.classList.contains("field-designer"))
                continue;

            const designerName = fieldElem.getAttribute("data-content-designer");
            const fieldModel: ContentFieldModel = JSON.parse(fieldElem.getAttribute("content-field-model"));
            let fieldDesigner: IContentFieldDesigner;
            switch (designerName.toLowerCase()) {
                case "text": {
                    fieldDesigner = new TextDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "html": {
                    fieldDesigner = new HtmlDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "image": {
                    fieldDesigner = new ImageDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "model": {
                    fieldDesigner = new ModelDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "page-blocks": {
                    fieldDesigner = new PageBlocksDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                default:
                    continue;
            }

            this.__fields[fieldDesigner.fullPath] = fieldDesigner;
        }

        this.page.refreshScripts();
    }

    redraw () { // Временный публичный метод для ModelDesigner
        this.__renderDesigner();
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

class Content {
    private __fields: { [key: string]: Field<any> } = {};
    private __container: HTMLElement;

    constructor(editor: Editor, model: any, container: HTMLElement = null) {

    }

    renderDesigner() {

    }

    redraw () {

    }
}

abstract class Field<TModel> {
    readonly model: TModel;
    designer: IContentFieldDesigner;

    constructor(editor: Editor, model: TModel, valueElem: HTMLElement = null) {
        this.model = model;
    }

    // renderDesigner() {
    //     this.designer = createDesigner();
    // }

    // abstract createDesigner(): IContentFieldDesigner;
}

class TextField extends Field<any> {

}