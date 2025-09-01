import { DOM } from "@brandup/ui-dom";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../../typings/content";
import { TextDesigner } from "./text";
import { HtmlDesigner } from "./html";
import { ModelDesigner } from "./model";
import { ImageDesigner } from "./image";
import { PageBlocksDesigner } from "./page-blocks";
import ContentPage from "../../pages/content";
import "./page.less";
import { AjaxQueue } from "@brandup/ui-ajax";

export class PageDesigner implements IPageDesigner {
    readonly page: ContentPage;
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __fields: { [key: string]: IContentFieldDesigner } = {};
    private __rootElem: HTMLElement;
    private __accentedField: IContentFieldDesigner = null;

    constructor(page: ContentPage) {
        this.page = page;
        this.editId = page.model.editId;

        this.queue = new AjaxQueue();
        this.__rootElem = DOM.queryElement(document.body, "[content-root]");
        this.__rootElem.classList.add("page-designer");

        this.render();

        document.body.classList.add("bp-state-design");
    }

    accentField(field: IContentFieldDesigner) {
        if (this.__accentedField)
            throw "";

        this.__rootElem.classList.add("accented");

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
            this.__rootElem.classList.remove("accented");

            for (const key in this.__fields) {
                const f = this.__fields[key];
                f.element.classList.remove("hide-ui");
            }

            this.__accentedField = null;
        }
    }

    render() {
        const fieldElements = DOM.queryElements(this.__rootElem, "[content-field]");
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

        this.page.renderComponents();
    }

    destroy() {
        for (const key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = null;

        document.body.classList.remove("bp-state-design");
    }
}