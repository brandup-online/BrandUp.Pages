import { UIElement } from "@brandup/ui";
import "./base.less";
import { FieldProvider, IFieldDesigner } from "../provider/base";

export abstract class FieldDesigner<TProvider extends FieldProvider<any, any>> extends UIElement implements IFieldDesigner {
    readonly provider: TProvider;

    constructor(provider: TProvider) {
        super();
        this.provider = provider;
        
        if (provider.valueElem) {
            this.setElement(provider.valueElem);
    
            this.element?.classList.add("field-designer");
    
            this.onRender(this.element!);
        }
    }
    
    protected abstract onRender(elem: HTMLElement): void;

    setErrors(errors: string[]) {
        errors.length === 0 ? this.element?.classList.remove("invalid") : this.element?.classList.add("invalid");
    }
    
    destroy() {
        this.element?.classList.remove("field-designer");
        super.destroy();
    }
}