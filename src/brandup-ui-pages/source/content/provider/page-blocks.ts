import { PageBlocksDesigner } from "../designer/page-blocks";
import { ModelFieldProvider } from "./model";

export class PageBlocksFieldProvider extends ModelFieldProvider {
    protected __designerType = PageBlocksDesigner;
}