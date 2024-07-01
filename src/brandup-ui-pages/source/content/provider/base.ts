import { Editor } from "../editor";
import { ContentFieldModel, IContentField, IContentFieldDesigner } from "../../typings/content";

export abstract class FieldProvider {
    readonly model: ContentFieldModel;
    designer: IContentFieldDesigner;
    field: IContentField;
    protected __valueElem: HTMLElement;
    protected __editor: Editor;

    constructor(editor: Editor, model: ContentFieldModel, valueElem: HTMLElement = null) {
        this.model = model;
        this.__valueElem = valueElem;
        this.__editor = editor;
    }

    renderDesigner() {
        this.designer = this.createDesigner();
    }

    abstract createDesigner(): IContentFieldDesigner;
    // abstract createField(): IContentField;

    destroy() {
        this.designer?.destroy();
        this.field?.destroy();
    }
}