import "./html.less";
import { FormField } from "./base";
import { HTMLValue } from "./value/html";

export default class HtmlContent extends FormField<HtmlFieldFormOptions> {
    get typeName(): string { return "BrandUpPages.Form.Field.Html"; }

    protected _renderValueElem() {
        return new HTMLValue(this.options);
    }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element?.classList.add("html");
    }
}

export interface HtmlFieldFormOptions {
    placeholder: string;
}