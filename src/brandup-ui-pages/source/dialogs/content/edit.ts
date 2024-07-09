import { DialogOptions, Dialog } from "../dialog";
import { AjaxQueue } from "brandup-ui-ajax";
import { DOM } from "brandup-ui-dom";
import { ContentEditor } from "../../content/editor";
import { Content } from "../../content/content";
import "../dialog-form.less";
import defs from "../../content/defs"
import { FieldProvider, IFormField } from "../../content/provider/base";
import { HtmlContent } from "../../content/field/html";
import { FormField } from "../../content/field/base";
import { TextContent } from "../../content/field/text";
import { ModelField } from "../../content/field/model";

export class PageEditDialog extends Dialog<any> {
    private __formElem: HTMLFormElement;
    private navElem: HTMLElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: IFormField } = {};
    private __modelPath: string;
    private __queue: AjaxQueue;
    private __content: Content;

    constructor(content: Content, modelPath?: string, options?: DialogOptions) {
        super(options);

        this.__content = content;
        this.__modelPath = modelPath ? modelPath : "";
    }

    get typeName(): string { return "BrandUpPages.PageEditDialog"; }
    get modelPath(): string { return this.__modelPath; }
    get queue(): AjaxQueue { return this.__queue; }

    protected _onRenderContent() {
        this.element.classList.add("bp-dialog-form");

        this.content.appendChild(this.__formElem = DOM.tag("form", { method: "POST", class: "nopad" }) as HTMLFormElement);
        this.__formElem.appendChild(this.__fieldsElem = DOM.tag("div", { class: "fields" }));

        this.__formElem.addEventListener("submit", (e: Event) => {
            e.preventDefault();
            return false;
        });

        this.setHeader("Контент страницы");

        this.__registerFieldsTypes();
        this.__renderForm();

        this.registerCommand("navigate", (elem: HTMLElement) => {
            const path = elem.getAttribute("data-path");
            this.navigate(path);
        });
    }

    private __registerFieldsTypes() {
        defs.registerFormField("html", () => new Promise<typeof HtmlContent>((resolve) => resolve(HtmlContent)));
        defs.registerFormField("text", () => new Promise<typeof TextContent>((resolve) => resolve(TextContent)));
        defs.registerFormField("model", () => new Promise<typeof ModelField>((resolve) => resolve(ModelField)));
    }

    private __renderForm() {
        if (!this.navElem) {
            this.navElem = DOM.tag("ol", { class: "nav" });
            this.content.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }

        let path = this.__content.path;
        //while (path || path === "") {
        //    const content = this.__content.host.editor.navigate(path);
        //    let title = content.typeTitle;
        //    this.navElem.insertAdjacentElement("afterbegin", DOM.tag("li", path === this.__modelPath ? { class: "current" } : null, [
        //        DOM.tag("a", { href: "", "data-command": "navigate", "data-path": path }, [
        //            DOM.tag("bolt", null, path || "root"),
        //            DOM.tag("div", null, [
        //                DOM.tag("span", null, title),
        //                DOM.tag("span", null, model.typeName),
        //            ]),
        //        ]),
        //    ]));

        //    path = content.parentPath;
        //}
        
        this.__content.fields.forEach(field => {
            this.addField(field);
        });
    }

    navigate(modelPath: string) {
        this.__modelPath = modelPath ? modelPath : "";
        //this.__content.getFields().forEach(provider => provider.destroyField());
        this.__content = this.__content.host.editor.navigate(modelPath);
        this.__renderForm();
    }

    validate(): boolean {
        return true;
    }

    getField(name: string): IFormField {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw `Field "${name}" not exists.`;
        return this.__fields[name.toLowerCase()];
    }
    protected addField(provider: FieldProvider<any, any>) {
        defs.resolveFormField(provider.type.toLowerCase()).then((type) => {
            const field: FormField<any> = new type(provider.title, provider.options, provider);
    
            if (this.__fields.hasOwnProperty(provider.name.toLowerCase()))
                throw `Field name "${provider.name}" already exists.`;
    
            const containerElem = DOM.tag("div", { class: "field" });
    
            provider.registerForm(field);
            field.render(containerElem);
            field.raiseUpdateValue(provider.getValue());
            
            this.__fieldsElem.appendChild(containerElem);
            this.__fields[provider.name.toLowerCase()] = field;
        }).catch((e) => console.log(e));
    }

    protected _onClose() {
        this.resolve(null);
    }

    destroy() {
        this.__queue.destroy();

        for (const fieldName in this.__fields) {
            const field = this.__fields[fieldName];
            field.destroy();
        }

        super.destroy();
    }
}

export const editPage = (content: Content, path?: string) => {
    const dialog = new PageEditDialog(content, path);
    return dialog.open();
};