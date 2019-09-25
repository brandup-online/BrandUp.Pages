import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import createEditor, { EditorInstance } from "brandup-pages-ckeditor";
import { DOM } from "brandup-ui";
import "./html.less";

export class HtmlContent extends Field<string, HtmlFieldFormOptions>  implements IContentField {
    readonly form: IContentForm;
    private __isChanged: boolean;
    private __value: HTMLElement;
    private __editor: EditorInstance;

    constructor(form: IContentForm, name: string, options: HtmlFieldFormOptions) {
        super(name, options);

        this.form = form;
    }

    get typeName(): string { return "BrandUpPages.Form.Field.Html"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("html");

        this.element.appendChild(this.__value = DOM.tag("div", { class: "value" }));

        if (this.options.placeholder)
            this.__value.setAttribute("data-placeholder", this.options.placeholder);

        createEditor(this.__value, {
            toolbar: ['heading', '|', 'bold', 'italic', 'strikethrough', 'link', 'bulletedList', 'numberedList'],
            placeholder: this.options.placeholder
        }).then(editor => {
            this.__editor = editor;

            editor.model.document.on('change', () => {
                if (editor.model.document.differ.hasDataChanges())
                    this.__isChanged = true;

                this.__refreshUI();
            });

            this.__refreshUI();
        });

        this.__value.addEventListener("focus", () => {
            this.__isChanged = false;
        });
        this.__value.addEventListener("blur", () => {
            if (this.__isChanged) {
                this.__editor.model.document.differ.reset();
                this._onChanged();
            }

            this.__refreshUI();
        });
    }

    getValue(): string {
        var data = this.__editor.data.get();
        return data ? data : null;
    }
    setValue(value: string) {
        if (this.__editor) {
            this.__editor.data.set(value ? value : "");
            this.__refreshUI();
        }
        else
            this.__value.innerHTML = value ? value : "";
    }
    hasValue(): boolean {
        var value = this.normalizeValue(this.__value.innerText);
        if (!value)
            return false;

        var val = this.__editor.model.hasContent(this.__editor.model.document.getRoot(), { ignoreWhitespaces: true });
        return value && val ? true : false;
    }

    protected _onChanged() {
        this.__refreshUI();

        var value = this.getValue();

        this.form.request(this, {
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (data: string, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
                else {
                    this.setErrors(["error"]);
                }
            }
        });
    }
    private __refreshUI() {
        if (this.hasValue())
            this.element.classList.remove("empty-value");
        else
            this.element.classList.add("empty-value");
    }

    normalizeValue(value: string): string {
        if (!value)
            return "";

        value = value.trim();

        return value;
    }

    destroy() {
        this.__editor.destroy().then(() => {
            super.destroy();
        });

        super.destroy();
    }
}

export interface HtmlFieldFormOptions {
    placeholder: string;
}