import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import { DOM } from "@brandup/ui";
import ContentEditor from "brandup-pages-ckeditor";
import "./html.less";

export class HtmlContent extends Field<string, HtmlFieldFormOptions> implements IContentField {
    readonly form: IContentForm;
    private __isChanged: boolean;
    private __value: HTMLElement;
    private __editor: ContentEditor;

    constructor(form: IContentForm, name: string, options: HtmlFieldFormOptions) {
        super(name, options);

        this.form = form;
    }

    get typeName(): string { return "BrandUpPages.Form.Field.Html"; }

    protected override _onRender() {
        super._onRender();

        this.element.classList.add("html");

        this.element.appendChild(this.__value = DOM.tag("div", { class: "value" }));

        if (this.options.placeholder)
            this.__value.setAttribute("data-placeholder", this.options.placeholder);

        ContentEditor.create(this.__value, { placeholder: this.options.placeholder, language: "ru", blockToolbarEnabled: true })
            .then(editor => {
                this.__editor = editor;

                // change:data срабатывает на любое изменение данных, включая форматирование
                // (атрибуты вроде bold/italic), которое обычный change + hasDataChanges может пропустить.
                editor.model.document.on('change:data', () => {
                    this.__isChanged = true;

                    this.__refreshUI();
                });

                // Сохраняем по потере фокуса редактором через focusTracker, а не по нативному blur
                // редактируемой области: focusTracker считает редактор в фокусе, пока фокус внутри
                // его UI (включая тулбары). Иначе клик по кнопке форматирования вызывает blur области
                // ДО применения формата, и изменение форматирования не сохраняется.
                editor.ui.focusTracker.on('change:isFocused', (_evt, _name, isFocused) => {
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
        if (this.__editor) {
            this.__editor.data.set(value ? value : "");
            this.__refreshUI();
        }
        else
            this.__value.innerHTML = value ? value : "";
    }
    hasValue(): boolean {
        const value = this.normalizeValue(this.__value.innerText);
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

        this.form.request(this, {
            url: '/brandup.pages/content/html',
            method: "POST",
            type: "JSON",
            data: value ? value : "",
            success: (response) => {
                if (response.status === 200) {
                    this.setValue(response.data);
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