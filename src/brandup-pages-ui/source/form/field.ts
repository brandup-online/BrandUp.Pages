import { DOM } from "brandup-ui-dom";
import { UIControl } from "../control";
import "./field.less";

export abstract class Field<TValue, TOptions> extends UIControl<TOptions> {
    readonly name: string;
    private __errorsElem: HTMLElement;

    constructor(name: string, options: TOptions) {
        super(options);

        this.name = name;
    }

    protected _onRender() {
        this.element.classList.add("website-form-field");

        this.defineEvent("changed", { bubbles: true, cancelable: false });
    }

    protected raiseChanged() {
        this.raiseEvent("changed", {
            field: this,
            value: this.getValue()
        });
    }

    abstract getValue(): TValue;
    abstract setValue(value: TValue);
    abstract hasValue(): boolean;

    setErrors(errors: Array<string>) {
        this.element.classList.remove("has-errors");
        if (this.__errorsElem) {
            this.__errorsElem.remove();
            this.__errorsElem = null;
        }

        if (!errors || errors.length === 0) {
            return;
        }

        this.element.classList.add("has-errors");
        this.__errorsElem = DOM.tag("ul", { class: "field-errors" });
        for (let i = 0; i < errors.length; i++)
            this.__errorsElem.appendChild(DOM.tag("li", null, errors[i]));
        this.element.insertAdjacentElement("afterend", this.__errorsElem);
    }
}