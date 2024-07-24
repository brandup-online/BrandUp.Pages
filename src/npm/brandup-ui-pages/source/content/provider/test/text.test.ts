import { AjaxRequest } from "@brandup/ui-ajax";
import { Content } from "../../../content/content";
import { TextFieldProvider } from "../text";
import { set_Text_Value_Request_Mock, set_Text_Error_Value_Request_Mock } from "../../../../../__mocks__/content/provider/text"

const editId = "77ace56a-0429-4258-a342-0e61159f082d";

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
    
        set_Text_Value_Request_Mock();

        await provider.saveValue("    test123   \r\n   ");
        expect(provider.getValue()).toEqual("test123");
    })

    it("Change value with errors", async () => {
        const provider = createProvider();

        expect(provider.errors).toEqual([]);
    
        set_Text_Error_Value_Request_Mock();

        await provider.saveValue("test123");
        expect(provider.errors).toEqual(["test error"]);
    })
})
    

