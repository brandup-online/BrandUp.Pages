import "./hyperlink.less";
import { FormField } from "./base";
import { HyperLinkType, HyperlinkValue } from "./value/hyperlink";

export class HyperLinkContent extends FormField<HyperLinkFieldFormOptions> {
    get typeName(): string { return "BrandUpPages.Form.Field.HyperLink"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("hyperlink");
    }

    protected _renderValueElem() {
        return new HyperlinkValue(this.options);
    }
}
export interface HyperLinkFieldFormOptions {
    valueType: HyperLinkType;
    value: string;
}