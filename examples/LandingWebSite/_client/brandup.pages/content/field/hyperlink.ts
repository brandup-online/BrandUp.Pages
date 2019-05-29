import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import { DOM, ajaxRequest } from "brandup-ui";
import iconArrow from "../../svg/combobox-arrow.svg";
import "./hyperlink.less";

export class HyperLinkContent extends Field<HyperLinkFieldFormValue, HyperLinkFieldFormOptions> implements IContentField {
    readonly form: IContentForm;
    private __typeElem: HTMLElement;
    private __valueElem: HTMLElement;
    private __urlValueInput: HTMLInputElement;
    private __pageValueInput: HTMLInputElement;
    private __typeMenuElem: HTMLElement;
    private __searchElem: HTMLElement;
    private __placeholderElem: HTMLElement;
    private __closeTypeMenuFunc: (e: MouseEvent) => void;
    private __closePageMenuFunc: (e: MouseEvent) => void;
    private __type: HyperLinkType = "Page";
    private __searchTimeout: number;
    private __searchRequest: XMLHttpRequest;

    constructor(form: IContentForm, name: string, options: HyperLinkFieldFormOptions) {
        super(name, options);

        this.form = form;
    }

    get typeName(): string { return "BrandUpPages.Form.Field.HyperLink"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("hyperlink");
        
        this.element.appendChild(DOM.tag("div", { class: "value-type", "data-command": "open-types-menu" }, [
            this.__typeElem = DOM.tag("span", null, "Page"),
            iconArrow
        ]));
        this.element.appendChild(this.__valueElem = DOM.tag("div", { class: "value", "data-command": "begin-input" }));
        this.element.appendChild(this.__placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "begin-input" }));

        this.__urlValueInput = <HTMLInputElement>DOM.tag("input", { type: "text", class: "url" });
        this.__urlValueInput.addEventListener("change", () => {
            this.__refreshUI();

            this.form.request(this, {
                url: `/brandup.pages/content/hyperlink/url`,
                urlParams: {
                    url: this.__urlValueInput.value
                },
                method: "POST",
                success: (data: HyperLinkFieldFormValue, status: number) => {
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
        this.__urlValueInput.addEventListener("blur", () => {
            this.element.classList.remove("inputing");
            this.__valueElem.innerText = this.__urlValueInput.value;
        });
        this.element.appendChild(this.__urlValueInput);

        this.__pageValueInput = <HTMLInputElement>DOM.tag("input", { type: "text", class: "page" });
        this.__pageValueInput.addEventListener("keyup", () => {
            var title = this.__pageValueInput.value;
            if (!title || title.length < 3)
                return;

            if (this.__searchTimeout)
                clearTimeout(this.__searchTimeout);

            if (this.__searchRequest)
                this.__searchRequest.abort();

            this.__searchTimeout = window.setTimeout(() => {
                this.__searchRequest = ajaxRequest({
                    url: `/brandup.pages/page/search`,
                    urlParams: {
                        title: title
                    },
                    method: "GET",
                    success: (data: Array<PageModel>, status: number) => {
                        switch (status) {
                            case 200:
                                DOM.empty(this.__searchElem);

                                if (data.length) {
                                    for (let i = 0; i < data.length; i++) {
                                        let page = data[i];

                                        this.__searchElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-page", "data-value": page.id }, page.title)));
                                    }
                                }
                                else
                                    this.__searchElem.appendChild(DOM.tag("li", { class: "text" }, "Страниц не найдено"));

                                break;
                            default:
                                throw "";
                        }
                    }
                });
            }, 500);
        });
        this.element.appendChild(this.__pageValueInput);
        
        this.__typeMenuElem = DOM.tag("ul", { class: "hyperlink-menu types" }, [
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Page" }, "Page")),
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Url" }, "Url"))
        ]);
        this.element.appendChild(this.__typeMenuElem);

        this.__searchElem = DOM.tag("ul", { class: "hyperlink-menu pages" });
        this.element.appendChild(this.__searchElem);

        this.__closeTypeMenuFunc = (e: MouseEvent) => {
            let t = <Element>e.target;
            if (!t.closest(".hyperlink-menu") && this.element) {
                this.element.classList.remove("opened-types");
                document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
            }
        };
        this.__closePageMenuFunc = (e: MouseEvent) => {
            let t = <Element>e.target;
            if (!t.closest(".hyperlink") && this.element) {
                this.element.classList.remove("inputing");
                this.element.classList.remove("opened-pages");
                document.body.removeEventListener("click", this.__closePageMenuFunc, false);
            }
        };

        this.registerCommand("open-types-menu", () => {
            if (this.element.classList.contains("opened-types")) {
                this.element.classList.remove("opened-types");
                document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
                return;
            }

            this.element.classList.add("opened-types")
            document.body.addEventListener("mousedown", this.__closeTypeMenuFunc, false);
        });
        this.registerCommand("begin-input", (elem: HTMLElement) => {
            this.element.classList.add("inputing");

            switch (this.__type) {
                case "Page":
                    if (!this.element.classList.toggle("opened-pages")) {
                        document.body.removeEventListener("click", this.__closePageMenuFunc, false);
                        return;
                    }

                    DOM.empty(this.__searchElem);
                    this.__searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название страницы или её url."));

                    this.__pageValueInput.focus();
                    this.__pageValueInput.select();

                    document.body.addEventListener("mousedown", this.__closePageMenuFunc, false);
                    break;
                case "Url":
                    this.__urlValueInput.focus();
                    break;
                default:
                    throw "";
            }
        });
        this.registerCommand("select-type", (elem: HTMLElement) => {
            let type = <HyperLinkType>elem.getAttribute("data-value");
            
            this.element.classList.remove("opened-types");
            document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);

            this.__type = type;
            this.__refreshUI();
        });
        this.registerCommand("select-page", (elem: HTMLElement) => {
            this.element.classList.remove("inputing");
            this.element.classList.remove("opened-pages");
            document.body.removeEventListener("click", this.__closePageMenuFunc, false);

            let pageId = elem.getAttribute("data-value");
            this.__pageValueInput.setAttribute("value-page-id", pageId);
            this.__valueElem.innerText = elem.innerText;
            this.__pageValueInput.value = elem.innerText;

            this.__refreshUI();

            this.form.request(this, {
                url: `/brandup.pages/content/hyperlink/page`,
                urlParams: {
                    pageId: pageId
                },
                method: "POST",
                success: (data: HyperLinkFieldFormValue, status: number) => {
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
    
    getValue(): HyperLinkFieldFormValue { throw "Not implemented"; }
    setValue(value: HyperLinkFieldFormValue) {
        if (value) {
            this.__type = value.valueType;

            switch (value.valueType) {
                case "Page": {
                    this.__pageValueInput.setAttribute("value-page-id", value.value);
                    this.__valueElem.innerText = value.pageTitle;
                    this.__pageValueInput.value = value.pageTitle;
                    break;
                }
                case "Url": {
                    this.__urlValueInput.value = value.value;
                    this.__valueElem.innerText = value.value;
                    break;
                }
                default:
                    throw "";
            }
        }

        this.__refreshUI();
    }
    hasValue(): boolean {
        switch (this.__type) {
            case "Page": {
                return this.__pageValueInput.hasAttribute("value-page-id");
            }
            case "Url": {
                return this.__urlValueInput.value ? true : false;
            }
            default:
                throw "";
        }
    }

    private __refreshUI() {
        if (this.hasValue())
            this.element.classList.add("has-value");
        else
            this.element.classList.remove("has-value");

        this.__typeElem.innerText = this.__type;

        switch (this.__type) {
            case "Page": {
                this.element.classList.remove("url-value");
                this.element.classList.add("page-value");
                this.__valueElem.innerText = this.__pageValueInput.value;
                this.__placeholderElem.innerText = "Выберите страницу";
                break;
            }
            case "Url": {
                this.element.classList.remove("page-value");
                this.element.classList.add("url-value");
                this.__valueElem.innerText = this.__urlValueInput.value;
                this.__placeholderElem.innerText = "Введите url";
                break;
            }
            default:
                throw "";
        }
    }

    destroy() {
        if (this.__searchRequest)
            this.__searchRequest.abort();

        window.clearTimeout(this.__searchTimeout);
        document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
        document.body.removeEventListener("click", this.__closePageMenuFunc, false);

        super.destroy();
    }
}

export type HyperLinkType = "Url" | "Page";

export interface HyperLinkFieldFormValue {
    valueType: HyperLinkType;
    value: string;
    pageTitle?: string;
}

export interface HyperLinkFieldFormOptions {
    valueType: "Url" | "Page";
    value: string;
}