import { DOM } from "brandup-ui-dom";
import { IContentEditor, IContentHost } from "../typings/content";
import { AjaxQueue, AjaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { WebsiteContext } from "brandup-ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import { editPage } from "../dialogs/pages/edit";
import { UIElement } from "brandup-ui";
import { Content } from "./content";
import { GetContentEditResult } from "../typings/models";
import { ModelFieldProvider } from "./provider/model";
import { errorPage } from "../dialogs/dialog-error";

export class ContentEditor extends UIElement implements IContentEditor, IContentHost {
    readonly website: WebsiteContext;
    readonly editId: string;
    readonly contentElem: HTMLElement;
    readonly queue: AjaxQueue;
    private __contents: Map<string, Content> = new Map();
    private __content: Content;
    private __isLoading = false;

    get typeName(): string { return "BrandUpPages.Editor"; }
    
    constructor(website: WebsiteContext, editId: string) {
        super();

        this.website = website;
        this.editId = editId;
        this.queue = new AjaxQueue();

        //const contentElem = DOM.queryElement(document.body, `[data-content-root='${contentKey}']`);

        this.buildContent("")
            .then(() => {
                this.__renderToolbar();

                this.contentElem.classList.add("root-designer");
                document.body.classList.add("bp-state-design");
            }).catch(() => {
                console.error("Error building content.");
            });
    }
    
    // IContentEditor members

    get content(): Content { return this.__content; }

    navigate(path: string) {
        return this.__contents.get(path);
    }

    api(request: AjaxRequest) {
        this.queue.push(request);
    }

    // IContentHost members
    get editor(): IContentEditor { return this; }
    get isList(): boolean { return false; }

    attach(content: Content) {
        if (this.__content)
            throw "Model field already exist content.";
        this.__content = content;
    }

    // Editor members

    buildContent(path: string): Promise<Content> {
        return new Promise<Content>((resolve, reject) => {
            this.__isLoading = true;

            this.queue.push({
                url: "/brandup.pages/page/content",
                urlParams: { editId: this.editId, path },
                method: "GET",
                success: (response: AjaxResponse<GetContentEditResult>) => {
                    this.__isLoading = false;

                    if (response.status !== 200)
                        throw "Error get content.";

                    const content = this.__renderContent(response.data);
                    resolve(content);
                },
            });
        });
    }

    private __renderContent(edit: GetContentEditResult): Content {
        const contentElements = new Map<string, ContentStructure>();
        //if (rootContainer) {
        //    const addContainer = elem => {
        //        const contentPath = elem.dataset.contentPath;

        //        contentElements.set(contentPath, {
        //            path: contentPath,
        //            container: elem,
        //            fields: new Map<string, HTMLElement>
        //        });
        //    };

        //    // Ensure all content elements
        //    addContainer(rootContainer);
        //    DOM.queryElements(rootContainer, "[data-content-path]").forEach(addContainer);

        //    // Ensure all fields by contents
        //    DOM.queryElements(rootContainer, "[data-content-field-path][data-content-field-name]").forEach(elem => {
        //        const contentPath = elem.dataset.contentFieldPath;
        //        const fieldName = elem.dataset.contentFieldName;
        //        contentElements.get(contentPath).fields.set(fieldName, elem);
        //    });
        //}

        // Create content wrappers
        let rootContent: Content;
        edit.contents.forEach((contentModel, index) => {
            const contentStructure = contentElements.get(contentModel.path);

            let host: IContentHost;
            if (contentModel.parentPath !== null) {
                const parentContent = this.navigate(contentModel.parentPath);
                if (!parentContent)
                    throw `Not found content by path "${contentModel.parentPath}".`;
                host = <ModelFieldProvider>parentContent.getField(contentModel.parentField);
            }
            else
                host = this;

            const content = new Content(host, contentModel, contentStructure?.container, contentStructure?.fields);
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
        
        this.website.nav({ url: url.toString(), replace: true });
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