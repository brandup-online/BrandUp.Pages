import { AjaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { ModelDesigner } from "../designer/model";
import { FieldProvider } from "./base";
import { IContentFieldDesigner } from "../../typings/content";

export class ModelFieldProvider extends FieldProvider<any> {
    protected __designerType = ModelDesigner;

    createDesigner(): IContentFieldDesigner {
        return new this.__designerType(this.__editor, this.__valueElem, this.model.options);
    }
}