import { UIElement } from "brandup-ui";
import { Utility } from "brandup-ui-helpers";

export abstract class UIControl<TOptions = {}> extends UIElement {
    readonly options: TOptions = {} as TOptions;
    private __fragment?: DocumentFragment | null = null;

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
        this._applyOptions(options);

        this._onInitialize();
    }

    // Options
    protected _onApplyDefaultOptions() { return; }
    protected _applyOptions<TOptions>(options: TOptions) {
        if (options)
            Utility.extend(this.options, options);
    }

    // Render
    protected _getTagName(): string {
        return "div";
    }
    protected _getHtmlTemplate(): string {
        return "";
    }
    render(container: HTMLElement | string, position: InsertPosition = "afterbegin"): this {
        if (container) {
            if (!this.__fragment)
                throw new Error();

            if (Utility.isString(container)) {
                container = document.getElementById((container as string).substr(1)) || "";
                if (!container)
                    throw new Error();
            }
        }

        const htmlTemplate = this._getHtmlTemplate();
        if (htmlTemplate)
            this.element?.insertAdjacentHTML(position, htmlTemplate);

        if (this.__fragment) {
            (container as HTMLElement).appendChild(this.__fragment);
            delete this.__fragment;
        }

        this._onRender();

        return this;
    }
    destroy() {
        if (!this.isInject && this.element)
            this.element?.remove();

        super.destroy();
    }

    protected _onInitialize() { return; }
    protected abstract _onRender();
}