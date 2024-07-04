import { DialogOptions } from "../dialog";
import { FormDialog, FormModel } from "../dialog-form";
import { ComboBoxItem } from "../../form/combobox";
import { PageCollectionModel, PageModel } from "../../typings/page";

export class PageCollectionUpdateDialog extends FormDialog<PageCollectionUpdateForm, PageCollectionUpdateValues, PageCollectionModel> {
    readonly collectionId: string;

    constructor(collectionId: string, options?: DialogOptions) {
        super(options);

        this.collectionId = collectionId;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionUpdateDialog"; }
    
    protected _getSaveButtonTitle(): string {
        return "Сохранить";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/collection/${this.collectionId}/update`;
    }
    protected _buildForm(model: PageCollectionUpdateForm) {
        this.setHeader("Параметры коллекции страниц");

        this.addTextBox("Title", "Название", { placeholder: "Введите название коллекции" });
        this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, model.sorts);
    }
}

interface PageCollectionUpdateForm extends FormModel<PageCollectionUpdateValues> {
    page: PageModel;
    sorts: Array<ComboBoxItem>;
}

interface PageCollectionUpdateValues {
    title: string;
    sort: string;
}

export var updatePageCollection = (collectionId: string) => {
    let dialog = new PageCollectionUpdateDialog(collectionId);
    return dialog.open();
};