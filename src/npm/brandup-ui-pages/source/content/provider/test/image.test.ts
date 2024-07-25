import { ImageFieldProvider } from "../image";
import { MockProviderValueResponse } from "../../../../mocks/content/provider/common"
import { MockedContent } from "../../../../mocks/content/content"

const contentModel = {
    "type": "Image",
    "name": "Background",
    "title": "Background",
    "isRequired": true,
    "options": null,
    "value": {
      "valueType": "Url",
      "value": "/images/banner.jpg",
      "previewUrl": "/images/banner.jpg"
    },
    "errors": []
}

const createProvider = () => {
    const content = new MockedContent();
    const provider = new ImageFieldProvider(content, contentModel);
    return provider;
}

describe('Image provider', () => {
    it("Success initialization", () => {
        const provider = createProvider();
        expect (provider).toBeInstanceOf(ImageFieldProvider);
    })

    it("Success change value", async () => {
        const provider = createProvider();

        expect(provider.getValue()).toEqual({ "valueType": "Url", "value": "/images/banner.jpg", "previewUrl": "/images/banner.jpg" });
    
        MockProviderValueResponse({ 
            value: { "valueType": "Url", "value": "test", "previewUrl": "test" },
            errors: [],
        });

        await provider.saveValue("test");
        expect(provider.getValue()).toEqual({ "valueType": "Url", "value": "test", "previewUrl": "test" });
    })

    it("Change value with errors", async () => {
        const provider = createProvider();

        expect(provider.errors).toEqual([]);
    
        MockProviderValueResponse({ value: "test123", errors: ["test error"] });

        await provider.saveValue("test123");
        expect(provider.errors).toEqual(["test error"]);
    })
})
    

