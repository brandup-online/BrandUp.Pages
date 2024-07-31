import "./image.less";
import { FormField } from "./base";
import { ImageFieldProvider } from "../../content/provider/image";
import { ImageValue } from "./value/image";

export default class ImageContent extends FormField<ImageFieldOptions> {
    declare readonly provider: ImageFieldProvider;

    get typeName(): string { return "BrandUpPages.Form.Field.Image"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element?.classList.add("image");
    }

    renderValueElem() {
        return new ImageValue(this.options);
    }
}

export interface ImageFieldOptions {
}