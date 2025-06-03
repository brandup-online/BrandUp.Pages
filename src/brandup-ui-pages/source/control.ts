import { UIElement } from "@brandup/ui";
import { TypeHelper } from "@brandup/ui-helpers";

export abstract class UIControl<TOptions = {}> extends UIElement {
    readonly options: TOptions = {} as TOptions;
    private __fragment: DocumentFragment;

    readonly isInject: boolean = false;

    constructor(options?: TOptions, element?: HTMLElement) {
        super();

        const tagName = this._getTagName();
        if (!tagName)
            throw new Error();

        if (!element) {
            this.__fragment = document.createDocumentFragment();
            element = document.createElement(tagName);
            this.__fragment.appendChild(element);
        }
        else
            this.isInject = true;

        this.setElement(element);

        this._onApplyDefaultOptions();

        if(options)
            this.options = { ...this.options, ...options };

        this._onInitialize();
    }

    // Options
    protected _onApplyDefaultOptions() { return; }

    // Render
    protected _getTagName(): string {
        return "div";
    }
    protected _getHtmlTemplate(): string {
        return null;
    }
    render(container: HTMLElement | string, position: InsertPosition = "afterbegin"): this {
        if (container) {
            if (!this.__fragment)
                throw new Error();

            if (TypeHelper.isString(container)) {
                container = document.getElementById((container as string).substr(1));
                if (!container)
                    throw new Error();
            }
        }

        const htmlTemplate = this._getHtmlTemplate();
        if (htmlTemplate)
            this.element.insertAdjacentHTML(position, htmlTemplate);

        if (this.__fragment) {
            (container as HTMLElement).appendChild(this.__fragment);
            delete this.__fragment;
        }

        this._onRender();

        return this;
    }
    destroy() {
        if (!this.isInject && this.element)
            this.element.remove();

        super.destroy();
    }

    protected _onInitialize() { return; }
    protected abstract _onRender();
}