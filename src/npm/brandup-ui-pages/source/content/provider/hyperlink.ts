import { AjaxResponse } from "@brandup/ui-ajax";
import { FieldProvider } from "./base";
import { FieldValueResult } from "../../typings/content";

export class HyperlinkFieldProvider extends FieldProvider<HyperLinkValue, HyperLinkFieldOptions> {
    createDesigner() {
        return null;
    }

    async saveValue(value: HyperLinkValue) {
        if (value.valueType === "Page")
            await this.selectPage(value.value);
        else if (value.valueType === "Url")
            await this.changeUrl(value.value);
    }

    async changeUrl(url: string) {
        const response: AjaxResponse<FieldValueResult> = await this.request({
            url: `/brandup.pages/content/hyperlink/url`,
            query: { url },
            method: "POST",
        });
        
        switch (response.status) {
            case 200:
                if (!response.data) throw new Error("error load data");

                this.onSavedValue(response.data);

                if (this.valueElem) {
                    const value = this.getValue();
                    this.valueElem.setAttribute("href", value.value);
                }

                break;
            default:
                throw new Error("");
        }
    }

    async selectPage(pageId: string) {
        const response: AjaxResponse<FieldValueResult> = await this.request({
            url: `/brandup.pages/content/hyperlink/page`,
            query: { pageId },
            method: "POST",
        });

        switch (response.status) {
            case 200:
                if (!response.data) throw new Error("error load data");

                this.onSavedValue(response.data);

                if (this.valueElem) {
                    const value = this.getValue();
                    this.valueElem.setAttribute("href", value.value);
                }

                break;
            default:
                throw new Error("");
        }
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