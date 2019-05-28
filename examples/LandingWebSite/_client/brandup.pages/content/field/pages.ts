import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import { DOM, ajaxRequest } from "brandup-ui";
import "./pages.less";

export class PagesContent extends Field<PagesFieldFormValue, PagesFieldFormOptions> implements IContentField {
    readonly form: IContentForm;
    private inputElem: HTMLInputElement;
    private valueElem: HTMLElement;
    private searchElem: HTMLElement;
    private __searchTimeout: number;
    private __searchRequest: XMLHttpRequest;
    private __closeMenuFunc: (e: MouseEvent) => void;

    constructor(form: IContentForm, name: string, options: PagesFieldFormOptions) {
        super(name, options);

        this.form = form;
    }

    get typeName(): string { return "BrandUpPages.Form.Field.Pages"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("pages");

        this.element.appendChild(this.inputElem = <HTMLInputElement>DOM.tag("input", { type: "text" }));
        this.element.appendChild(this.valueElem = DOM.tag("div", { class: "value", "data-command": "begin-input" }));
        this.element.appendChild(DOM.tag("div", { class: "placeholder", "data-command": "begin-input" }, this.options.placeholder));
        this.element.appendChild(this.searchElem = DOM.tag("ul", { class: "pages-menu" }));

        this.inputElem.addEventListener("keyup", () => {
            var title = this.inputElem.value;
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
                    success: (data: Array<PageCollectionModel>, status: number) => {
                        switch (status) {
                            case 200:
                                DOM.empty(this.searchElem);

                                if (data.length) {
                                    for (let i = 0; i < data.length; i++) {
                                        let collection = data[i];

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
            let t = <Element>e.target;
            if (!t.closest(".pages")) {
                this.element.classList.remove("inputing");
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
            }
        };

        this.registerCommand("begin-input", () => {
            if (!this.element.classList.toggle("inputing")) {
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
                return;
            }

            this.element.classList.add("inputing");

            DOM.empty(this.searchElem);
            this.searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название коллекции страниц."));

            this.inputElem.focus();
            this.inputElem.select();

            document.body.addEventListener("mousedown", this.__closeMenuFunc, false);
        });

        this.registerCommand("select", (elem: HTMLElement) => {
            this.element.classList.remove("inputing");
            document.body.removeEventListener("click", this.__closeMenuFunc, false);

            let pageCollectionId = elem.getAttribute("data-value");
            let pageUrl = elem.getAttribute("data-url");

            this.setValue({
                id: pageCollectionId,
                title: elem.innerText,
                pageUrl: pageUrl
            });

            this.form.request(this, {
                url: `/brandup.pages/content/pages`,
                urlParams: { pageCollectionId: pageCollectionId },
                method: "POST",
                success: (data: PagesFieldFormValue, status: number) => {
                    switch (status) {
                        case 200:
                            this.setValue(data);

                            break;
                        default:
                            throw "";
                    }
                }
            });
        });

        this.__refreshUI();
    }

    getValue(): PagesFieldFormValue { throw new Error("Method not implemented."); }
    setValue(value: PagesFieldFormValue) {
        if (!value) {
            this.inputElem.removeAttribute("value-collection-id");
            this.inputElem.value = "";
            this.valueElem.innerText = "";
        }
        else {
            this.inputElem.setAttribute("value-collection-id", value.id);
            this.inputElem.value = value.title;
            this.valueElem.innerText = value.title + ": " + value.pageUrl;
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