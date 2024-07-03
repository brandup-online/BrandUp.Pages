import { IContentField, IContentForm } from "../../typings/content";
import { DOM } from "brandup-ui-dom";
import { ContentModel, ContentTypeModel } from "../../typings/models";
import iconEdit from "../../svg/toolbar-button-edit.svg";
import iconDelete from "../../svg/toolbar-button-discard.svg";
import { selectContentType } from "../../dialogs/dialog-select-content-type";
import { FormField } from "./base";
import { ModelFieldProvider } from "../../content/provider/model";
import "./model.less";

export class ModelField extends FormField<ModelFieldFormValue, ModelDesignerOptions, ModelFieldProvider> implements IContentField {
    private __value: ModelFieldFormValue;
    private __itemsElem: HTMLElement;

    get typeName(): string { return "BrandUpPages.Form.Field.Content"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("content");

        this.__itemsElem = DOM.tag("div", { class: "items" });
        this.element.appendChild(this.__itemsElem);

        this.registerCommand("item-settings", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path-index]");
            const itemIndex = itemElem.getAttribute("data-content-path-index");
            let contentPath = this.provider.content.model.path;
            let modelPath = (contentPath ? contentPath + "." : "") + `${this.name}[${itemIndex}]`;

            this.provider.settingItem(modelPath);
        });
        this.registerCommand("item-delete", (elem: HTMLElement) => {
            const itemElem = elem.closest("[data-content-path-index]");
            const itemIndex = parseInt(itemElem.getAttribute("data-content-path-index"));
            const path = itemElem.getAttribute("data-content-path")
            this.provider.deleteItem(itemIndex, path);
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

        this.__itemsElem.addEventListener("dragstart", (e: DragEvent) => {
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
        this.__itemsElem.addEventListener("dragenter", (e: DragEvent) => {
            e.preventDefault();
            return true;
        });
        this.__itemsElem.addEventListener("dragover", (e: DragEvent) => {
            e.preventDefault();
        });
        this.__itemsElem.addEventListener("drop", (e: DragEvent) => {
            const target = e.target as Element;
            const sourceIndex = e.dataTransfer.getData("data-content-path-index");
            const elem = target.closest("[data-content-path-index]");
            if (elem) {
                const destIndex = elem.getAttribute("data-content-path-index");
                if (destIndex !== sourceIndex) {
                    console.log(`Source: ${sourceIndex}; Dest: ${destIndex}`);

                    const sourceElem = DOM.queryElement(this.__itemsElem, `[data-content-path-index="${sourceIndex}"]`);
                    if (sourceElem) {
                        if (destIndex < sourceIndex) {
                            elem.insertAdjacentElement("beforebegin", sourceElem);
                        }
                        else {
                            elem.insertAdjacentElement("afterend", sourceElem);
                        }
                        this.provider.moveItem(+sourceIndex, +destIndex)
                    }
                }
            }

            e.stopPropagation();
            return false;
        });
    }

    deleteItem(index: number) {
        const itemElem = DOM.queryElement(this.__itemsElem, `[data-content-path-index="${index}"]`);
        itemElem.remove();
        this._refreshBlockIndexes();
    }

    getValue(): ModelFieldFormValue {
        return this.__value;
    }
    setValue(value: ModelFieldFormValue) {
        this.__value = value;

        this.__renderItems();
    }
    hasValue(): boolean {
        return this.__value && this.__value.items && this.__value.items.length > 0;
    }

    private __renderItems() {
        DOM.empty(this.__itemsElem);

        let i = 0
        for (i = 0; i < this.__value.items.length; i++) {
            const item = this.__value.items[i];
            this.__itemsElem.appendChild(this.__createItemElem(item, i));
        }

        this.__itemsElem.appendChild(DOM.tag("div", { class: "item new" }, [
            DOM.tag("div", { class: "index" }, `#${i + 1}`),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-add" }, this.options.addText ? this.options.addText : "Добавить")
        ]));
    }
    private __createItemElem(item: ContentModel, index: number) {
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
        for (let i = 0; i < this.__itemsElem.children.length; i++) {
            const itemElem = this.__itemsElem.children.item(i);
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
        this.provider.addItem(itemType, 0);
    }
}

export interface ModelDesignerOptions {
    addText: string;
    isListValue: boolean;
    itemType: ContentTypeModel;
    itemTypes: Array<ContentTypeModel>;
}

export interface ModelFieldFormValue {
    items: Array<ContentModel>;
}