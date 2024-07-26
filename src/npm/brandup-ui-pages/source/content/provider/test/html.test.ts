import { MockedContent } from "../../../../mocks/content/content"
import { HtmlFieldProvider } from "../html";
import { MockResponse } from "../../../../mocks/common"

const contentModel = {
    "type": "Html",
    "name": "Text",
    "title": "Text",
    "isRequired": true,
    "options": {
    "placeholder": "Введите текст"
    },
    "value": "<p>Test test test test test</p>",
    "errors": []
}

const createProvider = () => {
    const content = new MockedContent();
    const provider = new HtmlFieldProvider(content, contentModel);
    return provider;
}

describe('Http provider', () => {
    it("Success initialization", () => {
        const provider = createProvider();
        expect (provider).toBeInstanceOf(HtmlFieldProvider);
    })

    it("Success change value", async () => {
        const provider = createProvider();

        expect(provider.getValue()).toEqual("<p>Test test test test test</p>");
    
        MockResponse({ value: "<p>test123</p>", errors: [] });

        await provider.saveValue("<p>test123</p>");
        expect(provider.getValue()).toEqual("<p>test123</p>");
    })

    it("Change value with errors", async () => {
        const provider = createProvider();

        expect(provider.errors).toEqual([]);
    
        MockResponse({ value: "<p>test123</p>", errors: ["test error"] });

        await provider.saveValue("<p>test123</p>");
        expect(provider.errors).toEqual(["test error"]);
    })
})
    

