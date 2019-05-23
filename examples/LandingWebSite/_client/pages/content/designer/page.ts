import { DOM, AjaxQueue } from "brandup-ui";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../../typings/content";
import { TextDesigner } from "./text";
import { HtmlDesigner } from "./html";
import { ModelDesigner } from "./model";
import { ImageDesigner } from "./image";
import ContentPage from "../../pages/content";
import { PageBlocksDesigner } from "../../../designers/page-blocks";
import "./page.less";

export class PageDesigner implements IPageDesigner {
    readonly page: ContentPage;
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __fields: { [key: string]: IContentFieldDesigner } = {};
    private __rootElem: HTMLElement;
    private __accentedField: IContentFieldDesigner = null;

    constructor(page: ContentPage) {
        page.attachDestroyFunc(() => { this.destroy(); }); 

        this.page = page;
        this.editId = page.model.editId;

        this.queue = new AjaxQueue();
        this.__rootElem = DOM.queryElement(document.body, "[content-root]");
        this.__rootElem.classList.add("page-designer");

        this.render();
    }

    accentField(field: IContentFieldDesigner) {
        if (this.__accentedField)
            throw "";

        this.__rootElem.classList.add("accented");

        for (let key in this.__fields) {
            let f = this.__fields[key];
            if (f === field)
                continue;
            f.element.classList.add("hide-ui");
        }

        this.__accentedField = field;
    }
    clearAccent() {
        if (this.__accentedField) {
            this.__rootElem.classList.remove("accented");

            for (let key in this.__fields) {
                let f = this.__fields[key];
                f.element.classList.remove("hide-ui");
            }

            this.__accentedField = null;
        }
    }

    render() {
        var fieldElements = DOM.queryElements(this.__rootElem, "[content-field]");
        for (let i = 0; i < fieldElements.length; i++) {
            let fieldElem = fieldElements.item(i);
            if (!fieldElem.hasAttribute("content-field-model") || !fieldElem.hasAttribute("content-designer") || fieldElem.classList.contains("field-designer"))
                continue;

            let designerName = fieldElem.getAttribute("content-designer");
            let fieldModel = <ContentFieldModel>JSON.parse(fieldElem.getAttribute("content-field-model"));
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
        for (let key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = null;
    }
}