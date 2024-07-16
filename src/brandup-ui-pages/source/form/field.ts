import { DOM } from "brandup-ui-dom";
import { UIControl } from "../control";
import "./field.less";

export abstract class Field<TValue, TOptions> extends UIControl<TOptions> {
    readonly name: string;
    private __errorsElem: HTMLElement;

    constructor(name: string, errors: string[], options: TOptions) {
        super(options);

        this.__errorsElem = DOM.tag("ul", { class: "field-errors" });
        this.element?.insertAdjacentElement("afterend", this.__errorsElem);
        this.name = name;
        this.setErrors(errors);
    }

    protected _onRender() {
        this.element?.classList.add("website-form-field");

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
        this.__errorsElem.innerHTML = '';
        if (!errors || errors.length === 0) {
            this.element?.classList.remove("has-errors");
            return;
        }

        this.element?.classList.add("has-errors");
        for (let i = 0; i < errors.length; i++)
            this.__errorsElem.appendChild(DOM.tag("li", null, errors[i]));
    }
}