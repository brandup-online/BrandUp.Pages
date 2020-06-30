import { UIElement } from "brandup-ui";

class BB1 extends UIElement {
    constructor(elem: HTMLElement) {
        super();

        this.setElement(elem);

        //this.element.addEventListener("click", () => { alert("test"); });
    }

    get typeName(): string { return "LandingWebSite.BB1" }
}

export default BB1;