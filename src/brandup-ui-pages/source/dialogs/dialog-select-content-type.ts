﻿import { Dialog, DialogOptions } from "./dialog";
import { DOM } from "brandup-ui-dom";
import { ContentTypeModel } from "../typings/models";
import "./dialog-select-content-type.less";

export class SelectContentTypeDialog extends Dialog<ContentTypeModel> {
    private __types: Array<ContentTypeModel>;

    constructor(types: Array<ContentTypeModel>, options?: DialogOptions) {
        super(options);

        this.__types = types;
    }

    get typeName(): string { return "BrandUpPages.SelectItemTypeDialog"; }

    protected _onRenderContent() {
        this.element.classList.add("bp-dialog-select-content-type");

        this.setHeader("Выберите тип контента");

        this.__types.map((type, index) => {
            const itemElem = DOM.tag("a", { class: "item", href: "", "data-command": "select", "data-index": index }, type.title);
            this.content.appendChild(itemElem);
        });

        this.registerCommand("select", (elem: HTMLElement) => {
            const index = parseInt(elem.getAttribute("data-index"));
            const type = this.__types[index];

            this.resolve(type);
        });


    }
}

export const selectContentType = (types: Array<ContentTypeModel>) => {
    const dialog = new SelectContentTypeDialog(types);
    return dialog.open();
};