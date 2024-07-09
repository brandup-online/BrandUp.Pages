import { DOM } from "brandup-ui-dom";
import iconEdit from "../../svg/toolbar-button-edit.svg";
import iconDelete from "../../svg/toolbar-button-discard.svg";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { FormField } from "./base";
import { ContentInfoModel, ModelFieldOptions, ModelFieldProvider, ModelFieldValue } from "../../content/provider/model";
import "./model.less";

export class ModelField extends FormField<ModelFieldOptions> {
    private __value: ModelFieldValue;

    get typeName(): string { return "BrandUpPages.Form.Field.Content"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("content");
    }

    protected _renderValueElem() {
        if (this.options.isListValue) {
            return this.__renderListValue();
        }
    }

    private __renderListValue() {
        const valueElem = DOM.tag("div", { class: "items" });

        this.registerCommand("item-settings", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path-index]");
            const itemIndex = itemElem.getAttribute("data-content-path-index");
            let contentPath = this.provider.content.path;
            let modelPath = (contentPath ? contentPath + "." : "") + `${this.provider.name}[${itemIndex}]`;

            // this.provider.settingItem(modelPath);
        });
        this.registerCommand("item-delete", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path-index]");
            const itemIndex = parseInt(itemElem.getAttribute("data-content-path-index"));
            const path = itemElem.getAttribute("data-content-path")
            // this.provider.deleteItem(itemIndex, path);
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

        valueElem.addEventListener("dragstart", (e: DragEvent) => {
            const target = e.target as Element;
            const itemElem = target.closest("[data-content-path-index]");
            if (itemElem) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("data-content-path-index", itemElem.getAttribute('data-content-path-index'));
                e.dataTransfer.setDragImage(itemElem, 0, 0);
                return true;
            }
            e.stopPropagation();
            e.preventDefault();
            return false;
        }, false);
        valueElem.addEventListener("dragenter", (e: DragEvent) => {
            e.preventDefault();
            return true;
        });
        valueElem.addEventListener("dragover", (e: DragEvent) => {
            e.preventDefault();
        });
        valueElem.addEventListener("drop", (e: DragEvent) => {
            const target = e.target as Element;
            const sourceIndex = e.dataTransfer.getData("data-content-path-index");
            const elem = target.closest("[data-content-path-index]");
            if (elem) {
                const destIndex = elem.getAttribute("data-content-path-index");
                if (destIndex !== sourceIndex) {
                    console.log(`Source: ${sourceIndex}; Dest: ${destIndex}`);

                    const sourceElem = DOM.queryElement(valueElem, `[data-content-path-index="${sourceIndex}"]`);
                    if (sourceElem) {
                        if (destIndex < sourceIndex) {
                            elem.insertAdjacentElement("beforebegin", sourceElem);
                        }
                        else {
                            elem.insertAdjacentElement("afterend", sourceElem);
                        }
                        // this.provider.moveItem(+sourceIndex, +destIndex)
                    }
                }
            }

            e.stopPropagation();
            return false;
        });
        return valueElem;
    }

    deleteItem(index: number) {
        const itemElem = DOM.queryElement(this.__valueElem, `[data-content-path-index="${index}"]`);
        itemElem.remove();
        this._refreshBlockIndexes();
    }

    getValue(): ModelFieldValue {
        return this.__value;
    }
    protected _setValue(value: ModelFieldValue) {
        this.__value = value;

        if (this.options.isListValue)
            this.__renderItems();
    }
    hasValue(): boolean {
        return this.__value && this.__value.items && this.__value.items.length > 0;
    }

    private __renderItems() {
        DOM.empty(this.__valueElem);

        let i = 0
        for (i = 0; i < this.__value.items.length; i++) {
            const item = this.__value.items[i];
            this.__valueElem.appendChild(this.__createItemElem(item, i));
        }

        this.__valueElem.appendChild(DOM.tag("div", { class: "item new" }, [
            DOM.tag("div", { class: "index" }, `#${i + 1}`),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-add" }, this.options.addText ? this.options.addText : "Добавить")
        ]));
    }
    private __createItemElem(item: ContentInfoModel, index: number) {
        const itemElem = DOM.tag("div", { class: "item", "data-content-path-index": index.toString() }, [
            DOM.tag("div", { class: "index", draggable: "true", title: "Нажмите, чтобы перетащить" }, `#${index + 1}`),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-settings" }, item.title),
            DOM.tag("div", { class: "type" }, item.type.title),
            DOM.tag("ul", null, [
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-settings", title: "Редактировать" }, iconEdit)),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-delete", title: "Удалить" }, iconDelete))
            ])
        ]);

        return itemElem;
    }
    private eachItems(f: (elem: Element, index: number) => void) {
        for (let i = 0; i < this.__valueElem.children.length; i++) {
            const itemElem = this.__valueElem.children.item(i);
            if (!itemElem.hasAttribute("data-content-path-index"))
                continue;
            f(itemElem, i);
        }
    }
    private _refreshBlockIndexes() {
        this.eachItems((elem, index) => {
            elem.setAttribute("data-content-path-index", index.toString());
            DOM.getElementByClass(elem, "index").innerText = `#${index + 1}`;
        });
    }
    private __addItem(itemType: string) {
        // this.provider.addItem( 0);
    }
}