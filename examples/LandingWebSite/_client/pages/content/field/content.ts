import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import { DOM } from "brandup-ui";
import "./content.less";
import iconEdit from "../../svg/toolbar-button-edit.svg";
import iconDelete from "../../svg/toolbar-button-discard.svg";
import { editPage } from "../../dialogs/page-edit";

export class ContentField extends Field<ContentFieldFormValue, ContentDesignerOptions> implements IContentField {
    readonly form: IContentForm;
    private __value: ContentFieldFormValue;
    private __itemsElem: HTMLElement;

    constructor(form: IContentForm, name: string, options: ContentDesignerOptions) {
        super(name, options);

        this.form = form;
    }

    get typeName(): string { return "BrandUpPages.Form.Field.Content"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("content");

        this.__itemsElem = DOM.tag("div", { class: "items" });
        this.element.appendChild(this.__itemsElem);

        this.registerCommand("item-settings", (elem: HTMLElement) => {
            let itemElem = elem.closest("[data-content-index]");
            let itemIndex = itemElem.getAttribute("data-content-index");
            let contentPath = `${this.form.contentPath}.${this.name}[${itemIndex}]`;

            editPage(this.form.editId, contentPath).then(() => {
                //this.__refreshItem(itemElem);
            });
        });
    }

    getValue(): ContentFieldFormValue {
        return this.__value;
    }
    setValue(value: ContentFieldFormValue) {
        this.__value = value;

        this.__renderItems();
    }
    hasValue(): boolean {
        return this.__value && this.__value.items && this.__value.items.length > 0;
    }

    private __renderItems() {
        DOM.empty(this.__itemsElem);

        for (let i = 0; i < this.__value.items.length; i++) {
            let item = this.__value.items[i];

            let itemElem = DOM.tag("div", { class: "item", "data-content-index": i.toString() }, [
                DOM.tag("div", { class: "index" }, `#${i + 1}`),
                DOM.tag("a", { href: "", class: "title", "data-command": "item-settings" }, item.type.title),
                DOM.tag("ul", null, [
                    DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-settings" }, iconEdit)),
                    DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-delete" }, iconDelete))
                ])
            ]);
            this.__itemsElem.appendChild(itemElem);
        }
    }
}

export interface ContentDesignerOptions {
    isListValue: boolean;
    itemTypes: Array<ContentItemType>;
}

export interface ContentFieldFormValue {
    items: Array<ContentItem>;
}

export interface ContentItemType {
    name: string;
    title: string;
}

export interface ContentItem {
    type: ContentItemType;
}