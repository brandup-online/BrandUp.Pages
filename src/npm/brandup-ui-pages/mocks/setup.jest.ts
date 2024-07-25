import fetchMock from "jest-fetch-mock"
import { mockApi } from "./brandup-ajax/queue";
fetchMock.enableMocks();

beforeAll(() => {
    mockApi;
})

beforeEach(() => {
    fetchMock.mockClear();
})