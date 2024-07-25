import { MockedContent } from "../../../../mocks/content/content"
import { TextFieldProvider } from "../text";
import { MockProviderValueResponse } from "../../../../mocks/content/provider/common"

const contentModel = {
    "type": "Text",
    "name": "Header",
    "title": "Header",
    "isRequired": true,
    "options": {
      "allowMultiline": false,
      "placeholder": null
    },
    "value": "test test",
    "errors": []
}

const createProvider = () => {
    const content = new MockedContent();
    const provider = new TextFieldProvider(content, contentModel);
    return provider;
}

describe('Text provider', () => {
    it("Success initialization", () => {
        const provider = createProvider();
        expect (provider).toBeInstanceOf(TextFieldProvider);
    })

    it("Success change value", async () => {
        const provider = createProvider();

        expect(provider.getValue()).toEqual("test test");

        expect(provider.normalizeValue("    test123   \r\n   ")).toEqual("test123");
    
        MockProviderValueResponse({ value: "    test123   \r\n   ", errors: [] });

        await provider.saveValue("    test123   \r\n   ");
        expect(provider.getValue()).toEqual("test123");
    })

    it("Change value with errors", async () => {
        const provider = createProvider();

        expect(provider.errors).toEqual([]);
    
        MockProviderValueResponse({ value: "test123", errors: ["test error"] });

        await provider.saveValue("test123");
        expect(provider.errors).toEqual(["test error"]);
    })
})
    

