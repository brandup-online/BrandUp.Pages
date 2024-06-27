import { PageBlocksDesigner } from "../designer/page-blocks";
import { ModelFieldProvider } from "./model";

export class PageBlocksFieldProvider extends ModelFieldProvider {
    createDesigner() {
        console.log("MODEL", this.model)
        return new PageBlocksDesigner(this.__editor, this.__valueElem, this.model.options);
    }
}