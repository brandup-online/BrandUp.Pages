import { UIElement } from "brandup-ui";
import { FieldProvider, IFormField } from "../provider/base";
import { DOM } from "brandup-ui-dom";
import { IFieldValueElement } from "../../typings/content";

export abstract class FormField<TOptions> extends UIElement implements IFormField {
    readonly provider: FieldProvider<any, any>;
    protected __errorsElem: HTMLElement = DOM.tag("ul", { class: "field-errors" });
    protected __valueElem: IFieldValueElement | null = null;
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
        this.__valueElem = this._renderValueElem();

        if (!this.__valueElem.element) return;

        const container = DOM.tag("div", {class: "website-form-field"}, [
            DOM.tag("label", null, this.caption),
            this.__valueElem.element,
            this.__errorsElem
        ])
        
        this.setElement(container);

        this.__valueElem.onChange(value => this.provider.saveValue(value));

        ownElem.appendChild(this.element!);

        this.raiseUpdateErrors(this.provider.errors);
        this._setValue(this.provider.getValue());
    }

    raiseUpdateErrors(errors: Array<string>) {
        this.__errorsElem.innerHTML = '';
        if (!errors || errors.length === 0) {
            this.element?.classList.remove("has-errors");
            return;
        }

        this.element?.classList.add("has-errors");
        for (let i = 0; i < errors.length; i++)
            this.__errorsElem.appendChild(DOM.tag("li", null, errors[i]));
    }

    raiseUpdateValue(value: any) {
        this._setValue(value);
    };

    protected abstract _renderValueElem(): IFieldValueElement;

    protected _setValue(value: any) {
        this.__valueElem?.setValue(value);
    }

    getValue() {
        return this.__valueElem?.getValue();
    };

    protected _onChanged() {
        const value = this.getValue();
        this.provider.saveValue(value);
    }

    destroy(): void {
        this.__valueElem?.destroy();
        
        this.element?.remove();
        super.destroy();
    }
}