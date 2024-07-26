import { Dialog } from "./dialog";
import { DOM } from "@brandup/ui-dom";
import { Result } from "../typings/page";
import { request, AjaxResponse } from "@brandup/ui-ajax";
import "./dialog-delete.less";

export abstract class DeleteDialog<TItem> extends Dialog<TItem> {
    private __errorsElem?: HTMLElement;
    private __item: TItem | null = null;

    protected async _onRenderContent() {
        this.element?.classList.add("bp-dialog-delete");

        this.addAction("close", "Отмена");
        this.addAction("confirm", "Удалить", true);

        this.registerCommand("confirm", () => {
            this.__delete();
        });

        this.content?.appendChild(DOM.tag("div", { class: "confirm-text" }, this._getText()));
        this.content?.appendChild(this.__errorsElem = DOM.tag("div", { class: "errors" }));

        const urlParams: { [key: string]: string } = {};

        this._buildUrlParams(urlParams);

        this.setLoading(true);
        const response: AjaxResponse<TItem> = await request({
            url: this._buildUrl(),
            query: urlParams,
            method: "GET",
        });

        this.setLoading(false);

        switch (response.status) {
            case 404: {
                this.setError("Данные для удаления не найдены.");
                break;
            }
            case 200: {
                this.__item = response.data;
                break;
            }
            default:
                throw new Error("");
        }
    }

    private async __delete() {
        this.setLoading(true);

        const urlParams: { [key: string]: string } = {};

        this._buildUrlParams(urlParams);

        const response: AjaxResponse = await request({
            url: this._buildUrl(),
            query: urlParams,
            method: "DELETE",
        });

        this.setLoading(false);

        switch (response.status) {
            case 400: {
                this.__renderErrors((response.data as Result).errors);
                break;
            }
            case 200: {
                if (this.__item)
                    this.resolve(this.__item);
                return;
            }
            default:
                throw new Error("");
        }
    }

    private __renderErrors(errors: Array<string>) {
        if (!this.__errorsElem) return;
        DOM.empty(this.__errorsElem);

        if (errors) {
            const elem = DOM.tag("ul");
            for (let i = 0; i < errors.length; i++) {
                elem.appendChild(DOM.tag("li", null, errors[i]));
            }
            this.__errorsElem.appendChild(elem);
        }
    }

    protected abstract _getText(): string;
    protected abstract _buildUrl(): string;
    protected abstract _buildUrlParams(urlParams: { [key: string]: string }): void;
}