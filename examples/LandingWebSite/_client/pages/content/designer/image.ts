import { FieldDesigner } from "./base";

export class ImageDesigner extends FieldDesigner<ImageDesignerOptions> {
    get typeName(): string { return "BrandUpPages.ImageDesigner"; }
    protected onRender(elem: HTMLElement) {
    }
    hasValue(): boolean {
        return false;
    }
}

export interface ImageDesignerOptions {
}