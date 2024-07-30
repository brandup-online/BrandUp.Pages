import { AjaxQueue } from "@brandup/ui-ajax";
import { FieldProvider } from "../../content/provider/base";
import { Dialog, DialogOptions } from "../dialog";
import { DOM } from "@brandup/ui-dom";

export class LocalizationDialog extends Dialog {
    private __queue?: AjaxQueue;

    get typeName(): string { return "BrandUpPages.ErrorDialog"; }

    constructor(options?: DialogOptions) {
        super(options);
    }

    protected _onRenderContent() {
        this.setHeader("Локализация контента");

        if (!this.content) throw new Error("dialog content is not defined");


    }

    private __loadContent() {
        if (this.__queue)
            this.__queue.destroy();
        this.__queue = new AjaxQueue();

        // const values = this.__queue.enque()
    }

    destroy() {
        this.__queue?.destroy();

        super.destroy();
    }
}

export const localizationContent = (provider: FieldProvider<any, any>) => {
    const dialog = new LocalizationDialog();
    return dialog.open();
};