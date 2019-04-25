import { DialogOptions } from "./dialog";
import { FormDialog } from "./dialog-form";
import { AJAXMethod, ajaxRequest } from "brandup-ui";

export class PageCollectionUpdateDialog extends FormDialog<any> {
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
        return `/brandup.pages/collection/${this.collectionId}`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
    }
    protected _getMethod(): AJAXMethod {
        return "POST";
    }
    protected _buildForm() {
        this.setHeader("Параметры коллекции страниц");

        ajaxRequest({
            url: `/brandup.pages/collection/${this.collectionId}`,
            success: (data: PageCollectionModel, status: number) => {
                if (status !== 200) {
                    this.setError("Ошибка загрузки.");
                    return;
                }

                this.addTextBox("Title", "Название", { placeholder: "Введите название коллекции" }, data.title);
                this.addComboBox("Sort", "Сортировка страниц", { placeholder: "Выберите порядок сортировки" }, [{ value: "FirstOld", title: "Сначало старые" }, { value: "FirstNew", title: "Сначало новые" }], data.sort);
            }
        });
    }
}

export var updatePageCollection = (collectionId: string) => {
    let dialog = new PageCollectionUpdateDialog(collectionId);
    return dialog.open();
};