import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import { DOM } from "brandup-ui";
import "./model.less";
import iconEdit from "../../svg/toolbar-button-edit.svg";
import iconDelete from "../../svg/toolbar-button-discard.svg";
import { editPage } from "../../dialogs/page-edit";
import { selectContentType } from "../../dialogs/dialog-select-content-type";

export class ModelField extends Field<ModelFieldFormValue, ModelDesignerOptions> implements IContentField {
    readonly form: IContentForm;
    private __value: ModelFieldFormValue;
    private __itemsElem: HTMLElement;

    constructor(form: IContentForm, name: string, options: ModelDesignerOptions) {
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
            let contentPath = `${this.name}[${itemIndex}]`;

            if (this.form.contentPath)
                contentPath = this.form.contentPath + "." + contentPath;

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
                url: '/brandup.pages/content/model',
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
            else {
                selectContentType(this.options.itemTypes).then((type) => {
                    this.__addItem(type.name);
                });
            }
        });

        this.__itemsElem.addEventListener("dragstart", (e: DragEvent) => {
            let target = <Element>e.target;
            if (target.hasAttribute("content-path-index")) {
                e.dataTransfer.effectAllowed = 'move';
                e.dataTransfer.setData("content-path-index", target.getAttribute('content-path-index'));
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
            let target = <Element>e.target;
            var sourceIndex = e.dataTransfer.getData("content-path-index");
            let elem = target.closest("[content-path-index]");
            if (elem) {
                let destIndex = elem.getAttribute("content-path-index");
                if (destIndex !== sourceIndex) {
                    console.log(`Source: ${sourceIndex}; Dest: ${destIndex}`);

                    let sourceElem = DOM.queryElement(this.__itemsElem, `[content-path-index="${sourceIndex}"]`);
                    if (sourceElem) {
                        elem.insertAdjacentElement("afterend", sourceElem);

                        this.form.queue.request({
                            url: '/brandup.pages/content/model/move',
                            urlParams: {
                                editId: this.form.editId,
                                path: this.form.contentPath,
                                field: this.name,
                                itemIndex: sourceIndex,
                                newIndex: destIndex
                            },
                            method: "POST",
                            success: (data: ModelFieldFormValue, status: number) => {
                                if (status === 200) {
                                    this.setValue(data);
                                }
                            }
                        });
                    }
                }
            }

            e.stopPropagation();
            return false;
        });
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
            let item = this.__value.items[i];
            this.__itemsElem.appendChild(this.__createItemElem(item, i));
        }

        this.__itemsElem.appendChild(DOM.tag("div", { class: "item new" }, [
            DOM.tag("div", { class: "index" }, `#${i + 1}`),
            DOM.tag("a", { href: "", class: "title", "data-command": "item-add" }, this.options.addText ? this.options.addText : "Добавить")
        ]));
    }
    private __createItemElem(item: ContentModel, index: number) {
        let itemElem = DOM.tag("div", { class: "item", "content-path-index": index.toString(), draggable: "true" }, [
            DOM.tag("div", { class: "index", title: "Нажмите, чтобы перетащить" }, `#${index + 1}`),
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
            url: '/brandup.pages/content/model/data',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name,
                itemIndex: itemIndex.toString()
            },
            method: "GET",
            success: (data: ContentModel, status: number) => {
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
            url: '/brandup.pages/content/model',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name,
                itemType: itemType
            },
            method: "PUT",
            success: (data: ModelFieldFormValue, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
            }
        });
    }
    private __refreshItems() {
        this.form.queue.request({
            url: '/brandup.pages/content/model',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name
            },
            method: "GET",
            success: (data: ModelFieldFormValue, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
            }
        });
    }
}

export interface ModelDesignerOptions {
    addText: string;
    isListValue: boolean;
    itemTypes: Array<ContentTypeModel>;
}

export interface ModelFieldFormValue {
    items: Array<ContentModel>;
}