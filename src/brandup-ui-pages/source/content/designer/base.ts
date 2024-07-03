import { UIElement } from "brandup-ui";
import { IContentFieldDesigner, IContentFieldProvider } from "../../typings/content";
import "./base.less";

export abstract class FieldDesigner<TProvider extends IContentFieldProvider> extends UIElement implements IContentFieldDesigner {
    readonly provider: TProvider;

    constructor(provider: TProvider) {
        super();
        this.provider = provider;
        
        this.setElement(provider.valueElem);

        this.element.classList.add("field-designer");

        this.onRender(this.element);
    }
    
    protected abstract onRender(elem: HTMLElement);

    setValid(val: boolean) {
        val ? this.element.classList.remove("invalid") : this.element.classList.add("invalid");
    }
    
    destroy() {
        this.element.classList.remove("field-designer");
        super.destroy();
    }
}
