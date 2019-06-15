import { DialogOptions } from "../dialog";
import { ListDialog } from "../dialog-list";
import { DOM } from "brandup-ui";
import { assignContentEditor } from "./assign";

export class EditorListDialog extends ListDialog<EditorListModel, ContentEditorModel> {
    readonly pageId: string;
    private __isModified: boolean = false;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.EditorListDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();

        this.setHeader("Редакторы контента");
        this.setNotes("Просмотр и управление редакторами контента.");

        this.registerCommand("item-create", () => {
            assignContentEditor().then(() => {
                this.refresh();
            });
        });
    }

    protected _onClose() {
        if (this.__isModified)
            this.resolve(null);
        else
            super._onClose();
    }

    protected _buildUrl(): string {
        return `/brandup.pages/editor/list`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
    }
    protected _buildList(model: EditorListModel) {
    }
    protected _getItemId(item: ContentEditorModel): string {
        return item.id;
    }
    protected _renderItemContent(item: ContentEditorModel, contentElem: HTMLElement) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("span", {}, item.email)));
    }
    protected _renderItemMenu(item: ContentEditorModel, menuElem: HTMLElement) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-delete" }, "Delete")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Редакторов не назначено.";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
        containerElem.appendChild(DOM.tag("a", { href: "", "data-command": "item-create" }, "Назначить редактора"));
    }
}

interface EditorListModel {
}

export var listEditor = () => {
    let dialog = new EditorListDialog(null);
    return dialog.open();
};