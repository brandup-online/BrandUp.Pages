import { UIElement } from "@brandup/ui";
import { DOM } from "@brandup/ui-dom";
import { HtmlFieldFormOptions } from "../html";
import { IFieldValueElement } from "../../../typings/content";
import ContentEditor from "brandup-pages-ckeditor";

export class HTMLValue extends UIElement implements IFieldValueElement {
    private __isChanged: boolean = false;
    private __editor?: ContentEditor;
    private __editorPromise: Promise<any>;

    private __onChange: (value: string) => void = () => {};

    get typeName(): string { return "BrandUpPages.Form.Field.Value.HTML"; }

    constructor(options: HtmlFieldFormOptions) {
        super();
        
        const valueElem = DOM.tag("div", { class: "value html" });
        if (options.placeholder)
            valueElem.setAttribute("data-placeholder", options.placeholder);

        valueElem.addEventListener("focus", () => {
            this.__isChanged = false;
        });

        valueElem.addEventListener("blur", () => {
            if (this.__isChanged) {
                this.__editor?.model.document.differ.reset();
                const value = this.getValue();
                this.__onChange(value);
            }

            this.__refreshUI();
        });

        this.__editorPromise = ContentEditor.create(valueElem, { placeholder: options.placeholder })
            .then(editor => {
                this.__editor = editor;

                editor.model.document.on('change', () => {
                    if (editor.model.document.differ.hasDataChanges())
                        this.__isChanged = true;

                    this.__refreshUI();
                });

                this.__refreshUI();
            });

        this.setElement(valueElem);
    }

    private __refreshUI() {
        if (this.hasValue())
            this.element?.classList.remove("empty-value");
        else
            this.element?.classList.add("empty-value");
    }

    normalizeValue(value: string): string {
        if (!value)
            return "";

        value = value.trim();

        return value;
    }

    hasValue(): boolean {
        const value = this.normalizeValue(this.element?.innerText || "");
        if (!value)
            return false;
        const root = this.__editor?.model.document.getRoot();
        if (!root) return false;

        const val = this.__editor?.model.hasContent(root, { ignoreWhitespaces: true });
        return value && val ? true : false;
    }

    setValue(value: string) {
        // console.log("🚀 ~ HTMLValue ~ setValue ~ value:", value)
        // this.__editorPromise.then (()=> { // если editor еще не создался - ждем
        //     if (this.__editor) {
        //         this.__editor.data.set(value ? value : "");
        //         this.__refreshUI();
        //     }
        //     else if (this.element)
        //         this.element.innerHTML = value ? value : "";
        // });
    }

    getValue(): string {
        const data = this.__editor?.data.get();
        if (data === undefined) throw new Error("getValue error: data is undefined");
        return data;
    }

    onChange(handler: (value: string) => void) {
        this.__onChange = handler;
    }

    destroy() {
        this.__editor?.destroy().then(() => {
            super.destroy();
        });

        super.destroy();
    }
}