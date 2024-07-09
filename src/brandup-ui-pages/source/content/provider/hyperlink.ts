import { AjaxResponse } from "brandup-ui-ajax";
import { FieldProvider } from "./base";
import { FieldValueResult } from "../../typings/content";

export class HyperlinkFieldProvider extends FieldProvider<HyperLinkValue, HyperLinkFieldOptions> {
    createDesigner() {
        return null;
    }

    saveValue(value: any) {
        throw "method saveValue not implemented in HyperlinkFieldProvider";
    }

    changeValue(url: string) {
        this.request({
            url: `/brandup.pages/content/hyperlink/url`,
            urlParams: { url },
            method: "POST",
            success: (response: AjaxResponse<FieldValueResult>) => {
                switch (response.status) {
                    case 200:
                        this.onSavedValue(response.data);

                        const value = this.getValue();
                        this.valueElem.setAttribute("href", value.value);

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
            urlParams: { pageId },
            method: "POST",
            success: (response: AjaxResponse<FieldValueResult>) => {
                switch (response.status) {
                    case 200:
                        this.onSavedValue(response.data);

                        const value = this.getValue();
                        this.valueElem.setAttribute("href", value.value);

                        break;
                    default:
                        throw "";
                }
            }
        });
    }
}

export type HyperLinkType = "Url" | "Page";

export interface HyperLinkValue {
    valueType: HyperLinkType;
    value: string;
    pageTitle?: string;
}

export interface HyperLinkFieldOptions {
    valueType: "Url" | "Page";
    value: string;
}