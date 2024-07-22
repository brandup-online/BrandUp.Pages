import { Page, PageModel } from "@brandup/ui-website";
import { PageToolbar } from "./admin/page-toolbar";
import "./styles.less";

let toolbar: PageToolbar | null = null;

const showUI = (page: Page<PageModel>) => {
    if (toolbar) {
        toolbar.destroy();
        toolbar = null;
    }

    toolbar = new PageToolbar(page);
    return toolbar.hasEditor;
}

export default showUI;