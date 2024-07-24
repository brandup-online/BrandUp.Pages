import { AjaxRequest } from "@brandup/ui-ajax";
import { Content } from "../../../content/content";
import { HyperlinkFieldProvider } from "../hyperlink";
import { 
    set_Hyperlink_PageValue_Request_Mock,
    set_Hyperlink_UrlValue_Request_Mock,
    set_Hyperlink_Error_Value_Request_Mock 
} from "../../../../../__mocks__/content/provider/hyperlink"

const editId = "77ace56a-0429-4258-a342-0e61159f082d";

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
    const provider = new HyperlinkFieldProvider(content, contentModel);
    return provider;
}

describe('Text provider', () => {
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
    
        set_Hyperlink_PageValue_Request_Mock();
        await provider.saveValue({ "valueType": "Page", "value": "test", "pageTitle": "test" });
        expect(provider.getValue()).toEqual({ "valueType": "Page", "value": "test", "pageTitle": "test" });

        set_Hyperlink_UrlValue_Request_Mock();
        await provider.saveValue({ "valueType": "Url", "value": "123", "pageTitle": "" });
        expect(provider.getValue()).toEqual({ "valueType": "Url", "value": "123", "pageTitle": null });
    })

    it("Change value with errors", async () => {
        const provider = createProvider();

        expect(provider.errors).toEqual([]);
    
        set_Hyperlink_Error_Value_Request_Mock();

        await provider.saveValue({ "valueType": "Url", "value": "123", "pageTitle": "" });
        expect(provider.errors).toEqual(["test error"]);
    })
})
    

