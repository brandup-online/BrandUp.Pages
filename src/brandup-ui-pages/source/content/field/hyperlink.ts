import "./hyperlink.less";
import { FormField } from "./base";
import { HyperLinkType, HyperlinkValue } from "./value/hyperlink";
import { HyperlinkFieldProvider, HyperLinkValue } from "../../content/provider/hyperlink";

export class HyperLinkContent extends FormField<HyperLinkFieldFormOptions> {
    get typeName(): string { return "BrandUpPages.Form.Field.HyperLink"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("hyperlink");
        this.__valueElem.onChange((value: HyperLinkValue) => {
            if (value.valueType === "Page")
                (this.provider as HyperlinkFieldProvider).selectPage(value.value);
            else if (value.valueType === "Url")
                (this.provider as HyperlinkFieldProvider).changeValue(value.value);
        });
    }

    protected _renderValueElem() {
        return new HyperlinkValue(this.options);
    }
}
export interface HyperLinkFieldFormOptions {
    valueType: HyperLinkType;
    value: string;
}