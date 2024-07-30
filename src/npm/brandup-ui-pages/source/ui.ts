import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";
import { PageToolbar } from "./admin/page-toolbar";
import "./styles.less";

let toolbar: PageToolbar | undefined;

const showUI = (page: Page<WebsiteApplication,PageModel>) => {
    if (toolbar) {
        toolbar.destroy();
        toolbar = undefined;
    }

    toolbar = new PageToolbar(page);
    return toolbar.hasEditor;
}

export default showUI;