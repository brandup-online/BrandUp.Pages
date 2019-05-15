import { FieldDesigner } from "./base";
import { TextboxOptions } from "../../form/textbox";
import createEditor, { EditorInstance } from "brandup-pages-ckeditor";
import "./html.less";

export class HtmlDesigner extends FieldDesigner<TextboxOptions> {
    private __isChanged: boolean;
    private __editor: EditorInstance;

    get typeName(): string { return "BrandUpPages.HtmlDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("html-designer");

        createEditor(elem, {
            toolbar: ['heading', '|', 'bold', 'italic', 'link', 'bulletedList', 'numberedList']
        }).then(editor => {
            this.__editor = editor;

            editor.model.document.on('change', () => {
                if (editor.model.document.differ.hasDataChanges()) {
                    this.__isChanged = true;
                }
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
        });
    }

    getValue(): string {
        var data = this.__editor.data.get();
        return data ? data : null;
    }
    setValue(value: string) {
        this.__editor.data.set(value ? value : "");

        this.__refreshUI();
    }
    hasValue(): boolean {
        var val = this.__editor.model.hasContent(this.__editor.model.document.getRoot(), { ignoreWhitespaces: true });
        return val ? true : false;
    }

    protected _onChanged() {
        this.__refreshUI();

        this.page.queue.request({
            url: '/brandup.pages/content/html',
            urlParams: {
                editId: this.page.editId,
                path: this.path,
                field: this.name
            },
            method: "POST",
            type: "JSON",
            data: this.getValue(),
            success: (data: string, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
            }
        });
    }
    private __refreshUI() {
        if (this.hasValue())
            this.element.classList.remove("has-value");
        else
            this.element.classList.add("empty-value");
    }

    destroy() {
        this.__editor.destroy().then(() => {
            super.destroy();
        });
    }
}