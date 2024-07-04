import { Page, PageModel } from "brandup-ui-website";
import { PageToolbar } from "./admin/page-toolbar";
import { ContentEditor } from "./content/editor";
import "./styles.less";

const showUI = (page: Page<PageModel>) => {
    const editId = page.nav.query["editid"];
    if (!editId) {
        page.attachDestroyElement(new PageToolbar(page));
    }
    else {
        page.attachDestroyElement(new ContentEditor(page.website, editId));
    }
}

export default showUI;