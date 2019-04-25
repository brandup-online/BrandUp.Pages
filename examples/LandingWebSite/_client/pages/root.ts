import "./styles.less";
import { DOM, UIElement } from "brandup-ui";
import { listPageCollection } from "./dialogs/page-collection-list";

class BrandUpPages extends UIElement {
    get typeName(): string { return "BrandUpPages.Toolbar"; }

    load() {
        this.__renderToolbars();
        this.__registerCommands();
    }

    private __renderToolbars() {
        var toolbarElem = DOM.tag("div", { class: "brandup-pages-elem brandup-pages-toolbar" }, [
            DOM.tag("button", { class: "brandup-pages-toolbar-button list", "data-command": "brandup-pages-collections" })
        ]);
        document.body.appendChild(toolbarElem);

        this.setElement(toolbarElem);
    }

    private __registerCommands() {
        this.registerCommand("brandup-pages-collections", () => {
            listPageCollection(null);
        });
    }
}

var current: BrandUpPages = null;
const initialize = () => {
    if (!current && document.readyState === "complete") {
        current = new BrandUpPages();
    }
}

document.addEventListener("readystatechange", initialize);
initialize();

window.addEventListener("load", () => {
    current.load();
});