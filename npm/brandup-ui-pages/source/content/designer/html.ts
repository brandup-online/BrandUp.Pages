import { FieldDesigner } from "./base";
import ContentEditor from "brandup-pages-ckeditor";
import "./html.less";
import { AjaxResponse } from "@brandup/ui-ajax";
import { asEmitter } from "../../utils/ckeditor";

export class HtmlDesigner extends FieldDesigner<HtmlFieldFormOptions> {
    private __isChanged: boolean;
    private __editor: ContentEditor;

    get typeName(): string { return "BrandUpPages.HtmlDesigner"; }

    protected onRender(elem: HTMLElement) {
        elem.classList.add("html-designer");
        if (this.options.placeholder)
            elem.setAttribute("data-placeholder", this.options.placeholder);

        ContentEditor.create(elem, { placeholder: this.options.placeholder, language: "ru", blockToolbarEnabled: true })
            .then(editor => {
                this.__editor = editor;

                // change:data срабатывает на любое изменение данных, включая форматирование
                // (атрибуты вроде bold/italic), которое обычный change + hasDataChanges может пропустить.
                asEmitter(editor.model.document).on('change:data', () => {
                    this.__isChanged = true;

                    this.__refreshUI();
                });

                // Сохраняем по потере фокуса редактором через focusTracker, а не по нативному blur
                // редактируемой области: focusTracker считает редактор в фокусе, пока фокус внутри
                // его UI (включая тулбары). Иначе клик по кнопке форматирования вызывает blur области
                // ДО применения формата, и изменение форматирования не сохраняется.
                asEmitter(editor.ui.focusTracker).on<[string, boolean]>('change:isFocused', (_evt, _name, isFocused) => {
                    if (isFocused)
                        this.__isChanged = false;
                    else if (this.__isChanged) {
                        editor.model.document.differ.reset();
                        this._onChanged();
                    }

                    this.__refreshUI();
                });

                this.__refreshUI();
            });
    }

    getValue(): string {
        if (!this.__editor)
            return null;

        const data = this.__editor.data.get();
        return data ? data : null;
    }
    setValue(value: string) {
        if (!this.__editor)
            return;

        this.__editor.data.set(value ? value : "");

        this.__refreshUI();
    }
    hasValue(): boolean {
        const value = this.normalizeValue(this.element.innerText);
        if (!value)
            return false;

        if (!this.__editor)
            return false;

        const val = this.__editor.model.hasContent(this.__editor.model.document.getRoot(), { ignoreWhitespaces: true });
        return value && val ? true : false;
    }

    protected _onChanged() {
        this.__refreshUI();

        const value = this.getValue();

        this.request({
            url: '/brandup.pages/content/html',
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
        if (this.__editor) {
            const editor = this.__editor;
            this.__editor = null;
            editor.destroy().finally(() => super.destroy());
        }
        else
            super.destroy();
    }
}

export interface HtmlFieldFormOptions {
    placeholder: string;
}