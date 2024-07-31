import { DOM } from "@brandup/ui-dom";
import { AjaxQueue, AjaxRequest, AjaxResponse } from "@brandup/ui-ajax";
import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";
import editBlockIcon from "../svg/new/edit-block.svg";
import saveIcon from "../svg/toolbar-button-save.svg";
import cancelIcon from "../svg/new/cancel.svg";
import { editPage } from "../dialogs/content/edit";
import { UIElement } from "@brandup/ui";
import { Content, IContentHost } from "./content";
import { ModelFieldProvider } from "./provider/model";
import { errorPage } from "../dialogs/dialog-error";
import { BeginContentEditResult, ContentModel, GetContentEditResult, ValidationContentModel } from "../typings/content";
import defs from "./defs";

defs.registerFormField("html", () => import ("./field/html"));
defs.registerFormField("text", () => import ("./field/text"));
defs.registerFormField("model", () => import ("./field/model"));
defs.registerFormField("image", () => import ("./field/image"));
defs.registerFormField("hyperlink", () => import ("./field/hyperlink"));
defs.registerFormField("pages", () => import ("./field/pages"));

export class ContentEditor extends UIElement implements IContentHost {
    readonly website: WebsiteApplication;
    readonly editId: string;
    readonly queue: AjaxQueue;

    private __contentElem?: HTMLElement;
    private __contents: Map<string, Content> = new Map();
    private __content?: Content;
    private __isLoading = false;

    private __complate?: ((value: EditResult) => void);
    private __error?: ((reason?: any) => void);

    get typeName(): string { return "BrandUpPages.Editor"; }
    
    private constructor(website: WebsiteApplication, editId: string) {
        super();

        this.website = website;
        this.editId = editId;
        this.queue = new AjaxQueue();
    }
    
    static begin(page: Page<WebsiteApplication,PageModel>, key: string, type: string, force: boolean = false) {
        return new Promise<{ editId: string, exist: boolean }>(async (resolve) => {
            const response: AjaxResponse<BeginContentEditResult> = await page.website.queue.enque({
                url: "/brandup.pages/page/content/begin",
                query: { bpcommand: "begin", key: key, type: type, force: force.toString() },
                method: "POST",
            });

            if (response.status !== 200 || !response.data)
                throw new Error("Error begin content edit.");
                
            resolve({ editId: response.data.editId, exist: response.data.exist });
        });
    }

    static create(website: WebsiteApplication, editId: string) {
        const editor = new ContentEditor(website, editId);
        return editor;
    }

    edit(container: HTMLElement) {
        if (document.body.classList.contains("bp-state-design"))
            throw new Error("Content editor already started.");
        document.body.classList.add("bp-state-design");
        
        this.__contentElem = container;
        this.__contentElem.classList.add("root-designer");

        return this.loadContent("")
            .then(() => {
                this.__renderToolbar();
                this.__renderDesigner();
                
                return new Promise<EditResult>((resolve, reject) => {
                    this.__complate = resolve;
                    this.__error = reject;
                });
            });
    }
    
    // IContentHost members
    get editor(): ContentEditor { return this; }
    get isList(): boolean { return false; }

    attach(content: Content) {
        if (this.__content)
            throw new Error("Model field already exist content.");
        this.__content = content;
    }

    // Editor members

    get content(): Content | undefined { return this.__content; }

    navigate(path: string) {
        const content = this.__contents.get(path);
        if (!content) throw new Error(`content by path "${path}" not found`);
        return content;
    }

    api(request: AjaxRequest) {
        return this.queue.enque(request);
    }

    loadContent(path: string): Promise<Content> {
        return new Promise<Content>(async (resolve, reject) => {
            const response: AjaxResponse<GetContentEditResult> = await this.api({
                url: "/brandup.pages/page/content",
                query: { editId: this.editId, path },
                method: "GET",
            });

            if (response.status !== 200 || !response.data) {
                reject(`Error load editor contents by path "${path}".`);
                return;
            }

            let rootContent: Content | null = this.initContent(response.data.contents);
            if (!rootContent) throw new Error("rootContent not found");
            resolve(rootContent);
        });
    }

    initContent(contents: Array<ContentModel>): Content {
        let rootContent: Content;

        if (!contents.length) throw new Error("empty contents array");

        contents.forEach((contentModel, index) => {
            if (this.__contents.has(contentModel.path))
                throw new Error(`Content "${contentModel.path}" already initialized.`);

            let host: IContentHost;
            if (contentModel.parentPath !== null) {
                const parentContent = this.navigate(contentModel.parentPath);
                if (!parentContent)
                    throw new Error(`Not found content by path "${contentModel.parentPath}".`);
                host = <ModelFieldProvider>parentContent.getField(contentModel.parentField);
            }
            else
                host = this;

            const content = new Content(host, contentModel);
            this.__contents.set(content.path, content);

            if (index === 0)
                rootContent = content;
        });

        return rootContent!;
    }
    
