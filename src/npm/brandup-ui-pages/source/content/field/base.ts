import { UIElement } from "@brandup/ui";
import { FieldProvider, IFormField } from "../provider/base";
import { DOM } from "@brandup/ui-dom";
import { IFieldValueElement } from "../../typings/content";
import LanguageIcon from "../../svg/new/language.svg";
import defs from "../../content/defs";

export abstract class FormField<TOptions> extends UIElement implements IFormField {
    readonly provider: FieldProvider<any, any>;
    protected __errorsElem: HTMLElement;
    protected __valueElem?: IFieldValueElement;
    readonly options: TOptions;
    readonly caption: string;

    get typeName(): string { return "BrandUpPages.Form.Field.Text"; }

    constructor(caption: string, options: TOptions, provider: FieldProvider<any, any>) {
        super();
        this.provider = provider;
        this.options = options;
        this.caption = caption;
        this.__errorsElem = DOM.tag("ul", { class: "field-errors" });
    }

    render(ownElem: HTMLElement) {
        this.__valueElem = this.renderValueElem();

        if (!this.__valueElem.element) return;

        const container = DOM.tag("div", {class: "website-form-field"}, [
            DOM.tag("div", { class: "caption" }, [
                DOM.tag("label", null, this.caption),
                DOM.tag("a", { href: "", command: "localization" }, LanguageIcon)
            ]),
            this.__valueElem.element,
            this.__errorsElem
        ])
        
        this.setElement(container);

        this.__valueElem.onChange((value: any) => this.provider.saveValue(value));

        ownElem.appendChild(this.element!);

        this.raiseUpdateErrors(this.provider.errors);
        this._setValue(this.provider.getValue());

        this.registerCommand("localization", () => {
            this.provider.showLocalization();
        })
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

    abstract renderValueElem(): IFieldValueElement;

    static async getValueElem(provider: FieldProvider<any, any>): Promise<IFieldValueElement> {
        const fildType = await defs.resolveFormField(provider.type.toLowerCase())
        const field: FormField<any> = new fildType.default(provider.title, provider.options, provider);
        return field.renderValueElem();
    }

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