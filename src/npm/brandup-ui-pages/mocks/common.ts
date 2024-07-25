import fetchMock, { MockParams } from "jest-fetch-mock"

export const MockProviderValueResponse = (response: any, options?: MockParams) => {
    fetchMock.mockResponseOnce(JSON.stringify(response), options);
}