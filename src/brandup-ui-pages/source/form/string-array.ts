import { Field } from "./field";
import { DOM } from "brandup-ui-dom";
import iconDelete from "../svg/toolbar-button-discard.svg";
import "./string-array.less";

export class StringArrayField extends Field<Array<string>, StringArrayFieldOptions> {
    private __itemsElem: HTMLElement;
    private __isChanged: boolean = false;
    private __items: Array<string> = [];

    get typeName(): string { return "BrandUpPages.Form.Field.StringArray"; }

    constructor(name: string, errors: string[], options: StringArrayFieldOptions) {
        super(name, errors, options);

        this.__itemsElem = DOM.tag("div", { class: "items" }) as HTMLInputElement;
    }

    protected _onRender() {
        super._onRender();

        this.element?.classList.add("string-array");
        this.element?.appendChild(this.__itemsElem);

        this.__renderItems();
        this.__refreshUI();

        this.registerCommand("item-delete", (elem: HTMLElement) => {
            const itemElem = elem.closest(".item");
            if (!itemElem || !itemElem.hasAttribute("data-index")) return;

            const index = parseInt(itemElem.getAttribute("data-index")!);

            itemElem.remove();
            this.__items.splice(index, 1);

            this.__refreshIndexes();
        });

        this.element?.addEventListener("keyup", (e: Event) => {
            const t = e.target as HTMLInputElement;
            const elem = t.closest(".item");
            if (!elem || !elem.hasAttribute("data-index")) return;

            let index = parseInt(elem?.getAttribute("data-index")!);
            if (!elem || !index) return;

            const value = this.normalizeValue(t.value);
            if (value) {
                elem.classList.add("has-value");

                if (this.__items.length - 1 < index)
                    this.__items.push(value);
                else
                    this.__items[index] = value;
            }
            else
                elem.classList.remove("has-value");

            if (index === this.__itemsElem.childElementCount - 1)
                this.__itemsElem.appendChild(this.__createItemElem("", index + 1));
        });

        this.element?.addEventListener("change", (e: Event) => {
            const t = e.target as HTMLInputElement;
            const elem = t.closest(".item");
            if (!elem || !elem.hasAttribute("data-index")) return;

            const index = parseInt(elem.getAttribute("data-index")!);
            const value = this.normalizeValue(t.value);

            if (!value) {
                elem.remove();
                this.__items.splice(index, 1);

                this.__refreshIndexes();
            }
        });

        //this.__itemsElem.addEventListener("mousedown", (e: MouseEvent) => {
        //    let target = <Element>e.target;
        //    if (target.tagName === "INPUT") {
        //        e.stopImmediatePropagation();
        //        e.stopPropagation();
        //        console.log("input");
        //        return false;
        //    }
        //});
        this.__itemsElem.addEventListener("dragstart", (e: DragEvent) => {
            const target = e.target as Element;
            const itemElem = target.closest("[data-index]");
            if (!e.dataTransfer) return false;

            if (itemElem && itemElem.classList.contains("has-value") && itemElem.hasAttribute('data-index')) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("data-index", itemElem.getAttribute('data-index')!);
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
            const sourceIndex = parseInt(e.dataTransfer?.getData("data-index") || "-1");
            const elem = target.closest("[data-index]");
            if (elem && elem.hasAttribute("data-index") && sourceIndex >= 0) {
                const destIndex = parseInt(elem.getAttribute("data-index")!);
                if (destIndex !== sourceIndex) {
                    console.log(`Source: ${sourceIndex}; Dest: ${destIndex}`);

                    const sourceElem = DOM.queryElement(this.__itemsElem, `[data-index="${sourceIndex}"]`);
                    if (sourceElem) {
                        if (destIndex < sourceIndex) {
                            elem.insertAdjacentElement("beforebegin", sourceElem);
                        }
                        else {
                            elem.insertAdjacentElement("afterend", sourceElem);
                        }

                        const removed = this.__items.splice(sourceIndex, 1);
                        this.__items.splice(destIndex, 0, removed[0]);

                        this.__refreshIndexes();

                        console.log(this.__items);
                    }
                }
            }

            e.stopPropagation();
            return false;
        });
    }

    private __renderItems() {
        DOM.empty(this.__itemsElem);

        let i = 0;
        for (i = 0; i < this.__items.length; i++) {
            const item = this.__items[i];
            this.__createItemElem(item, i);
        }

        this.__createItemElem("", i + 1);
    }
    private __createItemElem(item: string, index: number) {
        const itemElem = DOM.tag("div", { class: "item", "data-index": index.toString() }, [
            DOM.tag("div", { class: "index", draggable: "true", title: "Нажмите, чтобы перетащить" }, `#${index + 1}`),
            DOM.tag("input", { type: "text", value: item, placeholder: this.options.placeholder ? this.options.placeholder : "" }),
            DOM.tag("ul", null, [
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-delete", title: "Удалить" }, iconDelete))
            ])
        ]);

        if (item)
            itemElem.classList.add("has-value");

        this.__itemsElem.appendChild(itemElem);

        return itemElem;
    }

    protected _onChanged() {
        this.__refreshUI();

        this.raiseChanged();
    }
    private __refreshUI() {
        const hasVal = this.hasValue();
        if (hasVal)
            this.element?.classList.add("has-value");
        else
            this.element?.classList.remove("has-value");
    }
    private __refreshIndexes() {
        for (let i = 0; i < this.__itemsElem.childElementCount; i++) {
            const elem = this.__itemsElem.children.item(i)!;

            elem.setAttribute("data-index", i.toString());
            const indexElem = DOM.getElementByClass(elem, "index");
            if (indexElem)
                indexElem.innerText = `#${i + 1}`;
        }
    }

    getValue(): Array<string> {
        if (this.hasValue())
            return this.__items;
        return [];
    }
    setValue(value: Array<string>) {
        const temp = new Array<string>();

        if (value) {
            for (let i = 0; i < value.length; i++) {
                const item = value[i];
                if (!item)
                    continue;

                temp.push(item);
            }
        }

        this.__items = temp;

        this.__renderItems();
        this.__refreshIndexes();
    }
    hasValue(): boolean {
        return this.__items !== null && this.__items.length !== 0;
    }

    normalizeValue(value: string) {
        if (!value)
            return "";

        return value.trim();
    }
}
export interface StringArrayFieldOptions {
    placeholder?: string;
    maxItems?: number;
}