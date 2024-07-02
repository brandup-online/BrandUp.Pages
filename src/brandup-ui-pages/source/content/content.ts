import { ContentFieldModel } from "../typings/content";
import { Editor } from "./editor";
import { FieldProvider } from "./provider/base";
import { ModelFieldProvider } from "./provider/model";
import { HtmlFieldProvider } from "./provider/html";
import { HyperlinkFieldProvider } from "./provider/hyperlink";
import { ImageFieldProvider } from "./provider/image";
import { TextFieldProvider } from "./provider/text";

export class Content {
    private __fields: Map<string, FieldProvider<any, any>>;
    private __container: HTMLElement;
    readonly __editor: Editor;
    private __parent: ModelFieldProvider | null;
    set parent(field: ModelFieldProvider) {this.__parent = field}
    get parent() { return this.__parent }
    get containerDataset() { return this.__container.dataset };

    constructor(editor: Editor, model: any, container: HTMLElement = null, fieldsElements: Map<string, Map<string, HTMLElement>>) {
        this.__container = container;
        this.__editor = editor;

        this.__fields = new Map<string, FieldProvider<any, any>>();
        model.fields.forEach((item: ContentFieldModel<any, any>) => {
            const fieldElem = fieldsElements.get(model.path)?.get(item.name);
            if (!fieldElem) return;
            const field = this.__getFieldType(item.type?.toLowerCase());
            this.__fields.set(item.name, new field(this, item, fieldElem));
        });

        this.renderDesigners();
    }

    private __getFieldType(type: string) {
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

    getDesigers() {
        const result = {};
        this.__fields.forEach(field => { if (field.designer) result[field.designer.fullPath] = field.designer });
        return result;
    }

    renderDesigners() {
        this.__fields.forEach(field => field.renderDesigner());
    }

    destroy() {
        this.__fields.forEach(field => field.destroy());
    }
}