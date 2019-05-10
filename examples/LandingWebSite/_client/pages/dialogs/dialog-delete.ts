import { Dialog, DialogOptions } from "./dialog";
import { DOM, ajaxRequest } from "brandup-ui";
import "./dialog-delete.less";

export abstract class DeleteDialog<TItem> extends Dialog<TItem> {
    private __textElem: HTMLElement;
    private __errorsElem: HTMLElement;
    private __item: TItem;
    
    protected _onRenderContent() {
        this.element.classList.add("website-dialog-delete");
        
        this.addAction("close", "Отмена");
        this.addAction("confirm", "Удалить", true);

        this.registerCommand("confirm", () => {
            this.__delete();
        });

        this.__textElem = DOM.tag("div", { class: "confirm-text" }, this._getText());
        this.content.appendChild(this.__textElem);
        this.__errorsElem = DOM.tag("div", { class: "errors" });
        this.content.appendChild(this.__errorsElem);

        var urlParams: { [key: string]: string; } = {};

        this._buildUrlParams(urlParams);

        this.setLoading(true);
        ajaxRequest({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "GET",
            success: (data: TItem, status: number) => {
                this.setLoading(false);

                switch (status) {
                    case 404: {
                        this.setError("Данные для удаления не найдены.");
                        break;
                    }
                    case 200: {
                        this.__item = data;
                        break;
                    }
                    default:
                        throw "";
                }
            }
        });
    }

    private __delete() {
        this.setLoading(true);

        var urlParams: { [key: string]: string; } = {};

        this._buildUrlParams(urlParams);

        ajaxRequest({
            url: this._buildUrl(),
            urlParams: urlParams,
            method: "DELETE",
            success: (data: any, status: number) => {
                this.setLoading(false);

                switch (status) {
                    case 400: {
                        this.__renderErrors((<Result>data).errors);
                        break;
                    }
                    case 200: {
                        this.resolve(this.__item);
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

    protected abstract _getText(): string;
    protected abstract _buildUrl(): string;
    protected abstract _buildUrlParams(urlParams: { [key: string]: string; });
}