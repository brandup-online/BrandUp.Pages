import { Dialog, DialogOptions } from "./dialog";
import { DOM } from "@brandup/ui-dom";
import "./dialog-select-content-type.less";
import { ContentTypeModel } from "../content/provider/model";
import { CommandContext } from "@brandup/ui";

export class SelectContentTypeDialog extends Dialog<ContentTypeModel> {
    private __types: Array<ContentTypeModel>;

    constructor(types: Array<ContentTypeModel>, options?: DialogOptions) {
        super(options);

        this.__types = types;
    }

    get typeName(): string { return "BrandUpPages.SelectItemTypeDialog"; }

    protected _onRenderContent() {
        this.element?.classList.add("bp-dialog-select-content-type");

        this.setHeader("Выберите тип контента");

        this.__types.map((type, index) => {
            const itemElem = DOM.tag("a", { class: "item", href: "", "data-command": "select", "data-index": index }, type.title);
            this.content?.appendChild(itemElem);
        });

        this.registerCommand("select", (context: CommandContext) => {
            const index = parseInt(context.target.getAttribute("data-index") || "-1");
            if (index < 0) return;

            const type = this.__types[index];

            this.resolve(type);
        });


    }
}

export const selectContentType = (types: Array<ContentTypeModel>) => {
    const dialog = new SelectContentTypeDialog(types);
    return dialog.open();
};