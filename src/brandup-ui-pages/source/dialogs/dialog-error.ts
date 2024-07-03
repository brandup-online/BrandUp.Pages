import { ValidationContentModel } from "../typings/models";
import { Editor } from "../content/editor";
import { Dialog, DialogOptions } from "./dialog";
import { DOM } from "brandup-ui-dom";
import infoIcon from "../svg/new/info.svg";
import "./dialog-error.less";

export class ErrorDialog extends Dialog {
    private __errors: ValidationContentModel[];
    private __listElem: HTMLElement

    get typeName(): string { return "BrandUpPages.ErrorDialog"; }

    constructor(editor: Editor, errors: ValidationContentModel[], options?: DialogOptions) {
        super(options);

        this.__errors = errors;
    }

    protected _onRenderContent() {
        this.setHeader("Ошибка");

        this.content.appendChild(this.__listElem = DOM.tag("div", { class: "error-list" }));

        this.__errors.forEach(err => {
            this._addError(err);
        })
    }

    protected _addError(error: ValidationContentModel) {
        const containerElem = DOM.tag("div", { class: "content-error-block" }, [
            DOM.tag("div", null, [DOM.tag("span", null, error.path), DOM.tag("h3", null, error.typeTitle)]),
            error.errors.length === 0 ? null : DOM.tag("ul", {class: "error-values"}, error.errors.map(err => DOM.tag("li", null, err))),
            error.fields.length === 0 ? null : DOM.tag("ul", {class: "fields-errors"}, error.fields.map(fieldError => {
                return DOM.tag("li", null, [
                    DOM.tag("div", {class: "header"}, [DOM.tag("span", null, fieldError.name), DOM.tag("h4", null, fieldError.title)]),
                    fieldError.errors.length === 0 ? null : DOM.tag("ul", null, ...fieldError.errors.map(err => DOM.tag("li", {class: "field-error"}, [infoIcon, err]))),
                ])
            })),
            DOM.tag("a", { href: "", "data-content-path": error.path }, "Редактировать")
        ]);

        this.__listElem.appendChild(containerElem);
    }
}

export const errorPage = (editor: Editor, errors: ValidationContentModel[]) => {
    const dialog = new ErrorDialog(editor, errors);
    return dialog.open();
};