import { CommandContext, UIElement } from "@brandup/ui";
import { DOM } from "@brandup/ui-dom";
import { HyperLinkFieldFormOptions } from "../hyperlink";
import iconArrow from "../../../svg/combobox-arrow.svg";
import { IFieldValueElement } from "../../../typings/content";
import { request, AjaxResponse } from "@brandup/ui-ajax";
import { PageModel } from "../../../typings/page";
import { HyperLinkValue } from "../../../content/provider/hyperlink";
import "./styles/hyperlink.less"

interface IHyperlinkElements {
    typeElem?: HTMLElement;
    inputElem: HTMLElement;
    urlValueInput: HTMLInputElement;
    pageValueInput: HTMLInputElement;
    typeMenuElem: HTMLElement;
    searchElem: HTMLElement;
    placeholderElem: HTMLElement;
}

export class HyperlinkValue extends UIElement implements IFieldValueElement {
    private __elements: IHyperlinkElements;
    private __closeTypeMenuFunc: (e: MouseEvent) => void;
    private __closePageMenuFunc: (e: MouseEvent) => void;
    private __type: HyperLinkType;
    private __searchTimeout: number = 0;
    private __abortController?: AbortController;

    private __onChange: ((value: HyperLinkValue) => void) = () => {};

    get typeName(): string { return "BrandUpPages.Form.Field.Value.Hyperlink"; }

