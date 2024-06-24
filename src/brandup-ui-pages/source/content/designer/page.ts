import { DOM } from "brandup-ui-dom";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../../typings/content";
import { TextDesigner } from "./text";
import { HtmlDesigner } from "./html";
import { ModelDesigner } from "./model";
import { ImageDesigner } from "./image";
import { PageBlocksDesigner } from "./page-blocks";
import { AjaxQueue } from "brandup-ui-ajax";
import { Page } from "brandup-ui-website";
import "./page.less";

export class PageDesigner implements IPageDesigner {
    readonly page: Page;
    readonly contentElem: HTMLElement;
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __fields: { [key: string]: IContentFieldDesigner } = {};
    private __accentedField: IContentFieldDesigner = null;

    constructor(page: Page, contentElem: HTMLElement) {
        this.page = page;
        this.contentElem = contentElem;
        this.contentElem.classList.add("page-designer");
        this.editId = contentElem.dataset["contentEditId"];

        this.queue = new AjaxQueue();

        this.render();

        document.body.classList.add("bp-state-design");
    }

    accentField(field: IContentFieldDesigner) {
        if (this.__accentedField)
            throw "";

        this.contentElem.classList.add("accented");

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
            this.contentElem.classList.remove("accented");

            for (const key in this.__fields) {
                const f = this.__fields[key];
                f.element.classList.remove("hide-ui");
            }

            this.__accentedField = null;
        }
    }

    render() {
        const fieldElements = DOM.queryElements(this.contentElem, "[content-field]");
        for (let i = 0; i < fieldElements.length; i++) {
            const fieldElem = fieldElements.item(i);
            if (!fieldElem.hasAttribute("content-field-model") || !fieldElem.hasAttribute("content-designer") || fieldElem.classList.contains("field-designer"))
                continue;

            const designerName = fieldElem.getAttribute("content-designer");
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

    destroy() {
        for (const key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = null;

        document.body.classList.remove("bp-state-design");
    }
}