import { DialogOptions } from "../dialog";
import { ListDialog } from "../dialog-list";
import { DOM } from "brandup-ui";
import { assignPageEditor } from "./assign";
import { deletePageEditor } from "./delete";

export class PageEditorListDialog extends ListDialog<EditorListModel, PageEditorModel> {
    readonly pageId: string;
    private __isModified: boolean = false;

    constructor(pageId: string, options?: DialogOptions) {
        super(options);

        this.pageId = pageId;
    }

    get typeName(): string { return "BrandUpPages.PageEditorListDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();

        this.setHeader("Редакторы страниц");
        this.setNotes("Просмотр и управление редакторами страниц.");

        this.registerCommand("item-create", () => {
            assignPageEditor().then(() => {
                this.loadItems();
            });
        });

        this.registerItemCommand("item-delete", (id) => {
            deletePageEditor(id).then(() => {
                this.loadItems();
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
    protected _getItemId(item: PageEditorModel): string {
        return item.id;
    }
    protected _renderItemContent(item: PageEditorModel, contentElem: HTMLElement) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("span", {}, item.email)));
    }
    protected _renderItemMenu(item: PageEditorModel, menuElem: HTMLElement) {
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

export var listPageEditor = () => {
    let dialog = new PageEditorListDialog(null);
    return dialog.open();
};