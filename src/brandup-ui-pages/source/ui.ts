import { DOM } from "brandup-ui-dom";
import { Page, PageModel } from "brandup-ui-website";
import { IContentModel, PageToolbar } from "./admin/page-toolbar";
import { Editor } from "./content/editor";
import "./styles.less";

const showUI = (page: Page<PageModel>, content: IContentModel[]) => {
    const editingContentElem = DOM.queryElement(document.body, "[data-content-edit-id]");
    if (!editingContentElem) {
        page.attachDestroyElement(new PageToolbar(page));
    }
    else {
        page.attachDestroyElement(new Editor(page, editingContentElem, content));
    }
}

export default showUI;