import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import { DOM } from "brandup-ui";
import "./content.less";
import iconEdit from "../../svg/toolbar-button-edit.svg";
import iconDelete from "../../svg/toolbar-button-discard.svg";
import { editPage } from "../../dialogs/page-edit";

export class ContentField extends Field<ContentFieldFormValue, ContentDesignerOptions> implements IContentField {
    readonly form: IContentForm;
    private __value: ContentFieldFormValue;
    private __itemsElem: HTMLElement;

    constructor(form: IContentForm, name: string, options: ContentDesignerOptions) {
        super(name, options);

        this.form = form;
    }

    get typeName(): string { return "BrandUpPages.Form.Field.Content"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("content");

        this.__itemsElem = DOM.tag("div", { class: "items" });
        this.element.appendChild(this.__itemsElem);

        this.registerCommand("item-settings", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = itemElem.getAttribute("content-path-index");
            let contentPath = `${this.form.contentPath}.${this.name}[${itemIndex}]`;

            editPage(this.form.editId, contentPath).then(() => {
                this.__refreshItem(itemElem);
            });
        });

        this.registerCommand("item-delete", (elem: HTMLElement) => {
            let itemElem = elem.closest("[content-path-index]");
            let itemIndex = parseInt(itemElem.getAttribute("content-path-index"));

            itemElem.remove();
            this._refreshBlockIndexes();

            this.form.queue.request({
                url: '/brandup.pages/content/content',
                urlParams: {
                    editId: this.form.editId,
                    path: this.form.contentPath,
                    field: this.name,
                    itemIndex: itemIndex.toString()
                },
                method: "DELETE",
                success: (data: string, status: number) => {
                    if (status === 200) {
                    }
                }
            });
        });

        this.registerCommand("item-add", (elem: HTMLElement) => {
            if (this.options.itemTypes.length === 1) {
                this.__addItem(this.options.itemTypes[0].name);
            }
        });
    }

    getValue(): ContentFieldFormValue {
        return this.__value;
    }
    setValue(value: ContentFieldFormValue) {
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
            let item = this.__value.items[i];
            this.__itemsElem.appendChild(this.__createItemElem(item, i));
        }

        this.__itemsElem.appendChild(DOM.tag("div", { class: "item new" }, [
            DOM.tag("div", { class: "index" }, `#${i + 1}`),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-add" }, "Добавить баннер")
        ]));
    }
    private __createItemElem(item: ContentItem, index: number) {
        let itemElem = DOM.tag("div", { class: "item", "content-path-index": index.toString(), draggable: "true" }, [
            DOM.tag("div", { class: "index" }, `#${index + 1}`),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-settings" }, item.type.title),
            DOM.tag("ul", null, [
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-settings" }, iconEdit)),
                DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "item-delete" }, iconDelete))
            ])
        ]);

        itemElem.addEventListener("dragstart", (e: DragEvent) => {
            e.preventDefault();
        }, false);

        itemElem.addEventListener("dragover", (e: DragEvent) => {
            e.preventDefault();
        });

        return itemElem;
    }
    private eachItems(f: (elem: Element, index: number) => void) {
        for (let i = 0; i < this.__itemsElem.children.length; i++) {
            let itemElem = this.__itemsElem.children.item(i);
            if (!itemElem.hasAttribute("content-path-index"))
                continue;
            f(itemElem, i);
        }
    }
    private _refreshBlockIndexes() {
        this.eachItems((elem, index) => {
            elem.setAttribute("content-path-index", index.toString());
            DOM.getElementByClass(elem, "index").innerText = `#${index + 1}`;
        });
    }
    private __refreshItem(elem: Element) {
        let itemIndex = parseInt(elem.getAttribute("content-path-index"));

        this.form.queue.request({
            url: '/brandup.pages/content/content/data',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name,
                itemIndex: itemIndex.toString()
            },
            method: "GET",
            success: (data: ContentItem, status: number) => {
                if (status === 200) {
                    this.__value.items[itemIndex] = data;
                    
                    let newElem = this.__createItemElem(data, itemIndex);
                    elem.insertAdjacentElement("afterend", newElem);
                    elem.remove();
                }
            }
        });
    }
    private __addItem(itemType: string) {
        this.form.queue.request({
            url: '/brandup.pages/content/content',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name,
                itemType: itemType
            },
            method: "PUT",
            success: (data: ContentFieldFormValue, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
            }
        });
    }
    private __refreshItems() {
        this.form.queue.request({
            url: '/brandup.pages/content/content',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name
            },
            method: "GET",
            success: (data: ContentFieldFormValue, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
            }
        });
    }
}

export interface ContentDesignerOptions {
    isListValue: boolean;
    itemTypes: Array<ContentItemType>;
}

export interface ContentFieldFormValue {
    items: Array<ContentItem>;
}

export interface ContentItemType {
    name: string;
    title: string;
}

export interface ContentItem {
    type: ContentItemType;
}