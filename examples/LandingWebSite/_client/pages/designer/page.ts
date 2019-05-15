import { DOM, AjaxQueue } from "brandup-ui";
import { IPageDesigner, IContentFieldDesigner, ContentFieldModel } from "../typings/content";
import { TextDesigner } from "./text";
import { HtmlDesigner } from "./html";
import { ContentDesigner } from "./content";

export class PageDesigner implements IPageDesigner {
    readonly editId: string;
    readonly queue: AjaxQueue;
    private __fields: { [key: string]: IContentFieldDesigner } = {};

    constructor(editId: string) {
        this.editId = editId;

        this.queue = new AjaxQueue();

        var contentElem = DOM.queryElement(document.body, "[content-page]");

        var fieldElements = DOM.queryElements(contentElem, "[content-field]");
        for (let i = 0; i < fieldElements.length; i++) {
            let fieldElem = fieldElements.item(i);
            if (!fieldElem.hasAttribute("content-field-model"))
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