import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { FormField } from "./base";
import { ModelFieldOptions, ModelFieldProvider } from "../../content/provider/model";
import "./model.less";
import { ModelListValue } from "./value/model-list";
import { CommandContext } from "@brandup/ui";

export default class ModelField extends FormField<ModelFieldOptions> {
    declare readonly provider: ModelFieldProvider;
    declare protected __valueElem: ModelListValue;

    get typeName(): string { return "BrandUpPages.Form.Field.Content"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element?.classList.add("content");
        this.__valueElem.onChange((sourceIndex: number, destIndex: number) => this.provider.moveItem(sourceIndex, destIndex));
    }

    protected _renderValueElem() {
        if (this.options.isListValue) {
            return this.__renderListValue();
        }
        throw "ModelValue class not implemented";
    }

    private __renderListValue(): ModelListValue {
        const valueElem = new ModelListValue(this.options);

        this.registerCommand("item-settings", (context: CommandContext) => {
            const itemElem = this.__getItemElem(context.target);

            const itemIndex = itemElem.getAttribute("data-content-path-index")!;
            const content = this.provider.getItem(+itemIndex);
            this.provider.settingItem(content.path);
        });
        this.registerCommand("item-delete", (context: CommandContext) => {
            const itemElem = this.__getItemElem(context.target);
            const itemIndex = parseInt(itemElem.getAttribute("data-content-path-index")!);
            const path = itemElem.getAttribute("data-content-path");
            if (path === null) throw "the element does not have an attribute data-content-path";

            this.provider.deleteItem(itemIndex, path);
            valueElem.deleteItem(itemIndex);
        });

        this.registerCommand("item-add", () => {
            if (this.options.itemTypes.length === 1) {
                this.__addItem(this.options.itemTypes[0].name);
            }
            else {
                selectContentType(this.options.itemTypes).then((type) => {
                    this.__addItem(type.name);
                });
            }
        });
        return valueElem;
    }

    private __getItemElem(elem: HTMLElement) {
        const itemElem = elem.closest("[data-content-path-index]");
        if (!itemElem) throw `item element with attribute data-content-path-index not found`;
        return itemElem;
    }

    private __addItem(itemType: string) {
        this.provider.addItem(0);
    }
}