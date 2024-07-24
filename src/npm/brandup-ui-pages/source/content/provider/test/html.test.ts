import { AjaxRequest } from "@brandup/ui-ajax";
import { Content } from "../../../content/content";
import { HtmlFieldProvider } from "../html";
import { set_Html_Value_Request_Mock, set_Html_Error_Value_Request_Mock } from "../../../../../__mocks__/content/provider/html"

const editId = "77ace56a-0429-4258-a342-0e61159f082d";

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

const api = async (request: AjaxRequest) => {
    const response = await fetch(request.url!.toString());
    const data = await response.json();
    return {status: response.status, data};
}

jest.mock('../../../content/content', () => {
    return {
        Content: jest.fn().mockImplementation(() => {
            return {
                path: "Blocks[1]",
                host: {
                    editor: {
                        editId: editId,
                        api: api
                    }
                },
            };
        })
    };
});

const MockedContent = <jest.Mock<Content>>Content;

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
    
        set_Html_Value_Request_Mock();

        await provider.saveValue("<p>test123</p>");
        expect(provider.getValue()).toEqual("<p>test123</p>");
    })

    it("Change value with errors", async () => {
        const provider = createProvider();

        expect(provider.errors).toEqual([]);
    
        set_Html_Error_Value_Request_Mock();

        await provider.saveValue("<p>test123</p>");
        expect(provider.errors).toEqual(["test error"]);
    })
})
    

