import { FieldValueResult } from "../../../source/typings/content";

export const MockProviderValueResponse = (response: FieldValueResult) => {
    fetchMock.mockResponseOnce(JSON.stringify(response));
}