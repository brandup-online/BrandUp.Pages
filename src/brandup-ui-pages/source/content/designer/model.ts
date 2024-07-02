import { DesignerEvent, FieldDesigner } from "./base";
import { ContentTypeModel } from "../../typings/models";
import "./model.less";

export class ModelDesigner extends FieldDesigner<ModelDesignerOptions> {
    get typeName(): string { return "BrandUpPages.ModelDesigner"; }

    protected onRender(elem: HTMLElement) {
        
    }

    getValue() {
        throw "method getValue not implemented";
    };

    setValue(value) {
        throw "method getValue not implemented";
    };

    hasValue() {
        return false;
    }
}
export interface ModelDesignerOptions {
    addText: string;
    isListValue: boolean;
    itemType: ContentTypeModel;
    itemTypes: Array<ContentTypeModel>;
}