import { FieldDesigner } from "./base";
import ContentEditor from "brandup-pages-ckeditor";
import "./html.less";
import { AjaxResponse } from "@brandup/ui-ajax";

export class HtmlDesigner extends FieldDesigner<HtmlFieldFormOptions> {
    private __isChanged: boolean;
    private __editor: ContentEditor;

    get typeName(): string { return "BrandUpPages.HtmlDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("html-designer");
        if (this.options.placeholder)
            elem.setAttribute("data-placeholder", this.options.placeholder);

        ContentEditor.create(elem, { placeholder: this.options.placeholder })
            .then(editor => {
                this.__editor = editor;

                editor.model.document.on('change', () => {
                    if (editor.model.document.differ.hasDataChanges())
                        this.__isChanged = true;

                    this.__refreshUI();
                });

                this.__refreshUI();
            });

        this.element.addEventListener("focus", () => {
            this.__isChanged = false;
        });
        this.element.addEventListener("blur", () => {
            if (this.__isChanged) {
                this.__editor.model.document.differ.reset();
                this._onChanged();
            }

            this.__refreshUI();
        });
    }

    getValue(): string {
        const data = this.__editor.data.get();
        return data ? data : null;
    }
    setValue(value: string) {
        this.__editor.data.set(value ? value : "");

        this.__refreshUI();
    }
    hasValue(): boolean {
        const value = this.normalizeValue(this.element.innerText);
        if (!value)
            return false;

        const val = this.__editor.model.hasContent(this.__editor.model.document.getRoot(), { ignoreWhitespaces: true });
        return value && val ? true : false;
    }

    protected _onChanged() {
        this.__refreshUI();

        const value = this.getValue();

        this.page.queue.push({
            url: '/brandup.pages/content/html',
            query: {
                editId: this.page.editId,
                path: this.path,
                field: this.name
            },
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response: AjaxResponse<string>) => {
                if (response.status === 200) {
                    this.setValue(response.data);
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

    override destroy() {
        this.__editor.destroy().then(() => {
            super.destroy();
        });

        super.destroy();
    }
}

export interface HtmlFieldFormOptions {
    placeholder: string;
}