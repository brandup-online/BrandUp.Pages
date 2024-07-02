import { HyperLinkFieldFormValue, HyperLinkFieldFormOptions } from "../../content/field/hyperlink";
import { FieldProvider } from "./base";

export class HyperlinkFieldProvider extends FieldProvider<HyperLinkFieldFormValue, HyperLinkFieldFormOptions> {
    createDesigner() {
        return null;
    }
}