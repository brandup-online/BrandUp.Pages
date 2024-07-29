import { CommandContext, UIElement } from "@brandup/ui";
import ContentPage from "../pages/content";
import { browserPage } from "../dialogs/pages/browser";
import iconBack from "../svg/toolbar-button-back.svg";
import iconDown from "../svg/new/arrow-down.svg";
import iconPublish from "../svg/new/upload.svg";
import iconSeo from "../svg/new/increase.svg";
import iconStar from "../svg/new/star.svg";
import { listContentType } from "../dialogs/content-types/list";
import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";
import { publishPage } from "../dialogs/pages/publish";
import { seoPage } from "../dialogs/pages/seo";
import { DOM } from "@brandup/ui-dom";
import { request, AjaxResponse } from "@brandup/ui-ajax";
import { ContentEditor } from "../content/editor";

import iconList from "../svg/new/menu.svg";
import iconEdit from "../svg/new/edit.svg";
import iconMore from "../svg/new/more.svg";
import iconPlus from "../svg/new/plus.svg";

const EDITID_QUERY_KEY = "editid";

export interface ILangguage {
    code: string,
    name: string;
    isMain: boolean;
    progress: number;
}

const languagesMock: ILangguage[] = [
    { code:"en", name: "Английский", isMain: false, progress: 50 },
    { code:"ru", name: "Русский", isMain: true, progress: 100 },
]

export class PageToolbar extends UIElement {
    private __page: Page<WebsiteApplication,PageModel>;
    private __isDestroyed: boolean = false;

    readonly isContentPage: boolean;

    private __menuCloseFunc?: ((e: MouseEvent) => void);
    private __menuExpandedElem?: HTMLElement;

    private __editor?: ContentEditor;
    
    get typeName(): string {
        return "BrandUpPages.PageToolbar";
    }
    get hasEditor(): boolean { return !!this.__editor; }

