import { DialogOptions, Dialog } from "../dialog";
import { DOM } from "brandup-ui-dom";
import { Content } from "../../content/content";
import "../dialog-form.less";
import defs from "../../content/defs"
import { FieldProvider, IFormField } from "../../content/provider/base";
import { FormField } from "../../content/field/base";

export class PageEditDialog extends Dialog<any> {
    private __formElem: HTMLFormElement;
    private navElem: HTMLElement | null = null;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: IFormField } = {};
    private __modelPath: string;
    private __content: Content;

    constructor(content: Content, modelPath?: string, options?: DialogOptions) {
        super(options);

        this.__formElem = DOM.tag("form", { method: "POST", class: "nopad" }) as HTMLFormElement;
        this.__fieldsElem = DOM.tag("div", { class: "fields" });

        this.__content = content;
        this.__modelPath = modelPath ? modelPath : "";
    }

    get typeName(): string { return "BrandUpPages.PageEditDialog"; }
    get modelPath(): string { return this.__modelPath; }

    protected _onRenderContent() {
        this.element?.classList.add("bp-dialog-form");

        this.content?.appendChild(this.__formElem);
        this.__formElem.appendChild(this.__fieldsElem);

        this.__formElem.addEventListener("submit", (e: Event) => {
            e.preventDefault();
            return false;
        });

        this.setHeader("Контент страницы");

        this.__renderForm();

        this.registerCommand("navigate", (elem: HTMLElement) => {
            const path = elem.getAttribute("data-path");
            if (path === null || path === undefined) throw "not found attribute data-path";

            this.navigate(path);
        });
    }

    private __renderForm() {
        if (!this.navElem) {
            this.navElem = DOM.tag("ol", { class: "nav" });
            this.content?.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }

        // Breadcrumbs
        let path = this.__content.path;
        while (path || path === "") {
           const content = this.__content.host.editor.navigate(path);
           let title = content.typeTitle;
           this.navElem.insertAdjacentElement("afterbegin", DOM.tag("li", path === this.__modelPath ? { class: "current" } : null, [
               DOM.tag("a", { href: "", "data-command": "navigate", "data-path": path }, [
                   DOM.tag("bolt", null, path || "root"),
                   DOM.tag("div", null, [
                       DOM.tag("span", null, title),
                       DOM.tag("span", null, content.typeName),
                   ]),
               ]),
           ]));

           path = content.parentPath;
        }
        
        // Fields
        const fieldsArr: FieldProvider<any, any>[] = [];
        this.__content.fields.forEach(field => {
            fieldsArr.push(field);
        });

        Promise.all(fieldsArr.map(field => {
            return this.addField(field);
        }));
    }

    protected addField(provider: FieldProvider<any, any>) {
        return defs.resolveFormField(provider.type.toLowerCase()).then((type) => {
            const field: FormField<any> = new type.default(provider.title, provider.options, provider);
    
            if (this.__fields.hasOwnProperty(provider.name.toLowerCase()))
                throw `Field name "${provider.name}" already exists.`;
    
            const containerElem = DOM.tag("div", { class: "field" });
    
            provider.registerForm(field);
            field.render(containerElem);
            
            this.__fieldsElem.appendChild(containerElem);
            this.__fields[provider.name.toLowerCase()] = field;
        }).catch((e) => console.log(e));
    }

    navigate(modelPath: string) {
        this.destroyFields();
        this.__modelPath = modelPath ? modelPath : "";

        this.__content = this.__content.host.editor.navigate(modelPath);
        this.__renderForm();
    }

    getField(name: string): IFormField {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw `Field "${name}" not exists.`;
        return this.__fields[name.toLowerCase()];
    }

    protected _onClose() {
        this.resolve(null);
    }

    destroyFields() {
        for (const key in this.__fields) {
            this.__fields[key].destroy();
        }
        this.__fields = {};
        DOM.empty(this.__fieldsElem);
    }

    destroy() {
        this.destroyFields();
        super.destroy();
    }
}

export const editPage = (content: Content, path?: string) => {
    const dialog = new PageEditDialog(content, path);
    return dialog.open();
};