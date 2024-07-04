import { DOM } from "brandup-ui-dom";
import { IPageDesigner, IContentFieldDesigner } from "../typings/content";
import { AjaxQueue, AjaxResponse } from "brandup-ui-ajax";
import { Page } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import { editPage } from "../dialogs/pages/edit";
import { UIElement } from "brandup-ui";
import { Content } from "./content";
import { ContentModel } from "../typings/models";
import { ModelFieldProvider } from "./provider/model";
import { errorPage } from "../dialogs/dialog-error";

export class Editor extends UIElement implements IPageDesigner {
    readonly page: Page;
    readonly editId: string;
    readonly contentElem: HTMLElement;
    readonly queue: AjaxQueue;
    private __contents: Map<string, Content> = new Map();
    private __content: Content;
    private __accentedField: IContentFieldDesigner = null;
    private __isLoading = false;

    get typeName(): string { return "BrandUpPages.Editor"; }
    get content(): Content { return this.__content; }

    constructor(page: Page, contentElem: HTMLElement) {
        super();
              
        this.page = page;
        this.editId = contentElem.dataset["contentEditId"];
        this.contentElem = contentElem;
        this.queue = new AjaxQueue();

        this.buildContent(this.contentElem)
            .then((content) => {
                this.__content = content;

                this.__renderToolbar();

                this.contentElem.classList.add("root-designer");
                document.body.classList.add("bp-state-design");
            }).catch(() => {
                console.error("Error building content.");
            });
    }

    buildContent(container: HTMLElement): Promise<Content> {
        if (!container.hasAttribute("data-content-path"))
            throw "Require data-content-path attribute.";
            
        return new Promise<Content>((resolve, reject) => {
            this.__isLoading = true;
            this.queue.push({
                url: "/brandup.pages/page/content",
                urlParams: { editId: this.editId, path: container.dataset.contentPath },
                method: "GET",
                success: (response: AjaxResponse<ContentModel[]>) => {
                    this.__isLoading = false;

                    if (response.status !== 200)
                        throw "Error get content.";

                    const content = this.__renderContent(container, response.data);
                    resolve(content);
                },
            });
        });
    }

    private __renderContent(rootContainer: HTMLElement, contents: ContentModel[]): Content {
        const contentElements = new Map<string, ContentStructure>();
        const addContainer = elem => {
            const contentPath = elem.dataset.contentPath;

            contentElements.set(contentPath, {
                path: contentPath,
                container: elem,
                fields: new Map<string, HTMLElement>
            });
        };

        // Ensure all content elements
        addContainer(rootContainer);
        DOM.queryElements(rootContainer, "[data-content-path]").forEach(addContainer);

        // Ensure all fields by contents
        DOM.queryElements(rootContainer, "[data-content-field-path][data-content-field-name]").forEach(elem => {
            const contentPath = elem.dataset.contentFieldPath;
            const fieldName = elem.dataset.contentFieldName;
            contentElements.get(contentPath).fields.set(fieldName, elem);
        });

        // Create content wrappers
        let rootContent: Content;
        contents.forEach((contentModel, index) => {
            const contentStructure = contentElements.get(contentModel.path);

            let parentField: ModelFieldProvider = null;
            if (contentModel.parentPath !== null) {
                const parentContent = this.getContentItem(contentModel.parentPath);
                parentField = <ModelFieldProvider>parentContent.getField(contentModel.parentField);
            }

            const content = new Content(this, parentField, contentModel, contentStructure.container, contentStructure.fields);
            this.__contents.set(content.path, content);

            if (index === 0)
                rootContent = content;
        });

        return rootContent;
    }

    private __renderToolbar() {
        const toolbarElem = DOM.tag("div", { class: "bp-elem editor-toolbar" }, [
            DOM.tag("button", { class: "bp-button", command: "bp-commit", title: "Применить изменения" }, [saveIcon, "Сохранить"]),
            DOM.tag("button", { class: "bp-button secondary", command: "bp-discard", title: "Отменить изменения" }, [cancelIcon, "Отмена"]),
            DOM.tag("button", { class: "bp-button neutral right", command: "bp-content", title: "Показать контент" }, [editBlockIcon, "Контент"]),
        ]);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.__initToolbarLogic();
    }

    private __initToolbarLogic() {
        this.registerCommand("bp-content", () => {
            editPage(this, "");
        });

        this.registerCommand("bp-commit", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.queue.push({
                url: "/brandup.pages/page/content/commit",
                urlParams: { editId: this.editId },
                method: "POST",
                success: (response) => {
                    this.__isLoading = false;
                    if (response.status !== 200)
                        throw "Error commit content editing.";
                    if (response.data.isSuccess)
                        this.__complateEdit();
                    else {
                        errorPage(this, response.data.validation);
                    }
                }
            });
        });

        this.registerCommand("bp-discard", () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            this.queue.push({
                url: "/brandup.pages/page/content/discard",
                urlParams: { editId: this.editId },
                method: "POST",
                success: (response) => {
                    if (response.status !== 200)
                        throw "Error discard content editing.";

                    this.__complateEdit();
                },
            });
        });
    }

    getContentItem(path: string) {
        if (!this.__contents.has(path))
            throw `Not found content by path "${path}".`;
        return this.__contents.get(path);
    }

    removeContentItem(path: string) {
        this.__contents.forEach((content, key) => {
            if (key.startsWith(path)) {
                content.destroy();
                this.__contents.delete(key);
            }
        });
    }

    private __complateEdit() {
        delete this.contentElem.dataset["contentEditId"];
        
        const url = new URL(location.href);
        url.searchParams.delete("editid");
        
        this.page.website.nav({ url: url.toString(), replace: true });
        this.__isLoading = false;
    }

    destroy() {
        this.__contents.forEach(content => content.destroy());

        this.queue.destroy();

        this.contentElem.classList.remove("root-designer");
        document.body.classList.remove("bp-state-design");

        this.element.remove();

        super.destroy();
    }
}

interface ContentStructure {
    path: string;
    container: HTMLElement;
    fields: Map<string, HTMLElement>;
}