    constructor(options: HyperLinkFieldFormOptions) {
        super();

        this.__type = options?.valueType || "Page";

        this.__elements = this.__renderUI();
        this.__initLogic();

        this.__closeTypeMenuFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".hyperlink-menu") && this.element) {
                this.element?.classList.remove("opened-types");
                document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
            }
        };

        this.__closePageMenuFunc = (e: MouseEvent) => {
            const t = e.target as Element;
            if (!t.closest(".hyperlink") && this.element) {
                this.element?.classList.remove("inputing");
                this.element?.classList.remove("opened-pages");
                document.body.removeEventListener("click", this.__closePageMenuFunc, false);
            }
        };
    }

    private __renderUI(): IHyperlinkElements {
        const inputElem = DOM.tag("div", { class: "input-value", "data-command": "begin-input" });
        const placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "begin-input" });
        const urlValueInput = DOM.tag("input", { type: "text", class: "url" }) as HTMLInputElement;
        const pageValueInput = DOM.tag("input", { type: "text", class: "page" }) as HTMLInputElement;
        const typeMenuElem = DOM.tag("ul", { class: "hyperlink-menu types" }, [
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Page" }, "Page")),
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Url" }, "Url"))
        ]);
        const searchElem = DOM.tag("ul", { class: "hyperlink-menu pages" });

        urlValueInput.addEventListener("blur", () => {
            this.element?.classList.remove("inputing");
            if (this.__elements.inputElem)
            this.__elements.inputElem.innerText = this.__elements.urlValueInput.value;
        });
        
        const valueElem = DOM.tag("div", { class: "form-field_value hyperlink" });
        const typeElem = DOM.tag("div");
        valueElem.appendChild(DOM.tag("div", { class: "value-type", "data-command": "open-types-menu" }, [
            typeElem,
            iconArrow
        ]));
        valueElem.appendChild(inputElem);
        valueElem.appendChild(placeholderElem);
        valueElem.appendChild(urlValueInput);
        valueElem.appendChild(pageValueInput);

        valueElem.appendChild(typeMenuElem);

        valueElem.appendChild(searchElem);

        this.setElement(valueElem);

        return { inputElem, placeholderElem, urlValueInput, pageValueInput, typeMenuElem, searchElem, typeElem };
    }

    private __initLogic() {
        this.registerCommand("open-types-menu", () => {
            if (this.element?.classList.contains("opened-types")) {
                this.element?.classList.remove("opened-types");
                document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
                return;
            }

            if (this.element?.classList.contains("opened-pages")) {
                this.element?.classList.remove("inputing");
                this.element?.classList.remove("opened-pages");
                document.body.removeEventListener("click", this.__closePageMenuFunc, false);
            }

            this.element?.classList.add("opened-types")
            document.body.addEventListener("mousedown", this.__closeTypeMenuFunc, false);
        });

        this.registerCommand("begin-input", () => {
            this.element?.classList.add("inputing");

            switch (this.__type) {
                case "Page":
                    if (!this.element?.classList.toggle("opened-pages")) {
                        document.body.removeEventListener("click", this.__closePageMenuFunc, false);
                        return;
                    }

                    if (!this.__elements.searchElem) break;

                    DOM.empty(this.__elements.searchElem);
                    this.__elements.searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название страницы или её url."));

                    this.__elements.pageValueInput?.focus();
                    this.__elements.pageValueInput?.select();

                    document.body.addEventListener("mousedown", this.__closePageMenuFunc, false);
                    break;
                case "Url":
                    this.__elements.urlValueInput?.focus();
                    break;
                default:
                    throw new Error("");
            }
        });

        this.registerCommand("select-type", (context: CommandContext) => {
            const type = context.target.getAttribute("data-value") as HyperLinkType | null;
            if (!type) throw new Error("can not find attribute data-value");

            this.element?.classList.remove("opened-types");
            document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);

            this.__type = type;
            this.refreshUI();
        });

        this.registerCommand("select-page", (context: CommandContext) => {
            this.element?.classList.remove("inputing");
            this.element?.classList.remove("opened-pages");
            document.body.removeEventListener("click", this.__closePageMenuFunc, false);

            const pageId = context.target.getAttribute("data-value");
            if (!pageId) throw new Error("pageId not found");
            this.__elements.pageValueInput?.setAttribute("value-page-id", pageId);
            this.__elements.inputElem.innerText = context.target.innerText;
            this.__elements.pageValueInput.value = context.target.innerText;

            this.refreshUI();

            if (this.__onChange)
                this.__onChange({value: pageId, valueType: "Page"});
        });

        this.__elements.urlValueInput?.addEventListener("change", () => {
            this.refreshUI();

            if (this.__onChange)
                this.__onChange({value: this.__elements.urlValueInput?.value || "", valueType: "Url"});
        });

        this.__elements.pageValueInput?.addEventListener("keyup", () => {
            const title = this.__elements.pageValueInput?.value;
            if (!title || title.length < 3)
                return;

            if (this.__searchTimeout)
                clearTimeout(this.__searchTimeout);

            if (this.__abortController) 
                this.__abortController.abort();

            this.__searchTimeout = window.setTimeout(() => {
                this.__abortController = new AbortController();
                request({
                    url: `/brandup.pages/page/search`,
                    query: {
                        title: title
                    },
                    method: "GET",
                }, this.__abortController.signal)
                .then((response: AjaxResponse<Array<PageModel>>) => {
                    switch (response.status) {
                        case 200:
                            if (!this.__elements.searchElem) throw new Error("");
                            DOM.empty(this.__elements.searchElem);
    
                            if (response.data?.length) {
                                for (let i = 0; i < response.data.length; i++) {
                                    const page = response.data[i];
    
                                    this.__elements.searchElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-page", "data-value": page.id }, page.title)));
                                }
                            }
                            else
                                this.__elements.searchElem.appendChild(DOM.tag("li", { class: "text" }, "Страниц не найдено"));
    
                            break;
                        default:
                            throw new Error("");
                    }
                });
            }, 500);
        });
    }

    onChange(handler: (value: HyperLinkValue) => void) {
        this.__onChange = handler;
    }

    getValue(): HyperLinkValue { 
        switch (this.__type) {
            case "Page": {
                return {
                    valueType: this.__type,
                    value: this.__elements.pageValueInput?.value,
                    pageTitle: this.__elements.pageValueInput?.value
                };
            }
            case "Url": {
                return {
                    valueType: this.__type,
                    value: this.__elements.urlValueInput?.value,
                };
            }
            default:
                throw new Error("");
        }
     }
    

    hasValue(): boolean {
        switch (this.__type) {
            case "Page": {
                return Boolean(this.__elements.pageValueInput?.hasAttribute("value-page-id"));
            }
            case "Url": {
                return this.__elements.urlValueInput?.value ? true : false;
            }
            default:
                throw new Error("");
        }
    }

    setValue(value: HyperLinkValue) {
        if (value) {
            this.__type = value.valueType;
            
            switch (value.valueType) {
                case "Page": {
                    this.__elements.pageValueInput?.setAttribute("value-page-id", value.value);
                    if (value.pageTitle) {
                        this.__elements.inputElem.innerText = value.pageTitle;
                        this.__elements.pageValueInput.value = value.pageTitle;
                    }
                    break;
                }
                case "Url": {
                    this.__elements.urlValueInput.value = value.value;
                    this.__elements.inputElem.innerText = value.value;
                    break;
                }
                default:
                    throw new Error("");
            }
        }

        this.refreshUI();
    }

    refreshUI() {
        if (this.hasValue())
            this.element?.classList.add("has-value");
        else
            this.element?.classList.remove("has-value");

        if (this.__elements.typeElem)
            this.__elements.typeElem.innerText = this.__type;

        switch (this.__type) {
            case "Page": {
                this.element?.classList.remove("url-value");
                this.element?.classList.add("page-value");
                this.__elements.inputElem.innerText = this.__elements.pageValueInput.value;
                this.__elements.placeholderElem.innerText = "Выберите страницу";
                break;
            }
            case "Url": {
                this.element?.classList.remove("page-value");
                this.element?.classList.add("url-value");
                this.__elements.inputElem.innerText = this.__elements.urlValueInput.value;
                this.__elements.placeholderElem.innerText = "Введите url";
                break;
            }
            default:
                throw new Error("");
        }
    }

    destroy() {
        if (this.__abortController)
            this.__abortController.abort();

        window.clearTimeout(this.__searchTimeout || undefined);
        document.body.removeEventListener("click", this.__closeTypeMenuFunc, false);
        document.body.removeEventListener("click", this.__closePageMenuFunc, false);

        super.destroy();
    }
}

export type HyperLinkType = "Url" | "Page";