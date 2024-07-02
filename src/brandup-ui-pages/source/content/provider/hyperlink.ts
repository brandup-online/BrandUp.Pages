import { AjaxResponse } from "brandup-ui-ajax";
import { HyperLinkFieldFormValue, HyperLinkFieldFormOptions, HyperLinkContent } from "../../content/field/hyperlink";
import { FieldProvider } from "./base";

export class HyperlinkFieldProvider extends FieldProvider<HyperLinkFieldFormValue, HyperLinkFieldFormOptions> {
    createDesigner() {
        return null;
    }

    createField() {
        const { name, errors, options } = this.__model;
        this.field = new HyperLinkContent(name, errors, options, this);
        return this.field;
    }

    changeValue(url: string) {
        this.request({
            url: `/brandup.pages/content/hyperlink/url`,
            urlParams: { url },
            method: "POST",
            success: (response: AjaxResponse<HyperLinkFieldFormValue>) => {
                switch (response.status) {
                    case 200:
                        this.setValue(response.data);

                        break;
                    default:
                        throw "";
                }
            }
        });
    }

    selectPage(pageId: string) {
        this.request({
            url: `/brandup.pages/content/hyperlink/page`,
            urlParams: {
                pageId: pageId
            },
            method: "POST",
            success: (response: AjaxResponse<HyperLinkFieldFormValue>) => {
                switch (response.status) {
                    case 200:
                        this.setValue(response.data);

                        break;
                    default:
                        throw "";
                }
            }
        });
    }
}