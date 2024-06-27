import { ModelDesigner } from "../designer/model";
import { FieldProvider } from "./base";

export class ModelFieldProvider extends FieldProvider<any> {
    createDesigner() {
        return new ModelDesigner(this.__editor, this.__valueElem, this.model.options);
    }
}