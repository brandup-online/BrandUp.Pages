import { DOM, AjaxQueue } from "brandup-ui";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../typings/content";
import { TextDesigner } from "./text";
import { HtmlDesigner } from "./html";
import { ContentDesigner } from "./content";
import "./page.less";

export class PageDesigner implements IPageDesigner {
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __fields: { [key: string]: IContentFieldDesigner } = {};
    private __rootElem: HTMLElement;
    private __accentedField: IContentFieldDesigner = null;

    constructor(editId: string) {
        this.editId = editId;

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
            if (!fieldElem.hasAttribute("content-field-model") || fieldElem.classList.contains("field-designer"))
                continue;

            let fieldModel = <ContentFieldModel>JSON.parse(fieldElem.getAttribute("content-field-model"));

            let fieldDesigner: IContentFieldDesigner;
            switch (fieldModel.type) {
                case "Text": {
                    fieldDesigner = new TextDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "Html": {
                    fieldDesigner = new HtmlDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                case "Content": {
                    fieldDesigner = new ContentDesigner(this, fieldElem, fieldModel.options);
                    break;
                }
                default:
                    throw "";
            }

            this.__fields[fieldDesigner.fullPath] = fieldDesigner;
        }
    }

    destroy() {
        for (let key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = null;
    }
}