import { Editor } from "../editor";
import { ContentFieldModel, IContentField, IContentFieldDesigner } from "../../typings/content";
import { DesignerEvent } from "../../content/designer/base";
import { AjaxRequest } from "brandup-ui-ajax";

export abstract class FieldProvider<TValue, TOptions> {
    protected __model: ContentFieldModel<TValue, TOptions>;
    designer: IContentFieldDesigner;
    field: IContentField;
    protected __valueElem: HTMLElement;
    protected __editor: Editor;

    constructor(editor: Editor, model: ContentFieldModel<TValue, TOptions>, valueElem: HTMLElement = null) {
        this.__model = model;
        this.__valueElem = valueElem;
        this.__editor = editor;
    }
    
    protected _onChange(e: DesignerEvent<any>) {
        this.setValue(e.value);
    }
    
    renderDesigner() {
        this.designer = this.createDesigner();
        this.designer?.setCallback("change", (e) => this._onChange(e));
    }

    getValue() {
        return this.__model.value;
    }

    setValue(value) {
        this.__model.value = value;
        this.designer?.setValue(value);
        this.field?.setValue(value);
    }

    protected request(options: AjaxRequest) {
        if (!options.urlParams)
            options.urlParams = {};

        options.urlParams["editId"] = this.__editor.editId;
        options.urlParams["path"] = this.__valueElem.dataset.contentFieldPath;
        options.urlParams["field"] = this.__model.name;

        this.__editor.queue.push(options);
    }

    abstract createDesigner(): IContentFieldDesigner;
    // abstract createField(): IContentField;

    destroy() {
        this.designer?.destroy();
        this.field?.destroy();
    }
}