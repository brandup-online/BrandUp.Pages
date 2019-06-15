import { DialogOptions } from "../dialog";
import { FormDialog, FormModel } from "../dialog-form";
import { ComboBoxItem } from "../../form/combobox";

export class PageSeoDialog extends FormDialog<PageSeoForm, PageSeoValues, PageModel> {
    readonly pageId: string;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageSeoDialog"; }
    protected _onRenderContent() {
        super._onRenderContent();

        this.setHeader("Параметры SEO");
    }
    protected _getSaveButtonTitle(): string {
        return "Сохранить";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/page/seo`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
        urlParams["pageId"] = this.pageId;
    }
    protected _buildForm(model: PageSeoForm) {
        this.addTextBox("Title", "Заголовок страницы", { });
        this.addTextBox("Description", "Описание страницы", { });
        this.addStringArray("Keywords", "Ключевые слова", { placeholder: "Введите ключевое слово" });
    }
}

interface PageSeoForm extends FormModel<PageSeoValues> {
    page: PageModel;
    pageTypes: Array<ComboBoxItem>;
}

interface PageSeoValues {
    title: string;
    description: string;
    keywords: string;
}

export var seoPage = (collectionId: string) => {
    let dialog = new PageSeoDialog(collectionId);
    return dialog.open();
};