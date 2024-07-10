import "./hyperlink.less";
import { FormField } from "./base";
import { HyperlinkValue } from "./value/hyperlink";
import { HyperlinkFieldProvider } from "../../content/provider/hyperlink";

export class HyperLinkContent extends FormField<HyperLinkFieldFormOptions> {
    get typeName(): string { return "BrandUpPages.Form.Field.HyperLink"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("hyperlink");
        this.__valueElem.onChange((value: string) => (this.provider as HyperlinkFieldProvider).selectPage(value));
    }

    protected _renderValueElem() {
        return new HyperlinkValue(this.options);
    }
}

export type HyperLinkType = "Url" | "Page";

export interface HyperLinkFieldFormValue {
    valueType: HyperLinkType;
    value: string;
    pageTitle?: string;
}

export interface HyperLinkFieldFormOptions {
    valueType: "Url" | "Page";
    value: string;
}