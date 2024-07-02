import { UIElement } from "brandup-ui";
import { IContentFieldDesigner, IContentFieldProvider, IPageDesigner } from "../../typings/content";
import "./base.less";

export abstract class FieldDesigner<TOptions, TProvider extends IContentFieldProvider> extends UIElement implements IContentFieldDesigner {
    readonly options: TOptions;
    readonly provider: TProvider;
    readonly path: string;
    readonly name: string;
    readonly fullPath: string;

    constructor(elem: HTMLElement, options: TOptions, provider: TProvider) {
        super();
        this.options = options;
        this.provider = provider;
        this.path = elem.getAttribute("data-content-field-path");
        this.name = this.fullPath = elem.getAttribute("data-content-field-name");

        if (this.path)
            this.fullPath = this.path + "." + this.fullPath;

        this.setElement(elem);

        elem.classList.add("field-designer");

        this.onRender(elem);
    }

    abstract getValue();

    abstract setValue(value);

    protected _onChanged() {
        this.provider.setValue(this.getValue());
    }

    protected abstract onRender(elem: HTMLElement);

    setValid(val: boolean) {
        val ? this.element.classList.remove("invalid") : this.element.classList.add("invalid");
    }

    abstract hasValue(): boolean;

    destroy() {
        this.element.classList.remove("field-designer");
        super.destroy();
    }
}
