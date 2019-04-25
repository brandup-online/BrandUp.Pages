import { Dialog, DialogOptions } from "./dialog";
import { ajaxRequest, DOM } from "brandup-ui";

class PageCollectionDeleteDialog extends Dialog<string> {
    readonly id: string;
    private __textElem: HTMLElement;
    private __errorsElem: HTMLElement;

    constructor(id: string, options?: DialogOptions) {
        super(options);

        this.id = id;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionDeleteDialog"; }

    protected _onRenderContent() {
        this.element.classList.add("website-dialog-delete");

        this.setHeader("Удаление коллекции страниц");
        this.addAction("close", "Отмена");
        this.addAction("confirm", "Удалить", true);

        this.registerCommand("confirm", () => {
            this.__delete();
        });

        this.__textElem = DOM.tag("div", { class: "confirm-text" }, "Подтвердите удаление коллекции страниц.");
        this.content.appendChild(this.__textElem);
        this.__errorsElem = DOM.tag("div", { class: "errors" });
        this.content.appendChild(this.__errorsElem);

        ajaxRequest({
            url: `/brandup.pages/collection/${this.id}`,
            success: (data: PageCollectionModel, status: number) => {
                switch (status) {
                    case 200:
                        break;
                    case 404:
                        break;
                    default:
                        throw "";
                }
            }
        });
    }

    private __delete() {
        this.setLoading(true);

        ajaxRequest({
            url: `/brandup.pages/collection/${this.id}`,
            method: "DELETE",
            success: (data: any, status: number) => {
                this.setLoading(false);

                switch (status) {
                    case 400: {
                        this.__renderErrors((<Result>data).errors);
                        break;
                    }
                    case 200: {
                        this.resolve(this.id);
                        return;
                    }
                    default:
                        throw "";
                }
            }
        });
    }

    private __renderErrors(errors: Array<string>) {
        DOM.empty(this.__errorsElem);

        if (errors) {
            var elem = DOM.tag("ul");
            for (var i = 0; i < errors.length; i++) {
                elem.appendChild(DOM.tag("li", null, errors[i]));
            }
            this.__errorsElem.appendChild(elem);
        }
    }
}

export var deletePageCollection = (id: string) => {
    let dialog = new PageCollectionDeleteDialog(id);
    return dialog.open();
};