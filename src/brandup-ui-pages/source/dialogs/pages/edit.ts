import { DialogOptions, Dialog } from "../dialog";
import { AjaxQueue } from "brandup-ui-ajax";
import { IContentForm, IContentField, IContentEditor, IContent } from "../../typings/content";
import "../dialog-form.less";
import { DOM } from "brandup-ui-dom";

export class PageEditDialog extends Dialog<any> implements IContentForm {
    private __formElem: HTMLFormElement;
    private navElem: HTMLElement;
    private __fieldsElem: HTMLElement;
    private __fields: { [key: string]: IContentField } = {};
    private __modelPath: string;
    private __queue: AjaxQueue;
    private __content: IContent;

    constructor(editor: IContentEditor, modelPath?: string, options?: DialogOptions) {
        super(options);

        this.__content = editor.navigate(modelPath);
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

        this.__renderForm();

        this.registerCommand("navigate", (elem: HTMLElement) => {
            const path = elem.getAttribute("data-path");
            this.navigate(path);
        });
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
            //this.addField(field.title, field.createField());
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

    getField(name: string): IContentField {
        if (!this.__fields.hasOwnProperty(name.toLowerCase()))
            throw `Field "${name}" not exists.`;
        return this.__fields[name.toLowerCase()];
    }
    protected addField(title: string, field: IContentField) {
        if (this.__fields.hasOwnProperty(field.name.toLowerCase()))
            throw `Field name "${field.name}" already exists.`;

        const containerElem = DOM.tag("div", { class: "field" });

        if (title)
            containerElem.appendChild(DOM.tag("label", { for: field.name }, title));

        field.render(containerElem);

        this.__fieldsElem.appendChild(containerElem);
        this.__fields[field.name.toLowerCase()] = field;
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

export const editPage = (editor: IContentEditor, path?: string) => {
    const dialog = new PageEditDialog(editor, path);
    return dialog.open();
};