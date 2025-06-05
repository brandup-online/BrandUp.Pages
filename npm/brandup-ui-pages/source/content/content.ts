import { FieldProvider } from "./provider/base";
import { ModelFieldProvider } from "./provider/model";
import { HtmlFieldProvider } from "./provider/html";
import { HyperlinkFieldProvider } from "./provider/hyperlink";
import { ImageFieldProvider } from "./provider/image";
import { TextFieldProvider } from "./provider/text";
import { ContentEditor } from "./editor";
import { ContentModel } from "../typings/content";

export class Content {
    readonly host: IContentHost;
    readonly path: string;
    readonly index: number;
    private __fields: Map<string, FieldProvider<any, any>>;

    private __container: HTMLElement = null;
    
    get container(): HTMLElement { return this.__container; }

    constructor(host: IContentHost, model: ContentModel) {
        this.host = host;
        this.path = model.path;
        this.index = model.index;

        this.host.attach(this);

        if (this.index >= 0 && this.container)
            this.container.dataset.contentPathIndex = this.index.toString();
        
        this.__fields = new Map<string, FieldProvider<any, any>>();

        model.fields.forEach(field => {
            const provider = this.__getFieldType(field.type.toLowerCase());
            this.__fields.set(field.name, new provider(this, field));
        });
    }
    
    get fields(): ReadonlyMap<string, FieldProvider<any, any>> { return this.__fields; }
    
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
    
    renderDesigner(structure: { container: HTMLElement, fields: Map<string, HTMLElement>}) {
        this.__container = structure.container;

        this.__fields.forEach(field => {
            if (!structure.fields.has(field.name))
                return;

            const fieldElem = structure.fields.get(field.name);
            field.renderDesigner(fieldElem);
        });
    }

    destroy() {
        this.__fields.forEach(field => field.destroy());
    }
}

export interface IContentHost {
    get editor(): ContentEditor;
    get isList(): boolean;
    attach(content: Content);
}