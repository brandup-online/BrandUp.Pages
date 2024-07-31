import { UIElement } from "@brandup/ui";
import { DOM, TagChildrenLike } from "@brandup/ui-dom";
import "../../../styles.less"

export class Toggler extends UIElement {
    get typeName(): string { return "BrandUpPages.Toggler"; }


    constructor(options: ITogglerOptions) {
        super();

        const container = DOM.tag("ul", {class: "bp-toggler"}, options.items.map((item, index) => 
            DOM.tag("li", { 
                dataset: { value: item.value.toString(), toggleIndex: index.toString()}, 
                class: options.defaultValue === item.value ? "current" : undefined,
                command: "toggle"
            }, item.content)
        ))

        this.setElement(container);

        this.registerCommand("toggle", (context) => {
            if (context.target.classList.contains("current")) return;
            if (!this.element) throw new Error("toggle element is undefined");

            const items = DOM.queryElements(this.element, "li");
            items.forEach(item => {
                item.classList.remove("current");
                if (item === context.target) {
                    item.classList.add("current");
                    this.trigger("change", item.dataset.value);
                }
            })
        })
    }
}

export interface ITogglerOptions {
    items: ITogglerItem[];
    defaultValue?: string | number;
}

export interface ITogglerItem {
    content: TagChildrenLike | Array<TagChildrenLike>;
    value: string | number;
}