import { AjaxQueue } from "@brandup/ui-ajax";
import { FieldProvider } from "../../content/provider/base";
import { Dialog, DialogOptions } from "../dialog";
import { DOM } from "@brandup/ui-dom";
import { Toggler } from "./components/toggler";
import { Breadcrumbs } from "./components/breadcrumbs";
import { editPage } from "./edit";
import { FormField } from "../../content/field/base";
import "../dialog-form.less";
import StartIcon from "../../svg/new/star.svg";

const localValues = [
    { lang: "RU", value: "Большое текст на несколько строчек", default: true },
    { lang: "EN", value: "Большое текст на несколько строчек Большое текст на несколько строчек\nБольшое текст на несколько строчек\nБольшое текст на несколько строчек", default: false },
]

export class LocalizationDialog extends Dialog {
    private __queue?: AjaxQueue;
    private __formElem: HTMLFormElement;
    private __provider: FieldProvider<any, any>;
    private __breadcrumbs?: Breadcrumbs;

    get typeName(): string { return "BrandUpPages.LocalizationDialog"; }

    constructor(provider: FieldProvider<any, any>, options?: DialogOptions) {
        super(options);
        this.__provider = provider;

        this.__formElem = DOM.tag("form", { method: "POST", class: "nopad" }, [
            DOM.tag("p", null, "Ниже представлены значения вариантов перевода поля для разных языков."),
            DOM.tag("h3", null, this.__provider.title),
        ]) as HTMLFormElement;
    }

    protected _onRenderContent() {
        this.element?.classList.add("bp-dialog-form", "localization");
        this.setHeader("Локализация контента");
        if (!this.content) throw new Error("dialog content is not defined");

        const fragment = document.createDocumentFragment();

        // Breadcrumbs
        if (!this.__breadcrumbs) {
            this.__breadcrumbs = new Breadcrumbs(this.__provider.content.host.editor);
            this.__breadcrumbs.on("navigate", (path) => editPage(this.__provider.content, path));
        }
        if (!this.__breadcrumbs.element) throw new Error("Breadcrumbs render error");
        this.__breadcrumbs.render(this.__provider.content.path, this.__provider.content.path);
        fragment.appendChild(this.__breadcrumbs.element);

        this.__renderFields();
        fragment.appendChild(this.__formElem);

        this.content.appendChild(fragment);
    }

    private async __renderFields() {
        const valuesList = DOM.tag("ul", { class: "field-values" });
        for (const val of localValues) {
            const valueElem = await FormField.getValueElem(this.__provider);
            if (!valueElem || !valueElem.element) throw new Error();

            valueElem.setValue(val.value);
            valuesList.append(DOM.tag("li", null, [valueElem.element, DOM.tag("label", null, val.lang), val.default ? StartIcon : null]));
        }
        this.__formElem.appendChild(valuesList);
    }

    destroy() {
        this.__queue?.destroy();

        super.destroy();
    }
}

export const localizationContent = (provider: FieldProvider<any, any>) => {
    const dialog = new LocalizationDialog(provider);
    return dialog.open();
};