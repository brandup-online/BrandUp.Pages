import { CommandContext } from "@brandup/ui";
import { DialogOptions } from "../dialog";
import { ListDialog } from "../dialog-list";
import { DOM } from "@brandup/ui-dom";

export class ContentTypeListDialog extends ListDialog<ContentTypeListModel, ContentTypeItemModel> {
    private __isModified: boolean = false;
    private navElem: HTMLElement;
    private baseContentType?: string | null;

    constructor(baseContentType: string | null, options?: DialogOptions) {
        super(options);

        this.navElem = DOM.tag("ol", { class: "nav" });

        this.baseContentType = baseContentType ? baseContentType : null;
    }

    get typeName(): string { return "BrandUpPages.PageCollectionListDialog"; }

    protected _onRenderContent() {
        super._onRenderContent();

        this.setHeader("Типы контента");
        this.setNotes("Просмотр и настройка типов контента страниц.");

        this.registerItemCommand("nav", (id: string) => {
            this.baseContentType = id;
            this.refresh();
        });

        this.registerCommand("nav2", (context: CommandContext) => {
            let name = context.target.getAttribute("data-value");
            this.baseContentType = name ? name : null;
            this.refresh();
        });
    }

    protected _onClose() {
        if (this.__isModified)
            this.resolve({});
        else
            super._onClose();
    }
    
    protected _buildUrl(): string {
        return `/brandup.pages/content-type/list`;
    }
    protected _buildUrlParams(urlParams: { [key: string]: string; }) {
        if (this.baseContentType)
            urlParams["baseType"] = this.baseContentType;
    }
    protected _buildList(model: ContentTypeListModel) {
        if (!this.navElem) {
            this.content?.insertAdjacentElement("afterbegin", this.navElem);
        }
        else {
            DOM.empty(this.navElem);
        }

        this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav2", "data-value": "" }, "root")));
        if (model.parents && model.parents.length) {
            for (let i = 0; i < model.parents.length; i++) {
                this.navElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "nav2", "data-value": model.parents[i] }, model.parents[i])));
            }
        }
    }
    protected _getItemId(item: ContentTypeItemModel): string {
        return item.name;
    }
    protected _renderItemContent(item: ContentTypeItemModel, contentElem: HTMLElement) {
        contentElem.appendChild(DOM.tag("div", { class: "title" }, DOM.tag("a", { href: "", "data-command": "nav" }, item.title)));
    }
    protected _renderItemMenu(item: ContentTypeItemModel, menuElem: HTMLElement) {
        menuElem.appendChild(DOM.tag("li", null, [DOM.tag("a", { href: "", "data-command": "item-templates" }, "Шаблоны данных")]));
    }
    protected _renderEmpty(container: HTMLElement) {
        container.innerText = "Типов контента не найдено";
    }
    protected _renderNewItem(containerElem: HTMLElement) {
    }
}

interface ContentTypeListModel {
    parents: Array<string>;
}

interface ContentTypeItemModel {
    name: string;
    title: string;
    isAbstract: boolean;
}

export var listContentType = (baseContentType: string | null = null) => {
    let dialog = new ContentTypeListDialog(baseContentType);
    return dialog.open();
};