import { UIElement } from "brandup-ui";
import ContentPage from "../pages/content";
import { browserPage } from "../dialogs/pages/browser";
import iconBack from "../svg/toolbar-button-back.svg";
import iconDown from "../svg/new/arrow-down.svg";
import iconPublish from "../svg/new/upload.svg";
import iconSeo from "../svg/new/increase.svg";
import { listContentType } from "../dialogs/content-types/list";
import { Page, PageModel } from "brandup-ui-website";
import { publishPage } from "../dialogs/pages/publish";
import { seoPage } from "../dialogs/pages/seo";
import { DOM } from "brandup-ui-dom";
import { ajaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { ContentEditor } from "../content/editor";

import iconList from "../svg/new/menu.svg";
import iconEdit from "../svg/new/edit.svg";
import iconMore from "../svg/new/more.svg";
import iconPlus from "../svg/new/plus.svg";

const EDITID_QUERY_KEY = "editid";

export class PageToolbar extends UIElement {
    private __page: Page<PageModel>;
    private __isDestroyed: boolean = false;

    readonly isContentPage: boolean;

    private __menuCloseFunc: (e: MouseEvent) => void;
    private __menuExpandedElem: HTMLElement = null;

    private __editor: ContentEditor = null;
    
    get typeName(): string {
        return "BrandUpPages.PageToolbar";
    }
    get hasEditor(): boolean { return !!this.__editor; }

    constructor(page: Page<PageModel>) {
        super();

        this.__page = page;
        this.isContentPage = page instanceof ContentPage;

        const editId = page.nav.query[EDITID_QUERY_KEY];
        if (editId) {
            this.__editor = ContentEditor.create(page.website, editId);

            const editContent = findEditContent(editId);
            this.__editor.edit(editContent?.container)
                .then(() => {
                    if (this.__isDestroyed)
                        return;

                    delete page.nav.query[EDITID_QUERY_KEY];

                    const editUrl = this.__page.website.buildUrl(page.nav.path, page.nav.query);
                    if (!editContent || editContent.inPage)
                        this.__page.website.nav({ url: editUrl, replace: true });
                    else
                        window.location.replace(editUrl);
                })
                .catch(() => console.error("Error in content editor."));
        }
        else
            this.__renderTollbar();
    }

    private __renderTollbar() {
        document.body.classList.add("bp-state-toolbars");

        const websiteMenuItems = document.createDocumentFragment();
        websiteMenuItems.append(DOM.tag("button", { command: "bp-content-types" }, [iconPlus, DOM.tag("span", null, "Типы контента")]));
        websiteMenuItems.append(DOM.tag("button", { command: "bp-pages" }, [iconList, DOM.tag("span", null, "Страницы этого уровня")]));

        let websiteMenu: HTMLElement;

        const toolbarButtons = [
            DOM.tag("div", { class: "first-button" }, [
                DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Меню сайта" }, iconList),
                websiteMenu = DOM.tag("menu", { class: "bp-page-toolbar-menu" }),
            ])
        ];
        
        const pageContents = findContents();
        if (pageContents.length) {
            let contentsMenuElem: HTMLElement;
            toolbarButtons.push(DOM.tag("div", null, [
                DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Редактировать контент страницы" }, iconEdit),
                contentsMenuElem = DOM.tag("menu", { class: "bp-page-toolbar-menu" })
            ]));

            pageContents.forEach(contentDefinition => {
                contentsMenuElem.appendChild(DOM.tag("button", { command: "bp-content-edit", dataset: { contentKey: contentDefinition.key, contentType: contentDefinition.type } }, [
                    DOM.tag('span', null, [contentDefinition.title, DOM.tag("i", null, contentDefinition.key)])
                ]));
            })
        }
        
        // Если страница динамическая
        if (this.isContentPage) {
            const contentPage = <ContentPage>this.__page;

            websiteMenuItems.append(DOM.tag("button", { command: "bp-pages-child" }, [iconDown, DOM.tag("span", null, "Дочерние страницы")]));
            
            if (contentPage.model.parentPageId) {
                websiteMenuItems.append(DOM.tag("button", { command: "bp-back" }, [iconBack, DOM.tag("span", null, "Перейти к родительской странице")]));
            }

            toolbarButtons.push(DOM.tag("div", null, DOM.tag("button", { class: "bp-page-toolbar-button page-status " + contentPage.model.status.toLowerCase() }, [DOM.tag("span")])));

            const pageMenuItems = [DOM.tag("button", { command: "bp-seo" }, [iconSeo, DOM.tag("span", null, "Индексирование страницы")]) ];
            
            if (contentPage.model.status !== "Published") {
                toolbarButtons.push(DOM.tag("div", null, DOM.tag("button", { class: "bp-page-toolbar-button", command: "bp-publish", title: "Опубликовать" }, iconPublish)));
                pageMenuItems.push(DOM.tag("button", { command: "bp-pages" }, [iconPublish, DOM.tag("span", null, "Опубликовать страницу")]));
            }

            toolbarButtons.push(DOM.tag("div", null, [
                DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Действия над страницей" }, iconMore),
                DOM.tag("menu", { class: "bp-page-toolbar-menu", title: "" }, pageMenuItems),
            ]));
        }
        websiteMenu.append(websiteMenuItems);

        const toolbarElem = DOM.tag("div", { class: "bp-elem bp-page-toolbar" }, toolbarButtons);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.__initTollbarLogic();
    }

    private __initTollbarLogic() {
        this.registerCommand("show-menu", (elem) => {
            if (elem.classList.toggle("expanded"))
                this.__openMenu(elem);
            else
                this.__closeMenu();
        });
        
        this.__menuCloseFunc = (e: MouseEvent) => {
            const target = e.target as HTMLElement;
            if (target.closest(`.bp-page-toolbar-menu`))
                return; // если клик внутри меню, то делать ничего не нужно

            this.__closeMenu();

            const clickedMenuItem = target.closest(`[data-command='show-menu'`);
            if (clickedMenuItem && this.__menuExpandedElem == clickedMenuItem) {
                // если клик по тому же элементу, который открыл контекстное меню, то останавливаем обработку клика, чтобы оно не отрылось заново

                e.preventDefault();
                e.stopImmediatePropagation();
            }
        };

        this.registerCommand("bp-back", () => {
            let parentPageId: string = null;
            if (this.isContentPage)
                parentPageId = this.__page.model.parentPageId;
            if (parentPageId) {
                ajaxRequest({
                    url: `/brandup.pages/page/${parentPageId}`,
                    success: (response: AjaxResponse<PageModel>) => {
                        this.__page.website.nav({ url: response.data.url });
                    }
                });
            }
        });

        const published = this.__page.model.status?.toLowerCase() === "published";

        if (!published)
            this.registerCommand("bp-publish", () => {
                publishPage(this.__page.model.id).then(result => {
                    this.__page.website.nav({ url: result.url, replace: true });
                });
            });

        this.registerCommand("bp-pages", () => {
            let parentPageId: string = null;
            if (this.isContentPage)
                parentPageId = this.__page.model.parentPageId;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-pages-child", () => {
            let parentPageId: string = null;
            if (this.isContentPage)
                parentPageId = this.__page.model.id;
            browserPage(parentPageId);
        });

        this.registerCommand("bp-content-types", () => {
            this.element.classList.remove("opened-menu");
            listContentType();
        });

        this.registerCommand("bp-seo", () => {
            seoPage(this.__page.model.id).then(() => {
                this.__page.website.app.reload();
            });
        });

        this.registerAsyncCommand("bp-content-edit", (context) => {
            this.__closeMenu();
            
            const contentKey = context.target.dataset.contentKey;
            const contentType = context.target.dataset.contentType;
            if (!contentKey || !contentType)
                throw "Not set content edit parameters.";

            ContentEditor.begin(this.__page, contentKey, contentType)
                .then(result => {
                    if (result.exist && !confirm("Continue editing?"))
                        return ContentEditor.begin(this.__page, contentKey, contentType, true);
                        
                    return result;
                })
                .then(result => {
                    const editUrl = this.__page.website.buildUrl(null, { editid: result.editId });

                    const contentLocation = findContent(contentKey)
                    if (!contentLocation || contentLocation.inPage)
                        this.__page.website.nav({ url: editUrl, replace: true });
                    else
                        window.location.replace(editUrl);
                })
                .catch(() => {
                    console.log(`Unable to begin edit content by key "${contentKey}".`);
                })
                .finally(() => {
                    context.complate();
                });
        });
    }
    
    private __openMenu(ownElem: HTMLElement) {
        document.body.addEventListener("click", this.__menuCloseFunc);

        this.__menuExpandedElem = ownElem;
    }

    private __closeMenu() {
        if (this.__menuExpandedElem)
            this.__menuExpandedElem.classList.remove("expanded"); // закрываем последнее открытое контекстное меню

        document.body.removeEventListener("click", this.__menuCloseFunc);
    }

    destroy() {
        if (this.__isDestroyed)
            return;
        this.__isDestroyed = true;

        if (this.__editor)
            this.__editor.destroy();

        this.__closeMenu();
        
        this.element?.remove();
        document.body.classList.remove("bp-state-toolbars");

        super.destroy();
    }
}


const createContentLocation = (container: HTMLElement): ContentLocation => {
    const contentKey = container.dataset.contentRoot;
    if (!contentKey) throw "Not set content root value.";

    const contentType = container.dataset.contentType;
    if (!contentType) throw "Not set content type value.";

    const contentTitle = container.dataset.contentTitle;
    if (!contentTitle) throw "Not set content type value.";

    return {
        key: contentKey,
        type: contentType,
        title: contentTitle,
        container: container,
        inPage: contentInPage(container)
    };
};

const contentInPage = (container: HTMLElement) => {
    return !!container.closest("#page-content");
};

const findContents = (): Array <ContentLocation> => {
    const definitions = new Array<ContentLocation>();

    const contentRoots = DOM.queryElements(document.body, "[data-content-root]");
    contentRoots.forEach(container => definitions.push(createContentLocation(container)));

    return definitions;
}

const findContent = (contentKey: string): ContentLocation => {
    const contentElem = DOM.queryElement(document.body, `[data-content-root='${contentKey}']`);
    if (!contentElem)
        return null;

    return createContentLocation(contentElem);
};

const findEditContent = (editId: string): ContentLocation => {
    var contentElem = DOM.queryElement(document.body, "[data-content-edit-id]");
    if (contentElem && contentElem.dataset.contentEditId != editId)
        throw "";

    return contentElem ? createContentLocation(contentElem) : null;
};

export interface ContentLocation {
    key: string;
    title: string;
    type: string;
    container: HTMLElement;
    inPage: boolean;
}