import { CommandContext, UIElement } from "@brandup/ui";
import { DOM } from "@brandup/ui-dom";
import { HyperLinkFieldFormOptions } from "../hyperlink";
import iconArrow from "../../../svg/combobox-arrow.svg";
import { IFieldValueElement } from "../../../typings/content";
import { request, AjaxResponse } from "@brandup/ui-ajax";
import { PageModel } from "../../../typings/page";
import { HyperLinkValue } from "../../../content/provider/hyperlink";

export class HyperlinkValue extends UIElement implements IFieldValueElement {
    private __typeElem: HTMLElement | null = null;
    private __inputElem: HTMLElement;
    private __urlValueInput: HTMLInputElement;
    private __pageValueInput: HTMLInputElement;
    private __typeMenuElem: HTMLElement;
    private __searchElem: HTMLElement;
    private __placeholderElem: HTMLElement;
    private __closeTypeMenuFunc: (e: MouseEvent) => void;
    private __closePageMenuFunc: (e: MouseEvent) => void;
    private __type: HyperLinkType;
    private __searchTimeout: number = 0;
    private __abortController: AbortController | null = null;

    private __onChange: ((value: HyperLinkValue) => void) = () => {};

    get typeName(): string { return "BrandUpPages.Form.Field.Value.Hyperlink"; }

    constructor(options: HyperLinkFieldFormOptions) {
        super();

        this.__type = options.valueType;

        this.__inputElem = DOM.tag("div", { class: "input-value", "data-command": "begin-input" });
        this.__placeholderElem = DOM.tag("div", { class: "placeholder", "data-command": "begin-input" });
        this.__urlValueInput = DOM.tag("input", { type: "text", class: "url" }) as HTMLInputElement;
        this.__pageValueInput = DOM.tag("input", { type: "text", class: "page" }) as HTMLInputElement;
        this.__typeMenuElem = DOM.tag("ul", { class: "hyperlink-menu types" }, [
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Page" }, "Page")),
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Url" }, "Url"))
        ]);
        this.__searchElem = DOM.tag("ul", { class: "hyperlink-menu pages" });

        this.__renderUI();
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

    private __renderUI() {
        this.__urlValueInput.addEventListener("blur", () => {
            this.element?.classList.remove("inputing");
            if (this.__inputElem)
            this.__inputElem.innerText = this.__urlValueInput!.value;
        });
        
        const valueElem = DOM.tag("div", { class: "value hyperlink" });
        valueElem.appendChild(DOM.tag("div", { class: "value-type", "data-command": "open-types-menu" }, [
            this.__typeElem,
            iconArrow
        ]));
        valueElem.appendChild(this.__inputElem);
        valueElem.appendChild(this.__placeholderElem);
        valueElem.appendChild(this.__urlValueInput);
        valueElem.appendChild(this.__pageValueInput);

        valueElem.appendChild(this.__typeMenuElem);

        valueElem.appendChild(this.__searchElem);

        this.setElement(valueElem);
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

                    if (!this.__searchElem) break;

                    DOM.empty(this.__searchElem);
                    this.__searchElem.appendChild(DOM.tag("li", { class: "text" }, "Начните вводить название страницы или её url."));

                    this.__pageValueInput?.focus();
                    this.__pageValueInput?.select();

                    document.body.addEventListener("mousedown", this.__closePageMenuFunc, false);
                    break;
                case "Url":
                    this.__urlValueInput?.focus();
                    break;
                default:
                    throw new Error("");
            }
        });

        this.registerCommand("select-type", (context: CommandContext) => {
            const type = context.target.getAttribute("data-value") as HyperLinkType;

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
            this.__pageValueInput?.setAttribute("value-page-id", pageId);
            this.__inputElem.innerText = context.target.innerText;
            this.__pageValueInput.value = context.target.innerText;

            this.refreshUI();

            if (this.__onChange)
                this.__onChange({value: pageId, valueType: "Page"});
        });

        this.__urlValueInput?.addEventListener("change", () => {
            this.refreshUI();

            if (this.__onChange)
                this.__onChange({value: this.__urlValueInput?.value || "", valueType: "Url"});
        });

        this.__pageValueInput?.addEventListener("keyup", () => {
            const title = this.__pageValueInput?.value;
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
                            if (!this.__searchElem) throw new Error("");
                            DOM.empty(this.__searchElem);
    
                            if (response.data?.length) {
                                for (let i = 0; i < response.data.length; i++) {
                                    const page = response.data[i];
    
                                    this.__searchElem.appendChild(DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-page", "data-value": page.id }, page.title)));
                                }
                            }
                            else
                                this.__searchElem.appendChild(DOM.tag("li", { class: "text" }, "Страниц не найдено"));
    
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
                    value: this.__pageValueInput?.value,
                    pageTitle: this.__pageValueInput?.value
                };
            }
            case "Url": {
                return {
                    valueType: this.__type,
                    value: this.__urlValueInput?.value,
                };
            }
            default:
                throw new Error("");
        }
     }
    

    hasValue(): boolean {
        switch (this.__type) {
            case "Page": {
                return Boolean(this.__pageValueInput?.hasAttribute("value-page-id"));
            }
            case "Url": {
                return this.__urlValueInput?.value ? true : false;
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
                    this.__pageValueInput?.setAttribute("value-page-id", value.value);
                    if (value.pageTitle) {
                        this.__inputElem.innerText = value.pageTitle;
                        this.__pageValueInput.value = value.pageTitle;
                    }
                    break;
                }
                case "Url": {
                    this.__urlValueInput.value = value.value;
                    this.__inputElem.innerText = value.value;
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

        if (this.__typeElem)
            this.__typeElem.innerText = this.__type;

        switch (this.__type) {
            case "Page": {
                this.element?.classList.remove("url-value");
                this.element?.classList.add("page-value");
                this.__inputElem.innerText = this.__pageValueInput.value;
                this.__placeholderElem.innerText = "Выберите страницу";
                break;
            }
            case "Url": {
                this.element?.classList.remove("page-value");
                this.element?.classList.add("url-value");
                this.__inputElem.innerText = this.__urlValueInput.value;
                this.__placeholderElem.innerText = "Введите url";
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