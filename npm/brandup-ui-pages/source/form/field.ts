import { DOM } from "@brandup/ui-dom";
import { UIControl } from "../control";
import "./field.less";

export abstract class Field<TValue, TOptions> extends UIControl<TOptions> {
    readonly name: string;
    private __errorsElem?: HTMLElement;

    constructor(name: string, options: TOptions) {
        super(options);

        this.name = name;
    }

    protected _onRender() {
        if (!this.element)
            return;

        this.element.classList.add("website-form-field");

        //this.element.defineEvent("changed", { bubbles: true, cancelable: false });
    }

    protected raiseChanged() {
        this.element.dispatchEvent(new CustomEvent("changed", {
            detail: {
                field: this,
                value: this.getValue()
            }
        }));
    }

    abstract getValue(): TValue | null;
    abstract setValue(value: TValue): void;
    abstract hasValue(): boolean;

    setErrors(errors: Array<string>) {
        if (!this.element)
            return;

        this.element.classList.remove("has-errors");
        if (this.__errorsElem) {
            this.__errorsElem.remove();
            delete this.__errorsElem;
        }

        if (!errors || errors.length === 0)
            return;

        this.element.classList.add("has-errors");
        this.__errorsElem = DOM.tag("ul", { class: "field-errors" });
        for (let i = 0; i < errors.length; i++)
            this.__errorsElem.appendChild(DOM.tag("li", null, errors[i]));
        this.element.insertAdjacentElement("afterend", this.__errorsElem);
    }
}