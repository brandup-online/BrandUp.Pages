import { DialogOptions } from "./dialog";
import { FormDialog, FormModel, ComboBoxItem } from "./dialog-form";

export class PageCreateDialog extends FormDialog<PageCreateForm, PageCreateValues, PageModel> {
    readonly collectionId: string;

    constructor(collectionId: string, options?: DialogOptions) {
        super(options);

        this.collectionId = collectionId;
    }

    get typeName(): string { return "BrandUpPages.PageCreateDialog"; }
    protected _getSaveButtonTitle(): string {
        return "Создать";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/page/create`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
        urlParams["collectionId"] = this.collectionId;
    }
    protected _buildForm(model: PageCreateForm) {
        this.setHeader("Параметры новой страницы");

        this.addTextBox("Title", "Название", { placeholder: "Введите название новой страницы" });
        this.addComboBox("PageType", "Тип страницы", { placeholder: "Выберите тип новой страницы" }, model.pageTypes);
    }
}

interface PageCreateForm extends FormModel<PageCreateValues> {
    page: PageModel;
    pageTypes: Array<ComboBoxItem>;
}

interface PageCreateValues {
    title: string;
    urlPath: string;
}

export var createPage = (collectionId: string) => {
    let dialog = new PageCreateDialog(collectionId);
    return dialog.open();
};