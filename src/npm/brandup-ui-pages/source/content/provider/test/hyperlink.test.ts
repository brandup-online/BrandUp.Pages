import { MockedContent } from "../../../../mocks/content/content"
import { HyperlinkFieldProvider } from "../hyperlink";
import { MockProviderValueResponse } from "../../../../mocks/content/provider/common"

const contentModel = {
    "type": "HyperLink",
    "name": "Link",
    "title": "Link",
    "isRequired": true,
    "options": null,
    "value": {
      "valueType": "Page",
      "value": "f3b5cdcd-def8-402b-82c7-fc614c558ab2",
      "pageTitle": "321"
    },
    "errors": []
}

const createProvider = () => {
    const content = new MockedContent();
    const provider = new HyperlinkFieldProvider(content, contentModel);
    return provider;
}

describe('Hyperlink provider', () => {
    it("Success initialization", () => {
        const provider = createProvider();
        expect (provider).toBeInstanceOf(HyperlinkFieldProvider);
    })

    it("Success change value", async () => {
        const provider = createProvider();

        expect(provider.getValue()).toEqual({
            "valueType": "Page",
            "value": "f3b5cdcd-def8-402b-82c7-fc614c558ab2",
            "pageTitle": "321"
        });
    
        MockProviderValueResponse({ value: { "valueType": "Page", "value": "test", "pageTitle": "test" }, errors: [] });
        await provider.saveValue({ "valueType": "Page", "value": "test", "pageTitle": "test" });
        expect(provider.getValue()).toEqual({ "valueType": "Page", "value": "test", "pageTitle": "test" });

        MockProviderValueResponse({ value: { "valueType": "Url", "value": "123", "pageTitle": null }, errors: [] });
        await provider.saveValue({ "valueType": "Url", "value": "123", "pageTitle": "" });
        expect(provider.getValue()).toEqual({ "valueType": "Url", "value": "123", "pageTitle": null });
    })

    it("Change value with errors", async () => {
        const provider = createProvider();

        expect(provider.errors).toEqual([]);
    
        MockProviderValueResponse({ value: { "valueType": "Url", "value": "123", "pageTitle": "" }, errors: ["test error"] });

        await provider.saveValue({ "valueType": "Url", "value": "123", "pageTitle": "" });
        expect(provider.errors).toEqual(["test error"]);
    })
})
    

