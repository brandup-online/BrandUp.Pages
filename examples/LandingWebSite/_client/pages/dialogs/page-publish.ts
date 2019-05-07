﻿import { FormDialog } from "./dialog-form2";
import { DialogOptions } from "./dialog";
import { FormModel } from "./dialog-form2";

export class PagePublishDialog extends FormDialog<PagePublishForm, PagePublishValues, PagePublishResult> {
    readonly pageId: string;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PagePublishDialog"; }
    protected _buildUrl(): string {
        return `/brandup.pages/page/${this.pageId}/publish`;
    }
    protected _buildForm() {
        this.setHeader("Публикация страницы");

        this.addTextBox("title", "Заголовок title", { });
        this.addTextBox("urlPath", "Название в url", { placeholder: "Введите название в url для страницы" });
    }
    protected _getSaveButtonTitle(): string {
        return "Опубликовать";
    }
}

interface PagePublishForm extends FormModel<PagePublishValues> {
    page: PageModel;
}

interface PagePublishValues {
    title: string;
    urlPath: string;
}

interface PagePublishResult {
    url: string;
}

export var publishPage = (pageId: string) => {
    let dialog = new PagePublishDialog(pageId);
    return dialog.open();
};