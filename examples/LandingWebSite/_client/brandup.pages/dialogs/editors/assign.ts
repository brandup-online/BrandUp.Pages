import { FormModel, FormDialog } from "../dialog-form";
import { DialogOptions } from "../dialog";
import { ComboBoxItem } from "../../form/combobox";

export class ContentEditorAssignDialog extends FormDialog<ContentEditorAssignForm, ContentEditorAssignValues, ContentEditorModel> {
    constructor(options?: DialogOptions) {
        super(options);
    }

    get typeName(): string { return "BrandUpPages.AssignContentEditorDialog"; }

    protected _getSaveButtonTitle(): string {
        return "Назначить";
    }
    protected _buildUrl(): string {
        return `/brandup.pages/editor/assign`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
    }
    protected _buildForm(model: ContentEditorAssignForm) {
        this.setHeader("Назначение редактора");

        this.addTextBox("Email", "E-mail", { placeholder: "Введите e-mail пользователя" });
    }
}

interface ContentEditorAssignForm extends FormModel<ContentEditorAssignValues> {
}

interface ContentEditorAssignValues {
    email: string;
}

export var assignContentEditor = () => {
    let dialog = new ContentEditorAssignDialog();
    return dialog.open();
};