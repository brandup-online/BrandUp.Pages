import { FormModel, FormDialog } from "../dialog-form";
import { DialogOptions } from "../dialog";
import { ComboBoxItem } from "../../form/combobox";

export class PageEditorAssignDialog extends FormDialog<PageEditorAssignForm, PageEditorAssignValues, PageEditorModel> {
    constructor(options?: DialogOptions) {
        super(options);
    }

    get typeName(): string { return "BrandUpPages.PageEditorAssignDialog"; }

    protected _getSaveButtonTitle(): string {
        return "Назначить";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/editor/assign`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
    }
    protected _buildForm(model: PageEditorAssignForm) {
        this.setHeader("Назначение редактора");

        this.addTextBox("Email", "E-mail", { placeholder: "Введите e-mail пользователя" });
    }
}

interface PageEditorAssignForm extends FormModel<PageEditorAssignValues> {
}

interface PageEditorAssignValues {
    email: string;
}

export var assignPageEditor = () => {
    let dialog = new PageEditorAssignDialog();
    return dialog.open();
};