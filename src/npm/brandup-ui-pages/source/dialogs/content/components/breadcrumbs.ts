import { CommandContext, UIElement } from "@brandup/ui";
import { ContentEditor } from "../../../content/editor";
import { DOM } from "@brandup/ui-dom";

export class Breadcrumbs extends UIElement {
    private __editor: ContentEditor;

    get typeName(): string { return "BrandUpPages.Breadcrumbs"; }

    constructor(editor: ContentEditor) {
        super();
        this.__editor = editor;

        const container = DOM.tag("ol", { class: "nav" });
        this.setElement(container);

        this.registerCommand("navigate", (context: CommandContext) => {
            const path = context.target.getAttribute("data-path");
            if (path === null || path === undefined) throw new Error("not found attribute data-path");

            this.trigger("navigate", path);
        });
    }

    render(path: string, modelPath: string) {
        if (!this.element) throw new Error("Breadcrumbs render error");

        DOM.empty(this.element);
        while (path || path === "") {
            const content = this.__editor.navigate(path);
            let title = content.typeTitle;
            this.element.insertAdjacentElement("afterbegin", DOM.tag("li", path === modelPath ? { class: "current" } : null, [
                DOM.tag("a", { href: "", "data-command": "navigate", "data-path": path }, [
                    DOM.tag("b", null, path || "root"),
                    DOM.tag("div", null, [
                        DOM.tag("span", null, title),
                        DOM.tag("span", null, content.typeName),
                    ]),
                ]),
            ]));
 
            path = content.parentPath;
        }
    }
}