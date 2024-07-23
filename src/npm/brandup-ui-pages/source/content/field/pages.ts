import "./pages.less";
import { FormField } from "./base";
import { PageValue } from "./value/page";

export default class PagesContent extends FormField<PagesFieldFormOptions> {
    get typeName(): string { return "BrandUpPages.Form.Field.Pages"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element?.classList.add("pages");
    }

    protected _renderValueElem() {
        return new PageValue(this.options);
    }
}

export interface PagesFieldFormOptions {
    placeholder: string;
    pageType: string;
}