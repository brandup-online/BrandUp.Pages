import { TextDesigner } from "../designer/text";
import { FieldProvider } from "./base";

export class TextFieldProvider extends FieldProvider<any> {
    createDesigner() {
        return new TextDesigner(this.__editor, this.__valueElem, this.model.options || {});
    }
}