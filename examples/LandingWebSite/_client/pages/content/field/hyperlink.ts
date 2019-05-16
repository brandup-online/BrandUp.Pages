import { IContentField, IContentForm } from "../../typings/content";
import { Field } from "../../form/field";
import { DOM } from "brandup-ui";
import iconArrow from "../../svg/combobox-arrow.svg";
import "./hyperlink.less";

export class HyperLinkContent extends Field<HyperLinkFieldFormValue, HyperLinkFieldFormOptions> implements IContentField {
    readonly form: IContentForm;
    private __typeElem: HTMLElement;
    private __pageValueElem: HTMLElement;
    private __urlValueElem: HTMLInputElement;
    private __menuElem: HTMLElement;
    private __closeMenuFunc: (e: MouseEvent) => void;
    private __type: HyperLinkType;

    constructor(form: IContentForm, name: string, options: HyperLinkFieldFormOptions) {
        super(name, options);

        this.form = form;
    }

    get typeName(): string { return "BrandUpPages.Form.Field.HyperLink"; }

    protected _onRender() {
        super._onRender();

        this.element.classList.add("hyperlink");

        var typeContainerElem = DOM.tag("div", { class: "type", "data-command": "open-menu" }, [
            this.__typeElem = DOM.tag("span", null, "Page"),
            iconArrow
        ]);
        this.element.appendChild(typeContainerElem);

        this.__pageValueElem = DOM.tag("div", { class: "value" });
        this.element.appendChild(this.__pageValueElem);

        this.__urlValueElem = <HTMLInputElement>DOM.tag("input", { type: "text" });
        this.element.appendChild(this.__urlValueElem);

        this.__urlValueElem.addEventListener("change", () => {
            this.form.queue.request({
                url: `/brandup.pages/content/hyperlink`,
                urlParams: {
                    editId: this.form.editId,
                    path: this.form.contentPath,
                    field: this.name,
                    url: this.__urlValueElem.value
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

        this.__menuElem = DOM.tag("ul", { class: "hyperlink-menu" }, [
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Page" }, "Page")),
            DOM.tag("li", null, DOM.tag("a", { href: "", "data-command": "select-type", "data-value": "Url" }, "Url"))
        ]);
        this.element.appendChild(this.__menuElem);

        this.__closeMenuFunc = (e: MouseEvent) => {
            let t = <Element>e.target;
            if (!t.closest(".hyperlink-menu")) {
                this.element.classList.remove("opened-menu");
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
            }
        };

        this.registerCommand("open-menu", () => {
            if (!this.element.classList.toggle("opened-menu")) {
                document.body.removeEventListener("click", this.__closeMenuFunc, false);
                return;
            }

            document.body.addEventListener("mousedown", this.__closeMenuFunc, false);
        });
        this.registerCommand("select-type", (elem: HTMLElement) => {
            let type = <HyperLinkType>elem.getAttribute("data-value");
            
            this.element.classList.remove("opened-menu");
            document.body.removeEventListener("click", this.__closeMenuFunc, false);

            this.__changeType(type);
        });

        this.__changeType("Page");
    }

    private __changeType(type: HyperLinkType) {
        this.__typeElem.innerText = type;
        this.__type = type;

        switch (type) {
            case "Page": {
                this.element.classList.remove("url-value");
                this.element.classList.add("page-value");
                break;
            }
            case "Url": {
                this.element.classList.remove("page-value");
                this.element.classList.add("url-value");
                break;
            }
        }
    }

    getValue(): HyperLinkFieldFormValue {
        switch (this.__type) {
            case "Page": {
                if (!this.__pageValueElem.innerText)
                    return null;

                return {
                    valueType: "Page",
                    value: this.__pageValueElem.innerText
                };
            }
            case "Url": {
                if (!this.__urlValueElem.value)
                    return null;

                return {
                    valueType: "Url",
                    value: this.__urlValueElem.value
                };
            }
            default:
                throw "";
        }
    }
    setValue(value: HyperLinkFieldFormValue) {
        if (value) {
            this.__changeType(value.valueType);

            switch (value.valueType) {
                case "Page": {
                    this.__pageValueElem.innerText = value.value;
                    break;
                }
                case "Url": {
                    this.__urlValueElem.value = value.value;
                    break;
                }
                default:
                    throw "";
            }
        }
        else {
        }
    }
    hasValue(): boolean {
        switch (this.__type) {
            case "Page": {
                return this.__pageValueElem.innerText ? true : false;
            }
            case "Url": {
                return this.__urlValueElem.value ? true : false;
            }
            default:
                throw "";
        }
    }

    protected _onChanged() {
        this.form.queue.request({
            url: '/brandup.pages/content/hyperlink',
            urlParams: {
                editId: this.form.editId,
                path: this.form.contentPath,
                field: this.name
            },
            method: "POST",
            type: "JSON",
            data: this.getValue(),
            success: (data: HyperLinkFieldFormValue, status: number) => {
                if (status === 200) {
                    this.setValue(data);
                }
                else {
                    this.setErrors([ "error" ]);
                }
            }
        });
    }
}

export type HyperLinkType = "Url" | "Page";

export interface HyperLinkFieldFormValue {
    valueType: HyperLinkType;
    value: string;
}

export interface HyperLinkFieldFormOptions {
    valueType: "Url" | "Page";
    value: string;
}