    constructor(page: Page<WebsiteApplication,PageModel>) {
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
                .catch((e) => console.error("Error in content editor.", e));
        }
        else
            this.__renderTollbar();
    }

    private __renderTollbar() {
        const leftToolbar = this.__renderLeftToolbar();
        const rightToolbar = this.__renderRightToolbar();

        document.body.classList.add("bp-state-toolbars");

        const toolbarElem = DOM.tag("div", { class: "bp-elem bp-page-toolbar" }, [leftToolbar, rightToolbar]);

        document.body.appendChild(toolbarElem);
        this.setElement(toolbarElem);

        this.__initTollbarLogic();
    }

    private __renderLeftToolbar() {
        const leftToolbar = DOM.tag("div", { class: "toolbar-menu left" });

        let websiteMenu: HTMLElement;
        leftToolbar.appendChild(DOM.tag("div", { class: "separator-right" }, [
            DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Меню сайта" }, iconList),
            websiteMenu = DOM.tag("menu", { class: "bp-page-toolbar-menu" }),
        ]));

        const websiteMenuItems = document.createDocumentFragment();
        websiteMenuItems.append(DOM.tag("button", { command: "bp-content-types" }, [iconPlus, DOM.tag("span", null, "Типы контента")]));
        websiteMenuItems.append(DOM.tag("button", { command: "bp-pages" }, [iconList, DOM.tag("span", null, "Страницы этого уровня")]));

        // Если страница динамическая
        if (this.isContentPage) {
            const contentPage = <ContentPage>this.__page;

            websiteMenuItems.append(DOM.tag("button", { command: "bp-pages-child" }, [iconDown, DOM.tag("span", null, "Дочерние страницы")]));
            
            if (contentPage.model.parentPageId) {
                websiteMenuItems.append(DOM.tag("button", { command: "bp-back" }, [iconBack, DOM.tag("span", null, "Перейти к родительской странице")]));
            }
            
            const pageMenuItems = [DOM.tag("button", { command: "bp-seo" }, [iconSeo, DOM.tag("span", null, "Индексирование страницы")]) ];

            leftToolbar.appendChild(DOM.tag("div", null, [
                DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Действия над страницей" }, iconMore),
                DOM.tag("menu", { class: "bp-page-toolbar-menu", title: "" }, pageMenuItems),
            ]));

            leftToolbar.appendChild(DOM.tag("div", null, DOM.tag("button", { class: "bp-page-toolbar-button page-status " + contentPage.model.status.toLowerCase() }, [DOM.tag("span")])));

            if (contentPage.model.status !== "Published") {
                leftToolbar.appendChild(DOM.tag("div", null, DOM.tag("button", { class: "bp-page-toolbar-button", command: "bp-publish", title: "Опубликовать" }, iconPublish)));
                pageMenuItems.push(DOM.tag("button", { command: "bp-publish" }, [iconPublish, DOM.tag("span", null, "Опубликовать страницу")]));
            }
        }

        websiteMenu.append(websiteMenuItems);

        return leftToolbar;
    }

    private __renderRightToolbar() {
        const rightToolbar = DOM.tag("div", { class: "toolbar-menu right" });
        const contents = findContents();

        if (contents.length) {
            let contentsMenuElem: HTMLElement;
            rightToolbar.appendChild(DOM.tag("div", {class: "separator-right"}, [
                DOM.tag("button", { class: "bp-page-toolbar-button", command: "show-menu", title: "Редактировать контент страницы" }, iconEdit),
                contentsMenuElem = DOM.tag("menu", { class: "bp-page-toolbar-menu" })
            ]));

            contents.forEach(contentDefinition => {
                contentsMenuElem.appendChild(DOM.tag("button", { command: "bp-content-edit", dataset: { contentKey: contentDefinition.key, contentType: contentDefinition.type } }, [
                    DOM.tag('span', null, [contentDefinition.title, DOM.tag("i", null, contentDefinition.key)])
                ]));
            })
        }

        const languages = [
            ...languagesMock.map(lang => DOM.tag("button", {command: "set-language"},[
                DOM.tag("div", { class: "code" }, lang.code),
                DOM.tag("div", { class: "description" }, [
                    lang.name,
                    DOM.tag("span", null, lang.isMain ? "Основной язык" : `Прогресс ${lang.progress}%`)
                ]),
                lang.isMain ? iconStar : null
            ])),
            DOM.tag("button", {command: "add-language"}, [iconPlus, "Сменить"])
        ]

        rightToolbar.appendChild(DOM.tag("div", null, [
            DOM.tag("button", { class: "bp-page-toolbar-button language", command: "show-menu", title: "Сменить язык страницы" }, [
                DOM.tag("span", null, "en"), "50%"
            ]),
            DOM.tag("menu", { class: "bp-page-toolbar-menu language", title: "" }, languages),
        ]));

        return rightToolbar;
    }

    private __initTollbarLogic() {
        this.registerCommand("show-menu", (context) => {
            if (context.target.classList.toggle("expanded"))
                this.__openMenu(context.target);
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
            let parentPageId: string = "";
            if (this.isContentPage)
                parentPageId = this.__page.model.parentPageId;
            if (parentPageId) {
                request({
                    url: `/brandup.pages/page/${parentPageId}`,
                }, this.__page.context.abort).then((response) => this.__page.website.nav({ url: response.data.url }));
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
            let parentPageId: string = "";
            if (this.isContentPage)
                parentPageId = this.__page.model.parentPageId || "";

            browserPage(parentPageId);
        });

        this.registerCommand("bp-pages-child", () => {
            browserPage(this.__page.model.id);
        });

        this.registerCommand("bp-content-types", () => {
            this.element?.classList.remove("opened-menu");
            listContentType();
        });

        this.registerCommand("bp-seo", () => {
            seoPage(this.__page.model.id).then(() => {
                this.__page.website.reload();
            });
        });

        this.registerCommand("bp-content-edit", (context: CommandContext) => {
            this.__closeMenu();
            
            const contentKey = context.target.dataset.contentKey;
            const contentType = context.target.dataset.contentType;
            if (!contentKey || !contentType)
                throw new Error("Not set content edit parameters.");

            return ContentEditor.begin(this.__page, contentKey, contentType)
                .then(result => {
                    if (result.exist && !confirm("Continue editing?"))
                        return ContentEditor.begin(this.__page, contentKey, contentType, true);
                        
                    return result;
                })
                .then(result => {
                    const editUrl = this.__page.website.buildUrl(this.__page.nav.path, { editid: result.editId });

                    const contentLocation = findContent(contentKey)
                    if (!contentLocation || contentLocation.inPage)
                        this.__page.website.nav({ url: editUrl, replace: true });
                    else
                        window.location.replace(editUrl);
                })
                .catch(() => {
                    console.log(`Unable to begin edit content by key "${contentKey}".`);
                })
        });
    }
    
    private __openMenu(ownElem: HTMLElement) {
        if (!this.__menuCloseFunc) return;

        document.body.addEventListener("click", this.__menuCloseFunc);

        this.__menuExpandedElem = ownElem;
    }

    private __closeMenu() {
        if (!this.__menuCloseFunc) return;

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
    if (!contentKey) throw new Error("Not set content root value.");

    const contentType = container.dataset.contentType;
    if (!contentType) throw new Error("Not set content type value.");

    const contentTitle = container.dataset.contentTitle;
    if (!contentTitle) throw new Error("Not set content type value.");

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
        throw new Error("couldn't find the content");

    return createContentLocation(contentElem);
};

const findEditContent = (editId: string): ContentLocation => {
    var contentElem = DOM.queryElement(document.body, "[data-content-edit-id]");
    if (contentElem && contentElem.dataset.contentEditId != editId)
        throw new Error("");

    if (!contentElem)
        throw new Error("couldn't find the content");

    return createContentLocation(contentElem);
};

export interface ContentLocation {
    key: string;
    title: string;
    type: string;
    container: HTMLElement;
    inPage: boolean;
}