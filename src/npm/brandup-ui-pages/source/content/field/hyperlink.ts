import { FormField } from "./base";
import { HyperLinkType, HyperlinkValue } from "./value/hyperlink";

export default class HyperLinkContent extends FormField<HyperLinkFieldFormOptions> {
    get typeName(): string { return "BrandUpPages.Form.Field.HyperLink"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element?.classList.add("hyperlink");
    }

    renderValueElem() {
        return new HyperlinkValue(this.options);
    }
}
export interface HyperLinkFieldFormOptions {
    valueType: HyperLinkType;
    value: string;
}