import { Editor } from "./editor";
import { FieldProvider } from "./provider/base";
import { ModelFieldProvider } from "./provider/model";
import { HtmlFieldProvider } from "./provider/html";
import { HyperlinkFieldProvider } from "./provider/hyperlink";
import { ImageFieldProvider } from "./provider/image";
import { TextFieldProvider } from "./provider/text";
import { ContentModel } from "../typings/models";

export class Content {
    private __fields: Map<string, FieldProvider<any, any>>;
    private __container: HTMLElement;
    readonly __editor: Editor;
    readonly model: ContentModel;
    private __parent: ModelFieldProvider | null;

    get parent() { return this.__parent }
    set parent(field: ModelFieldProvider) { this.__parent = field; }

    constructor(editor: Editor, model: ContentModel, container: HTMLElement = null, fieldsElements: Map<string, HTMLElement>) {
        this.__container = container;
        this.__editor = editor;

        this.__fields = new Map<string, FieldProvider<any, any>>();

        this.model = model;
        model.fields.forEach(field => {
            const fieldElem = fieldsElements.get(field.name);

            const provider = this.__getFieldType(field.type.toLowerCase());
            this.__fields.set(field.name, new provider(this, field, fieldElem));
        });

        this.renderDesigners();
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

    getFields() {
        return this.__fields;
    }

    renderDesigners() {
        if (!this.__container)
            return;

        this.__fields.forEach(field => field.renderDesigner());
    }

    destroy() {
        this.__fields.forEach(field => field.destroy());
    }
}