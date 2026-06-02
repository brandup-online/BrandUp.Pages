import { DOM } from "@brandup/ui";
import { AjaxQueue } from "@brandup/ui-ajax";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../../typings/content";
import { TextDesigner } from "./text";
import { HtmlDesigner } from "./html";
import { ModelDesigner } from "./model";
import { ImageDesigner } from "./image";
import { PageBlocksDesigner } from "./page-blocks";
import { ContentPage } from "../../pages/content";
import "./page.less";

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
        if (!this.__rootElem)
            throw new Error("PageDesigner: не найден корневой элемент контента [content-root].");
        this.__rootElem.classList.add("page-designer");

        this.render();

        document.body.classList.add("bp-state-design");
    }

    accentField(field: IContentFieldDesigner) {
        if (this.__accentedField)
            throw new Error("Another field is already accented.");

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

            const factory = DESIGNER_FACTORIES[designerName.toLowerCase()];
            if (!factory)
                continue;

            const fieldDesigner = factory(this, fieldElem, fieldModel.options);
            this.__fields[fieldDesigner.fullPath] = fieldDesigner;
        }

        this.page.renderComponents();
    }

    destroy() {
        for (const key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = null;

        this.queue.destroy();

        document.body.classList.remove("bp-state-design");
    }
}

type DesignerFactory = (page: PageDesigner, elem: HTMLElement, options: any) => IContentFieldDesigner;

// Реестр designer'ов полей по имени — вместо switch при рендеринге.
const DESIGNER_FACTORIES: { [name: string]: DesignerFactory } = {
    "text": (page, elem, options) => new TextDesigner(page, elem, options),
    "html": (page, elem, options) => new HtmlDesigner(page, elem, options),
    "image": (page, elem, options) => new ImageDesigner(page, elem, options),
    "model": (page, elem, options) => new ModelDesigner(page, elem, options),
    "page-blocks": (page, elem, options) => new PageBlocksDesigner(page, elem, options),
};