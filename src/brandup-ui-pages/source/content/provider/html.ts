import { FieldProvider } from "./base";
import { HtmlDesigner } from "../designer/html";

export class HtmlFieldProvider extends FieldProvider {
    createDesigner() {
        return new HtmlDesigner(this.__editor, this.__valueElem, this.model.options);
    }
}