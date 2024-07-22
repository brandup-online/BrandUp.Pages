import { ContentEditor } from "../content/editor";
import { Dialog, DialogOptions } from "./dialog";
import { DOM } from "@brandup/ui-dom";
import infoIcon from "../svg/new/info.svg";
import "./dialog-error.less";
import { ValidationContentModel } from "../typings/content";
import { editPage } from "./content/edit";

export class ErrorDialog extends Dialog {
    private __errors: ValidationContentModel[] = [];
    private __listElem: HTMLElement;

    get typeName(): string { return "BrandUpPages.ErrorDialog"; }

    constructor(editor: ContentEditor, errors: ValidationContentModel[], options?: DialogOptions) {
        super(options);

        this.__listElem = DOM.tag("ul", { class: "error-list" });

        this.__errors = errors;
        this.registerCommand("navigate", (context) => {
            const contentPath = context.target.dataset.contentPath;
            if (!contentPath) throw "data-content-path attribute not found"

            const content = editor.navigate(contentPath);
            if (!content) throw `content path ${contentPath} not found`;
            editPage(content, contentPath).then(() => {
                this.__errors = editor.validate();
                if (this.__errors.length === 0) this._onClose();
                this.__refreshErrors();
            });
        })
    }

    protected _onRenderContent() {
        this.setHeader("Ошибка");

        this.content?.appendChild(this.__listElem);
        this.__refreshErrors();
    }

    private __refreshErrors() {
        DOM.empty(this.__listElem);

        this.__errors.forEach(err => {
            this._addError(err);
        })
    }

    protected _addError(error: ValidationContentModel) {
        const containerElem = DOM.tag("li", { class: "content-error-block" }, [
            DOM.tag("hgroup", null, [DOM.tag("span", null, error.path), DOM.tag("h3", null, error.typeTitle)]),
            error.errors.length === 0 ? "" : DOM.tag("ul", {class: "error-values"}, error.errors.map(err => DOM.tag("li", null, err))),
            error.fields.length === 0 ? "" : DOM.tag("ul", {class: "fields-errors"}, error.fields.map(fieldError => {
                return DOM.tag("li", null, [
                    DOM.tag("hgroup", {class: "header"}, [DOM.tag("span", null, fieldError.name), DOM.tag("h4", null, fieldError.title)]),
                    fieldError.errors.length === 0 ? "" : DOM.tag("ul", null, ...fieldError.errors.map(err => DOM.tag("li", {class: "field-error"}, [infoIcon, err]))),
                ])
            })),
            DOM.tag("a", { href: "#", command: "navigate", "data-content-path": error.path }, "Редактировать")
        ]);

        this.__listElem.appendChild(containerElem);
    }
}

export const errorPage = (editor: ContentEditor, errors: ValidationContentModel[]) => {
    const dialog = new ErrorDialog(editor, errors);
    return dialog.open();
};