import { UIElement } from "brandup-ui";
import { FieldProvider, IFormField } from "../provider/base";
import { DOM } from "brandup-ui-dom";

export abstract class FormField<TOptions> extends UIElement implements IFormField {
    readonly provider: FieldProvider<any, any>;
    protected __errorsElem: HTMLElement;
    protected __valueElem: HTMLElement;
    readonly options: TOptions;
    readonly caption: string;

    get typeName(): string { return "BrandUpPages.Form.Field.Text"; }

    constructor(caption: string, options: TOptions, provider: FieldProvider<any, any>) {
        super();
        this.provider = provider;
        this.options = options;
        this.caption = caption;
    }

    render(ownElem: HTMLElement) {
        const container = DOM.tag("div", {class: "website-form-field"}, [
            DOM.tag("label", null, this.caption),
            this.__valueElem = this._renderValueElem(),
            this.__errorsElem = DOM.tag("ul", { class: "field-errors" })
        ])
        
        this.setElement(container);

        ownElem.appendChild(this.element);
    }

    raiseUpdateErrors(errors: Array<string>) {
        this.__errorsElem.innerHTML = '';
        if (!errors || errors.length === 0) {
            this.element.classList.remove("has-errors");
            return;
        }

        this.element.classList.add("has-errors");
        for (let i = 0; i < errors.length; i++)
            this.__errorsElem.appendChild(DOM.tag("li", null, errors[i]));
    }

    raiseUpdateValue(value: any) {
        this._setValue(value);
    };

    protected abstract _renderValueElem(): HTMLElement;

    protected abstract _setValue(value: any);

    abstract getValue();

    protected _onChanged() {
        const value = this.getValue();
        this.provider.saveValue(value);
    }
}