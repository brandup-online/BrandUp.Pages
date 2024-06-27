import { Editor } from "../editor";
import { IContentFieldDesigner } from "../../typings/content";

export abstract class FieldProvider<TModel> {
    readonly model: TModel;
    designer: IContentFieldDesigner;
    protected __valueElem: HTMLElement;
    protected __editor: Editor;

    constructor(editor: Editor, model: TModel, valueElem: HTMLElement = null) {
        this.model = model;
        this.__valueElem = valueElem;
        this.__editor = editor;
    }

    renderDesigner() {
        this.designer = this.createDesigner();
    }

    abstract createDesigner(): IContentFieldDesigner;
}