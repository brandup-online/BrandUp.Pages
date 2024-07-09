import { ajaxRequest, AjaxResponse } from "brandup-ui-ajax";
import { PageCollectionModel } from "../../typings/page";
import "./pages.less";
import { DOM } from "brandup-ui-dom";
import { FormField } from "./base";

export class PagesContent extends FormField<PagesFieldFormOptions> {
    private inputElem: HTMLInputElement;
    private __pagesValueElem: HTMLElement;
    private searchElem: HTMLElement;
    private __searchTimeout: number;
    private __searchRequest: XMLHttpRequest;
    private __closeMenuFunc: (e: MouseEvent) => void;
    
    get typeName(): string { return "BrandUpPages.Form.Field.Pages"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("pages");
    }

    protected _renderValueElem() {
        const valueElem = DOM.tag("div", {class: "value"}, [
            this.inputElem = DOM.tag("input", { type: "text" }) as HTMLInputElement,
            this.__pagesValueElem = DOM.tag("div", { class: "pages-value", "data-command": "begin-input" }),
            DOM.tag("div", { class: "placeholder", "data-command": "begin-input" }, this.options.placeholder),
            this.searchElem = DOM.tag("ul", { class: "pages-menu" })
        ])

        this.inputElem.addEventListener("keyup", () => {
            const title = this.inputElem.value;
            if (!title || title.length < 3)
                return;

            if (this.__searchTimeout)
                clearTimeout(this.__searchTimeout);

            if (this.__searchRequest)
                this.__searchRequest.abort();

            this.__searchTimeout = window.setTimeout(() => {
                this.__searchRequest = ajaxRequest({
                    url: `/brandup.pages/collection/search`,
                    urlParams: {
                        pageType: this.options.pageType,
                        title: title
                    },
                    method: "GET",
                    success: (response: AjaxResponse<Array<PageCollectionModel>>) => {
                        switch (response.status) {
                            case 200:
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
                                throw "";
                        }
                    }
                });
            }, 500);
        });

        this.__closeMenuFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".pages") && valueElem) {
                valueElem.classList.remove("inputing");
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
            }
        };

        this.registerCommand("begin-input", () => {
            if (!valueElem.classList.toggle("inputing")) {
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
                return;
            }

            valueElem.classList.add("inputing");

            DOM.empty(this.searchElem);
            this.searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название коллекции страниц."));

            this.inputElem.focus();
            this.inputElem.select();

            document.body.addEventListener("mousedown", this.__closeMenuFunc, false);
        });

        this.registerCommand("select", (elem: HTMLElement) => {
            valueElem.classList.remove("inputing");
            document.body.removeEventListener("click", this.__closeMenuFunc, false);

            const pageCollectionId = elem.getAttribute("data-value");
            const pageUrl = elem.getAttribute("data-url");

            this._setValue({
                id: pageCollectionId,
                title: elem.innerText,
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
            //                 throw "";
            //         }
            //     }
            // });
        });

        this.__refreshUI();
        return valueElem;
    }

    getValue(): PagesFieldFormValue { throw new Error("Method not implemented."); }
    protected _setValue(value: PagesFieldFormValue) {
        if (!value) {
            this.inputElem.removeAttribute("value-collection-id");
            this.inputElem.value = "";
            this.__pagesValueElem.innerText = "";
        }
        else {
            this.inputElem.setAttribute("value-collection-id", value.id);
            this.inputElem.value = value.title;
            this.__pagesValueElem.innerText = value.title + ": " + value.pageUrl;
        }

        this.__refreshUI();
    }
    hasValue(): boolean {
        return this.inputElem.hasAttribute("value-collection-id");
    }

    private __refreshUI() {
        if (this.hasValue())
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");
    }

    destroy() {
        if (this.__searchRequest)
            this.__searchRequest.abort();

        window.clearTimeout(this.__searchTimeout);
        document.body.removeEventListener("click", this.__closeMenuFunc, false);

        super.destroy();
    }
}

export interface PagesFieldFormOptions {
    placeholder: string;
    pageType: string;
}

export interface PagesFieldFormValue {
    id: string;
    title: string;
    pageUrl: string;
}