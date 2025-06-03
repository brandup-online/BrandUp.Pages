import { Page, PageModel, WebsiteApplication } from "@brandup/ui-website";
import { DOM } from "@brandup/ui-dom";

class AboutPage extends Page<WebsiteApplication, PageModel> {
    get typeName(): string { return "AboutPage" }

    protected async onRenderContent() {
        await super.onRenderContent();

        //this.element.appendChild(DOM.tag("div", null, DOM.tag("a", { href: this.buildUrl({ param: "test1" }), class: "applink" }, "test1")));
        //this.element.appendChild(DOM.tag("div", null, DOM.tag("a", { href: this.buildUrl({ param: "test2" }), class: "applink" }, "test2")));
        //this.element.appendChild(DOM.tag("div", null, DOM.tag("a", { href: this.buildUrl({ param: "test3" }), class: "applink" }, "test3")));
    }
}

export default AboutPage;