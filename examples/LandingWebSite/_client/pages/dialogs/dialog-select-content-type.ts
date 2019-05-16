import { Dialog, DialogOptions } from "./dialog";
import { DOM } from "brandup-ui";
import "./dialog-select-content-type.less";

export class SelectContentTypeDialog extends Dialog<ContentTypeModel> {
    private __types: Array<ContentTypeModel>;

    constructor(types: Array<ContentTypeModel>, options?: DialogOptions) {
        super(options);

        this.__types = types;
    }

    get typeName(): string { return "BrandUpPages.SelectItemTypeDialog"; }

    protected _onRenderContent() {
        this.element.classList.add("website-dialog-select-content-type");

        this.setHeader("Выберите тип элемента");

        this.__types.map((type, index) => {
            let itemElem = DOM.tag("a", { class: "item", href: "", "data-command": "select", "data-index": index }, type.title);
            this.content.appendChild(itemElem);
        });

        this.registerCommand("select", (elem: HTMLElement) => {
            let index = parseInt(elem.getAttribute("data-index"));
            let type = this.__types[index];

            this.resolve(type);
        });
    }
}

export var selectContentType = (types: Array<ContentTypeModel>) => {
    let dialog = new SelectContentTypeDialog(types);
    return dialog.open();
};