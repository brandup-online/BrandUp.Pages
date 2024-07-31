import { CommandContext, UIElement } from "@brandup/ui";
import { DOM } from "@brandup/ui-dom";
import { PagesFieldFormOptions } from "../pages";
import { IFieldValueElement } from "../../../typings/content";
import { AjaxResponse, request } from "@brandup/ui-ajax";
import { PageCollectionModel } from "../../../typings/page";

export class PageValue extends UIElement implements IFieldValueElement {
    private __inputElem: HTMLInputElement;
    private __pagesValueElem: HTMLElement;
    private __searchElem: HTMLElement;
    private __closeMenuFunc: (e: MouseEvent) => void;
    private __searchTimeout: number = 0;
    private __abortController?: AbortController;

    private __onChange: (value: string) => void = () => {};

    readonly options: PagesFieldFormOptions;

    get typeName(): string { return "BrandUpPages.Form.Field.Value.Page"; }

    get inputElem() { return this.__inputElem };
    get searchElem() { return this.__searchElem };

    constructor(options: PagesFieldFormOptions) {
        super();

        this.options = options;
        
        const valueElem = DOM.tag("div", {class: "form-field_value pages"}, [
            this.__inputElem = DOM.tag("input", { type: "text" }) as HTMLInputElement,
            this.__pagesValueElem = DOM.tag("div", { class: "pages-value", "data-command": "begin-input" }),
            DOM.tag("div", { class: "placeholder", "data-command": "begin-input" }, options.placeholder),
            this.__searchElem = DOM.tag("ul", { class: "pages-menu" })
        ])

        this.setElement(valueElem);
        this.__refreshUI();

        this.__initLogic();

        this.__closeMenuFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".pages") && this.element) {
                this.element?.classList.remove("inputing");
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
            }
        };
    }

    private __initLogic() {
        this.registerCommand("begin-input", () => {
            if (!this.element?.classList.toggle("inputing")) {
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
                return;
            }

            this.element?.classList.add("inputing");

            DOM.empty(this.__searchElem);
            this.__searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название коллекции страниц."));

            this.__inputElem.focus();
            this.__inputElem.select();

            document.body.addEventListener("mousedown", this.__closeMenuFunc, false);
        });

        this.registerCommand("select", (context: CommandContext) => {
            this.element?.classList.remove("inputing");
            document.body.removeEventListener("click", this.__closeMenuFunc, false);

            const pageCollectionId = context.target.getAttribute("data-value");
            const pageUrl = context.target.getAttribute("data-url");

            if (!pageCollectionId || !pageUrl) return;

            this.setValue({
                id: pageCollectionId,
                title: context.target.innerText,
                pageUrl: pageUrl
            });

            // this.form.request(this, {
            //     url: `/brandup.pages/content/pages`,
            //     urlParams: { pageCollectionId: pageCollectionId },
            //     method: "POST",
            //     success: (response: AjaxResponse<PagesFieldFormValue>) => {
            //         switch (response.status) {
            //             case 200:
            //                 this.setValue(response.data);

            //                 break;
            //             default:
            //                 throw new Error("";
            //         }
            //     }
            // });
        });

        this.inputElem.addEventListener("keyup", () => {
            const title = this.inputElem.value;
            if (!title || title.length < 3)
                return;

            if (this.__searchTimeout)
                clearTimeout(this.__searchTimeout);

            if (this.__abortController) 
                this.__abortController.abort();

            this.__searchTimeout = window.setTimeout(() => {
                request({
                    url: `/brandup.pages/collection/search`,
                    query: {
                        pageType: this.options.pageType,
                        title: title
                    },
                    method: "GET",
                }, this.__abortController?.signal)
                .then((response: AjaxResponse<Array<PageCollectionModel>>) => {
                    switch (response.status) {
                        case 200:
                            if (response.data === undefined || response.data === null) break;
    
                            DOM.empty(this.searchElem);
    
                            if (response.data.length) {
                                for (let i = 0; i < response.data.length; i++) {
                                    const collection = response.data[i];
    
                                    this.searchElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select", "data-value": collection.id, "data-url": collection.pageUrl }, collection.title + ": " + collection.pageUrl)));
                                }
                            }
                            else {
                                this.searchElem.appendChild(DOM.tag("li", { class: "text" }, "Коллекций страниц не найдено"));
                            }
    
                            break;
                        default:
                            throw new Error("");
                    }
                });
            }, 500);
        });
    }

    hasValue(): boolean {
        return this.__inputElem.hasAttribute("value-collection-id");
    }

    getValue(): any {
        throw new Error("getValue method not implemented")
    }

    setValue(value: PagesFieldFormValue) {
        if (!value) {
            this.__inputElem.removeAttribute("value-collection-id");
            this.__inputElem.value = "";
            this.__pagesValueElem.innerText = "";
        }
        else {
            this.__inputElem.setAttribute("value-collection-id", value.id);
            this.__inputElem.value = value.title;
            this.__pagesValueElem.innerText = value.title + ": " + value.pageUrl;
        }

        this.__refreshUI();
    }

    private __refreshUI() {
        if (this.hasValue())
            this.element?.classList.add("has-value");
        else
            this.element?.classList.remove("has-value");
    }

    onChange(handler: (file: File | string) => void) {
        this.__onChange = handler;
    }

    destroy() {
        if (this.__abortController)
            this.__abortController.abort();

        window.clearTimeout(this.__searchTimeout);
        document.body.removeEventListener("click", this.__closeMenuFunc, false);

        super.destroy();
    }
}

export interface PagesFieldFormValue {
    id: string;
    title: string;
    pageUrl: string;
}