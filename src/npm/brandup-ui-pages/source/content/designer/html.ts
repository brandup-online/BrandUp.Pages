import { FieldDesigner } from "./base";
import ContentEditor from "brandup-pages-ckeditor";
import "./html.less";
import { HtmlFieldProvider } from "../../content/provider/html";

export class HtmlDesigner extends FieldDesigner<HtmlFieldProvider> {
    private __isChanged: boolean = false;
    private __editor?: ContentEditor;
    private __editorPromise?: Promise<any>;

    get typeName(): string { return "BrandUpPages.HtmlDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("html-designer");
        if (this.provider.options.placeholder)
            elem.setAttribute("data-placeholder", this.provider.options.placeholder);

        this.__editorPromise = ContentEditor.create(elem, { placeholder: this.provider.options.placeholder });
        this.__editorPromise.then(editor => {
            this.__editor = editor;

            editor.model.document.on('change', () => {
                if (editor.model.document.differ.hasDataChanges())
                    this.__isChanged = true;
            });
        });

        this.element?.addEventListener("focus", () => {
            this.__isChanged = false;
        });
        this.element?.addEventListener("blur", () => {
            if (this.__isChanged) {
                this.__editor?.model.document.differ.reset();

                const data = this.__editor?.data.get();
                this.provider.saveValue(data ? data : "");
            }
        });
    }
    
    destroy() {
        this.element?.classList.remove("html-designer");
        this.element?.removeAttribute("data-placeholder");

        if (this.__editor)
            this.__editor.destroy().then(() => super.destroy());
        else this.__editorPromise?.then(editor => {
            editor.destroy().then(() => super.destroy());
        })
    }
}