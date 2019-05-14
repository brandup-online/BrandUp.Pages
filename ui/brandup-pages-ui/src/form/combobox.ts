import { Field } from "./field";
import { DOM } from "brandup-ui";
import "./combobox.less";

export class ComboBoxField extends Field<string, ComboBoxFieldOptions> {
    private __valueElem: HTMLElement;
    private __itemsElem: HTMLElement;
    private __value: string = null;
    private __isChanged: boolean;

    get typeName(): string { return "BrandUpPages.Form.ComboBoxField"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("combobox");
        this.element.setAttribute("tabindex", "0");

        this.__valueElem = <HTMLInputElement>DOM.tag("div", { class: "value" });
        this.element.appendChild(this.__valueElem);

        let placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "toggle" }, this.options.placeholder);
        this.element.appendChild(placeholderElem);

        this.__itemsElem = <HTMLInputElement>DOM.tag("ul");
        this.element.appendChild(this.__itemsElem);

        var isFocused = false;
        var md = false;
        this.addEventListener("focus", () => {
            isFocused = true;
        });
        this.addEventListener("blur", () => {
            isFocused = false;
        });

        placeholderElem.addEventListener("mousedown", () => {
            md = isFocused;
        });

        placeholderElem.addEventListener("mouseup", () => {
            if (md && isFocused)
                this.element.blur();
        });

        this.registerCommand("select", (elem: HTMLElement) => {
            DOM.removeClass(this.__itemsElem, ".selected", "selected");

            elem.classList.add("selected");

            this.__value = elem.getAttribute("data-value");
            this.__valueElem.innerText = elem.innerText;

            this.__refreshUI();

            this.element.blur();

            this.raiseChanged();
        });
    }

    private __refreshUI() {
        let hasVal = this.hasValue();
        if (hasVal)
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }

    addItem(item: ComboBoxItem) {
        this.__itemsElem.appendChild(DOM.tag("li", { "data-value": item.value, "data-command": "select" }, item.title));
    }
    addItems(items: Array<ComboBoxItem>) {
        if (items) {
            for (var i = 0; i < items.length; i++)
                this.addItem(items[i]);
        }
    }
    clearItems() {
        DOM.empty(this.__valueElem);

        this.__value = null;
    }

    getValue(): string {
        return this.__value;
    }
    setValue(value: string) {
        var text: string = "";
        if (value !== null) {
            var itemElem = DOM.queryElement(this.__itemsElem, `li[data-value="${value}"]`);
            if (!itemElem) {
                this.setValue(null);
                return;
            }
            text = itemElem.innerText;
            itemElem.classList.add("selected");
        }
        else
            DOM.removeClass(this.__itemsElem, ".selected", "selected");

        this.__value = value;
        this.__valueElem.innerText = text;

        this.__refreshUI();
    }
    hasValue(): boolean {
        var val = this.__value;
        return val ? true : false;
    }
}
export interface ComboBoxFieldOptions {
    placeholder?: string;
    emptyText?: string;
}
export interface ComboBoxItem {
    value: string;
    title: string;
}