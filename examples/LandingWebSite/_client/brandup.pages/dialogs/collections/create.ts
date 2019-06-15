import { FormModel, FormDialog } from "../dialog-form";
import { DialogOptions } from "../dialog";
import { ComboBoxItem } from "../../form/combobox";

export class PageCollectionCreateDialog extends FormDialog<PageCollectionCreateForm, PageCollectionCreateValues, PageCollectionModel> {
    readonly pageId: string;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionCreateDialog"; }

    protected _getSaveButtonTitle(): string {
        return "Создать";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/collection/create`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
        if (this.pageId)
            urlParams["pageId"] = this.pageId;
    }
    protected _buildForm(model: PageCollectionCreateForm) {
        this.setHeader("Создание коллекции страниц");

        this.addTextBox("Title", "Название", { placeholder: "Введите название коллекции" });
        this.addComboBox("PageType", "Тип страниц", { placeholder: "Выберите тип страниц" }, model.pageTypes);
        this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, model.sorts);
    }
}

interface PageCollectionCreateForm extends FormModel<PageCollectionCreateValues> {
    page: PageModel;
    sorts: Array<ComboBoxItem>;
    pageTypes: Array<ComboBoxItem>;
}

interface PageCollectionCreateValues {
    title: string;
    pageType: string;
    sort: string;
}

export var createPageCollection = (pageId: string) => {
    let dialog = new PageCollectionCreateDialog(pageId);
    return dialog.open();
};