import { Editor } from "./editor";
import { FieldProvider } from "./provider/base";
import { ModelFieldProvider } from "./provider/model";
import { HtmlFieldProvider } from "./provider/html";
import { HyperlinkFieldProvider } from "./provider/hyperlink";
import { ImageFieldProvider } from "./provider/image";
import { TextFieldProvider } from "./provider/text";
import { ContentModel } from "../typings/models";
import { IContent, IParentContent } from "../typings/content";

export class Content implements IContent {
    readonly editor: Editor;
    readonly path: string;
    readonly index: number;
    readonly parent: IParentContent;
    readonly model: ContentModel;
    readonly container: HTMLElement;
    private __fields: Map<string, FieldProvider<any, any>>;
    
    constructor(editor: Editor, parent: IParentContent, model: ContentModel, container?: HTMLElement, fieldsElements?: Map<string, HTMLElement>) {
        this.editor = editor;
        this.path = model.path;
        this.index = model.index;
        this.parent = parent;
        this.model = model;
        this.container = container;

        this.parent?.map(this);

        if (this.index >= 0 && this.container)
            this.container.dataset.contentPathIndex = this.index.toString();
        
        this.__fields = new Map<string, FieldProvider<any, any>>();

        model.fields.forEach(field => {
            const fieldElem = fieldsElements.get(field.name);

            const provider = this.__getFieldType(field.type.toLowerCase());
            this.__fields.set(field.name, new provider(this, field, fieldElem));
        });

        this.renderDesigner();
    }

    private __getFieldType(type: string): any {
        switch (type) {
            case "text":
                return TextFieldProvider;
            case "hyperlink":
                return HyperlinkFieldProvider;
            case "html":
                return HtmlFieldProvider;
            case "image":
                return ImageFieldProvider;
            case "model":
                return ModelFieldProvider;
            default:
                throw new Error(`field type ${type} not found`);
        }
    }

    getField(name: string) {
        if (!this.__fields.has(name))
            throw `Not found field "${name}" for content path "${this.path}".`;

        return this.__fields.get(name);
    }

    getFields() {
        return this.__fields;
    }

    renderDesigner() {
        if (!this.container)
            return;

        this.__fields.forEach(field => field.renderDesigner());
    }

    destroy() {
        this.__fields.forEach(field => field.destroy());
    }
}