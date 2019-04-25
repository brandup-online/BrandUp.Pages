import { FormDialog } from "./dialog-form";
import { AJAXMethod } from "brandup-ui";
import { DialogOptions } from "./dialog";

export class PageCreateDialog extends FormDialog<PageModel> {
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
        return `/brandup.pages/page`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
        urlParams["collectionId"] = this.collectionId;
    }
    protected _getMethod(): AJAXMethod {
        return "PUT";
    }
    protected _buildForm() {
        this.setHeader("Параметры новой страницы");

        this.addTextBox("Title", "Название", { placeholder: "Введите название новой страницы" }, null);
        this.addComboBox2<PageTypeModel>("PageType", "Тип страницы", { placeholder: "Выберите тип новой страницы" }, null, `/brandup.pages/collection/${this.collectionId}/pageTypes`, (item: PageTypeModel) => { return { value: item.name, title: item.title } });
    }
}

export var createPage = (collectionId: string) => {
    let dialog = new PageCreateDialog(collectionId);
    return dialog.open();
};