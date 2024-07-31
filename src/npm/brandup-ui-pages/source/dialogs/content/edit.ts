import { DialogOptions, Dialog } from "../dialog";
import { DOM } from "@brandup/ui-dom";
import { Content } from "../../content/content";
import "../dialog-form.less";
import defs from "../../content/defs"
import { FieldProvider, IFormField } from "../../content/provider/base";
import { FormField } from "../../content/field/base";
import { CommandContext } from "@brandup/ui";
import { Toggler } from "./components/toggler";
import { Breadcrumbs } from "./components/breadcrumbs";

export class PageEditDialog extends Dialog<any> {
    private __formElem: HTMLFormElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: IFormField } = {};
    private __modelPath: string;
    private __content: Content;
    private __toggler?: Toggler;
    private __breadcrumbs?: Breadcrumbs;

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

        this.__formElem.appendChild(this.__fieldsElem);

        this.__formElem.addEventListener("submit", (e: Event) => {
            e.preventDefault();
            return false;
        });

        this.registerCommand("navigate", (context: CommandContext) => {
            const path = context.target.getAttribute("data-path");
            if (path === null || path === undefined) throw new Error("not found attribute data-path");

            this.navigate(path);
        });

        this.setHeader("Контент страницы");

        this.__renderForm();
    }

    private __renderForm() {
        if (!this.content) throw new Error("dialog content is not defined");

        // Breadcrumbs
        if (!this.__breadcrumbs) {
            this.__breadcrumbs = new Breadcrumbs(this.__content.host.editor);
            this.__breadcrumbs.on("navigate", (path) => this.navigate(path));
        }
        if (!this.__breadcrumbs.element) throw new Error("Breadcrumbs render error");
        this.__breadcrumbs.render(this.__content.path, this.__modelPath);
        this.content.insertAdjacentElement("afterbegin", this.__breadcrumbs.element);

        // Toggler
        if (!this.__toggler) {
            this.__toggler = new Toggler({
                items: [{ value: "ru", content: ["RU", DOM.tag("span", null, "100%")] }, { value: "en", content: ["EN", DOM.tag("span", null, "50%")] }],
                defaultValue: "en",
            })
            if (!this.__toggler.element) throw new Error("toggler creating error");
            this.__toggler.on("change", (val: string) => {
                // TODO переключение языка
            })
            this.content.appendChild(this.__toggler.element);
        }

        this.content.appendChild(this.__formElem);
        
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
                throw new Error(`Field name "${provider.name}" already exists.`);
    
            const containerElem = DOM.tag("div", { class: "field" });
    
            provider.registerForm(field);
            field.render(containerElem);
            
            this.__fieldsElem.appendChild(containerElem);
            this.__fields[provider.name.toLowerCase()] = field;
        }).catch((e) => console.error(e));
    }

    navigate(modelPath: string) {
        this.destroyFields();
        this.__modelPath = modelPath ? modelPath : "";

        this.__content = this.__content.host.editor.navigate(modelPath);
        this.__renderForm();
    }

    getField(name: string): IFormField {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw new Error(`Field "${name}" not exists.`);
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
        this.__toggler?.destroy();
        this.destroyFields();
        super.destroy();
    }
}

export const editPage = (content: Content, path?: string) => {
    const dialog = new PageEditDialog(content, path);
    return dialog.open();
};