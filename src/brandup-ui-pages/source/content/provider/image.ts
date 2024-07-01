import { ImageDesigner } from "../designer/image";
import { FieldProvider } from "./base";

export class ImageFieldProvider extends FieldProvider {
    createDesigner() {
        return new ImageDesigner(this.__editor, this.__valueElem, this.model.options);
    }
}