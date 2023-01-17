import { Page, PageModel } from "brandup-ui-website";
import "./form.less";

class FormPage<TModel extends PageModel> extends Page<TModel> {
    get typeName(): string { return "LandingWebSite.FormPage" }

    protected _onRenderElement(elem) {
        super._onRenderElement(elem);
    }
}

export default FormPage;