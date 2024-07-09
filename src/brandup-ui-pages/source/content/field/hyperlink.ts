import { ajaxRequest, AjaxResponse } from "brandup-ui-ajax";
import iconArrow from "../../svg/combobox-arrow.svg";
import { PageModel } from "../../typings/page";
import "./hyperlink.less";
import { DOM } from "brandup-ui-dom";
import { FormField } from "./base";

export class HyperLinkContent extends FormField<HyperLinkFieldFormOptions> {
    private __typeElem: HTMLElement;
    private __inputElem: HTMLElement;
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

    get typeName(): string { return "BrandUpPages.Form.Field.HyperLink"; }

    render(ownElem: HTMLElement): void {
        super.render(ownElem);
        this.element.classList.add("hyperlink");
    }

    protected _renderValueElem() {
        const valueElem = DOM.tag("div", { class: "value" });

        valueElem.appendChild(DOM.tag("div", { class: "value-type", "data-command": "open-types-menu" }, [
            this.__typeElem = DOM.tag("span", null, "Page"),
            iconArrow
        ]));
        valueElem.appendChild(this.__inputElem = DOM.tag("div", { class: "input-value", "data-command": "begin-input" }));
        valueElem.appendChild(this.__placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "begin-input" }));

        this.__urlValueInput = DOM.tag("input", { type: "text", class: "url" }) as HTMLInputElement;
        this.__urlValueInput.addEventListener("change", () => {
            this.__refreshUI();

            this.provider.saveValue(this.__urlValueInput.value);
        });
        this.__urlValueInput.addEventListener("blur", () => {
            valueElem.classList.remove("inputing");
            this.__inputElem.innerText = this.__urlValueInput.value;
        });
        valueElem.appendChild(this.__urlValueInput);

        this.__pageValueInput = DOM.tag("input", { type: "text", class: "page" }) as HTMLInputElement;
        this.__pageValueInput.addEventListener("keyup", () => {
            const title = this.__pageValueInput.value;
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
                    success: (response: AjaxResponse<Array<PageModel>>) => {
                        switch (response.status) {
                            case 200:
                                DOM.empty(this.__searchElem);

                                if (response.data.length) {
                                    for (let i = 0; i < response.data.length; i++) {
                                        const page = response.data[i];

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
        valueElem.appendChild(this.__pageValueInput);

        this.__typeMenuElem = DOM.tag("ul", { class: "hyperlink-menu types" }, [
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Page" }, "Page")),
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Url" }, "Url"))
        ]);
        valueElem.appendChild(this.__typeMenuElem);

        this.__searchElem = DOM.tag("ul", { class: "hyperlink-menu pages" });
        valueElem.appendChild(this.__searchElem);

        this.__closeTypeMenuFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".hyperlink-menu") && valueElem) {
                valueElem.classList.remove("opened-types");
                document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
            }
        };
        this.__closePageMenuFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".hyperlink") && valueElem) {
                valueElem.classList.remove("inputing");
                valueElem.classList.remove("opened-pages");
                document.body.removeEventListener("click", this.__closePageMenuFunc, false);
            }
        };

        this.registerCommand("open-types-menu", () => {
            if (valueElem.classList.contains("opened-types")) {
                valueElem.classList.remove("opened-types");
                document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
                return;
            }

            if (valueElem.classList.contains("opened-pages")) {
                valueElem.classList.remove("inputing");
                valueElem.classList.remove("opened-pages");
                document.body.removeEventListener("click", this.__closePageMenuFunc, false);
            }

            valueElem.classList.add("opened-types")
            document.body.addEventListener("mousedown", this.__closeTypeMenuFunc, false);
        });
        this.registerCommand("begin-input", () => {
            valueElem.classList.add("inputing");

            switch (this.__type) {
                case "Page":
                    if (!valueElem.classList.toggle("opened-pages")) {
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
            const type = elem.getAttribute("data-value") as HyperLinkType;

            valueElem.classList.remove("opened-types");
            document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);

            this.__type = type;
            this.__refreshUI();
        });
        this.registerCommand("select-page", (elem: HTMLElement) => {
            valueElem.classList.remove("inputing");
            valueElem.classList.remove("opened-pages");
            document.body.removeEventListener("click", this.__closePageMenuFunc, false);

            const pageId = elem.getAttribute("data-value");
            this.__pageValueInput.setAttribute("value-page-id", pageId);
            this.__inputElem.innerText = elem.innerText;
            this.__pageValueInput.value = elem.innerText;

            this.__refreshUI();

            // this.provider.selectPage(pageId);
        });
        return valueElem;
    }

    getValue(): HyperLinkFieldFormValue { throw "Not implemented"; }
    protected _setValue(value: HyperLinkFieldFormValue) {
        if (value) {
            this.__type = value.valueType;

            switch (value.valueType) {
                case "Page": {
                    this.__pageValueInput.setAttribute("value-page-id", value.value);
                    this.__inputElem.innerText = value.pageTitle;
                    this.__pageValueInput.value = value.pageTitle;
                    break;
                }
                case "Url": {
                    this.__urlValueInput.value = value.value;
                    this.__inputElem.innerText = value.value;
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
            this.__valueElem.classList.add("has-value");
        else
            this.__valueElem.classList.remove("has-value");

        this.__typeElem.innerText = this.__type;

        switch (this.__type) {
            case "Page": {
                this.__valueElem.classList.remove("url-value");
                this.__valueElem.classList.add("page-value");
                this.__inputElem.innerText = this.__pageValueInput.value;
                this.__placeholderElem.innerText = "Выберите страницу";
                break;
            }
            case "Url": {
                this.__valueElem.classList.remove("page-value");
                this.__valueElem.classList.add("url-value");
                this.__inputElem.innerText = this.__urlValueInput.value;
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