import { TextboxOptions } from "../../form/textbox";
import { FormField } from "./base";
import { TextBoxValue } from "./value/textbox";

export default class TextContent extends FormField<TextboxOptions> {

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("text");
    }

    protected _renderValueElem() {
        return new TextBoxValue(this.options);
    }
}