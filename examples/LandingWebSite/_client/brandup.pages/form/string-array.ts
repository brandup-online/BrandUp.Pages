import { Field } from "./field";
import { DOM } from "brandup-ui";
import "./string-array.less";
import iconDelete from "../svg/toolbar-button-discard.svg";

export class StringArrayField extends Field<Array<string>, StringArrayFieldOptions> {
    private __itemsElem: HTMLElement;
    private __isChanged: boolean;
    private __items: Array<string> = [];

    get typeName(): string { return "BrandUpPages.Form.Field.StringArray"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("string-array");

        this.__itemsElem = <HTMLInputElement>DOM.tag("div", { class: "items" });
        this.element.appendChild(this.__itemsElem);

        this.__renderItems();

        this.__refreshUI();

        this.element.addEventListener("keydown", (e: Event) => {
            let t = <HTMLInputElement>e.target;
            let elem = t.closest(".item");
            let index = parseInt(elem.getAttribute("data-index"));
            if (t.value)
                elem.classList.add("has-value");
            else
                elem.classList.remove("has-value");

            if (index == this.__items.length)
                this.__itemsElem.appendChild(this.__createItemElem("", index + 1));
        });
    }

    private __renderItems() {
        DOM.empty(this.__itemsElem);

        let i = 0;
        for (let i = 0; i < this.__items.length; i++) {
            let item = this.__items[i];
            this.__itemsElem.appendChild(this.__createItemElem(item, i));
        }

        this.__itemsElem.appendChild(this.__createItemElem("", i));
    }
    private __createItemElem(item: string, index: number) {
        let itemElem = DOM.tag("div", { class: "item", "data-index": index.toString(), draggable: "true" }, [
            DOM.tag("div", { class: "index", title: "Нажмите, чтобы перетащить" }, `#${index + 1}`),
            DOM.tag("input", { type: "text", value: item, placeholder: this.options.placeholder ? this.options.placeholder : "" }),
            DOM.tag("ul", null, [
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-delete", title: "Удалить" }, iconDelete))
            ])
        ]);

        if (item)
            itemElem.classList.add("has-value");

        return itemElem;
    }

    protected _onChanged() {
        this.__refreshUI();

        this.raiseChanged();
    }
    private __refreshUI() {
        let hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }

    getValue(): Array<string> {
        if (this.hasValue())
            return this.__items;
        return null;
    }
    setValue(value: Array<string>) {
        var temp = new Array<string>();

        if (value) {
            for (let i = 0; i < value.length; i++) {
                let item = value[i];
                if (!item)
                    continue;

                temp.push(item);
            }
        }

        this.__items = temp;
    }
    hasValue(): boolean {
        return this.__items !== null && this.__items.length !== 0;
    }
}
export interface StringArrayFieldOptions {
    placeholder?: string;
    maxItems?: number;
}