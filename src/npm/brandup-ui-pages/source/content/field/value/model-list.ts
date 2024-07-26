import { UIElement } from "@brandup/ui";
import { DOM } from "@brandup/ui-dom";
import { ContentInfoModel, ModelFieldOptions, ModelFieldValue } from "../../../content/provider/model";
import iconEdit from "../../../svg/toolbar-button-edit.svg";
import iconDelete from "../../../svg/toolbar-button-discard.svg";
import { IFieldValueElement } from "../../../typings/content";

export class ModelListValue extends UIElement implements IFieldValueElement {
    private __value?: ModelFieldValue;
    private __onChange: (sourceIndex: number, destIndex: number) => void = () => {};

    get typeName(): string { return "BrandUpPages.Form.Field.Value.ModelList"; }
    readonly options: ModelFieldOptions;

    constructor(options: ModelFieldOptions) {
        super();

        this.options = options
        
        const valueElem = DOM.tag("div", { class: "items" });

        this.setElement(valueElem);
        this.__initLogic();
    }

    private __initLogic() {
        this.element?.addEventListener("dragstart", (e: DragEvent) => {
            const target = e.target as Element;
            const itemElem = target.closest("[data-content-path-index]");
            if (itemElem && e.dataTransfer) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("data-content-path-index", itemElem.getAttribute('data-content-path-index')!);
                e.dataTransfer.setDragImage(itemElem, 0, 0);
                return true;
            }
            e.stopPropagation();
            e.preventDefault();
            return false;
        }, false);
        this.element?.addEventListener("dragenter", (e: DragEvent) => {
            e.preventDefault();
            return true;
        });
        this.element?.addEventListener("dragover", (e: DragEvent) => {
            e.preventDefault();
        });
        this.element?.addEventListener("drop", (e: DragEvent) => {
            const target = e.target as Element;
            const sourceIndex = e.dataTransfer?.getData("data-content-path-index") || -1;
            const elem = target.closest("[data-content-path-index]");
            if (elem) {
                const destIndex = elem.getAttribute("data-content-path-index")!;
                if (destIndex !== sourceIndex) {
                    console.log(`Source: ${sourceIndex}; Dest: ${destIndex}`);

                    const sourceElem = this.element ? DOM.queryElement(this.element, `[data-content-path-index="${sourceIndex}"]`) : null;
                    if (sourceElem) {
                        if (destIndex < sourceIndex) {
                            elem.insertAdjacentElement("beforebegin", sourceElem);
                        }
                        else {
                            elem.insertAdjacentElement("afterend", sourceElem);
                        }
                        if (this.__onChange)
                            this.__onChange(+sourceIndex, +destIndex)
                        this.refreshBlockIndexes();
                    }
                }
            }

            e.stopPropagation();
            return false;
        });
    }

    renderItems(items: Array<ContentInfoModel>) {
        if (!this.element) return;

        DOM.empty(this.element);

        let i = 0
        for (i = 0; i < items.length; i++) {
            const item = items[i];
            this.element?.appendChild(this.__createItemElem(item, i));
        }

        this.element?.appendChild(DOM.tag("div", { class: "item new" }, [
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
        if (!this.element) return;

        for (let i = 0; i < this.element.children.length; i++) {
            const itemElem = this.element.children.item(i)!;
            if (!itemElem.hasAttribute("data-content-path-index"))
                continue;
            f(itemElem, i);
        }
    }
    refreshBlockIndexes() {
        this.eachItems((elem, index) => {
            elem.setAttribute("data-content-path-index", index.toString());
            const indexElem = DOM.getByClass(elem, "index");
            if (indexElem)
                indexElem.innerText = `#${index + 1}`;
        });
    }

    onChange(handler: (sourceIndex: number, destIndex: number) => void) {
        this.__onChange = handler;
    }

    deleteItem(index: number) {
        if (!this.element) return;

        const itemElem = DOM.queryElement(this.element, `[data-content-path-index="${index}"]`);
        itemElem?.remove();
        this.refreshBlockIndexes();
    }

    getValue(): ModelFieldValue | undefined {
        return this.__value;
    }
    setValue(value: ModelFieldValue) {
        this.__value = value;

        if (this.options.isListValue)
            this.renderItems(this.__value.items);
    }
    
    hasValue(): boolean {
        return Boolean(this.__value && this.__value.items && this.__value.items.length > 0);
    }
}