    private __renderToolbar() {
        const toolbarElem = DOM.tag("div", { class: "bp-elem editor-toolbar" }, [
            DOM.tag("menu", "primary", [
                DOM.tag("button", { class: "bp-button", command: "bp-commit", title: "Применить изменения" }, [saveIcon, DOM.tag("span", null, "Сохранить")]),
                DOM.tag("button", { class: "bp-button secondary", command: "bp-discard", title: "Отменить изменения" }, [cancelIcon, DOM.tag("span", null, "Отмена")])
            ]),
            DOM.tag("menu", null, [
                DOM.tag("button", { class: "bp-button neutral language", command: "change-language", title: "Сменить язык" }, "en"),
                DOM.tag("button", { class: "bp-button neutral", command: "bp-content", title: "Показать контент" }, [editBlockIcon, DOM.tag("span", null, "Контент")])
            ])
        ]);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        const rootContent = this.__contents.get("");
        if (rootContent)
            this.registerCommand("bp-content", () => editPage(rootContent, ""));

        this.registerCommand("bp-commit", async () => {
            if (this.__isLoading)
                return;

            const errorModels: ValidationContentModel[] = this.validate();
            if (errorModels.length)
                return errorPage(this, errorModels);

            this.__isLoading = true;

            const response = await this.api({
                url: "/brandup.pages/page/content/commit",
                query: { editId: this.editId },
                method: "POST",
            });

            this.__isLoading = false;
            if (response.status !== 200)
                throw new Error("Error commit content editing.");

            if (response.data.isSuccess) {
                if (!this.__complate)
                    throw new Error("Complate function not set");
                this.__complate({ reason: "Commit" });
            } else {
                errorPage(this, response.data.validation);
            }
        });

        this.registerCommand("bp-discard", async () => {
            if (this.__isLoading)
                return;
            this.__isLoading = true;

            const response = await this.api({
                url: "/brandup.pages/page/content/discard",
                query: { editId: this.editId },
                method: "POST",
            });

            if (response.status !== 200)
                throw new Error("Error discard content editing.");
            if (!this.__complate)
                throw new Error("Complate function not set");

            this.__complate({ reason: "Discard" });
        });
    }

    validate() {
        const errorModels: ValidationContentModel[] = [];
        this.__contents.forEach(content => {
            const errors = content.validate();
            if (errors) errorModels.push(errors);
        })

        return errorModels;
    }
    
    private __renderDesigner() {
        if (!this.__contentElem)
            return;

        const contentElements = new Map<string, ContentStructure>();

        const addContainer = (elem: HTMLElement) => {
            const contentPath = elem.dataset.contentPath;
            if (contentPath === null || contentPath === undefined)
                return;

            contentElements.set(contentPath, {
                path: contentPath,
                container: elem,
                fields: new Map<string, HTMLElement>
            });
        };

        // Ensure all content elements
        addContainer(this.__contentElem);
        DOM.queryElements(this.__contentElem, "[data-content-path]").forEach(addContainer);

        // Ensure all fields by contents
        DOM.queryElements(this.__contentElem, "[data-content-field-path][data-content-field-name]").forEach(elem => {
            const contentPath = elem.dataset.contentFieldPath;
            const fieldName = elem.dataset.contentFieldName;
            contentElements.get(contentPath!)!.fields.set(fieldName!, elem);
        });

        Array.from(contentElements.values()).forEach(contentStructure => {
            if (!this.__contents.has(contentStructure.path))
                throw new Error(`Content is not exist by path "${contentStructure.path}".`);

            const content = this.__contents.get(contentStructure.path);
            content?.renderDesigner(contentStructure);
        });
    }

    destroy() {
        if (this.__complate)
            this.__complate({ reason: "Destroy" });

        this.__contents.forEach(content => content.destroy());

        this.queue.destroy();

        this.__contentElem?.classList.remove("root-designer");
        document.body.classList.remove("bp-state-design");

        this.element?.remove();

        super.destroy();
    }
}

export interface EditResult {
    reason: "Commit" | "Discard" | "Destroy";
}

interface ContentStructure {
    path: string;
    container: HTMLElement;
    fields: Map<string, HTMLElement>;
}