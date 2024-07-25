import { Content } from "../../source/content/content";
import { AjaxRequest } from "@brandup/ui-ajax";

const editId = "77ace56a-0429-4258-a342-0e61159f082d";

const api = async (request: AjaxRequest) => {
    const response = await fetch(request.url!.toString());
    const data = await response.json();
    return {status: response.status, data};
}

jest.mock('../../source/content/content', () => {
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

export const MockedContent = <jest.Mock<Content>>Content;