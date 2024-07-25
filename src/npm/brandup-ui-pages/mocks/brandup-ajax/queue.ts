import {AjaxRequest, AjaxResponse } from "@brandup/ui-ajax";

export class MockedAjaxQueue {
	async enque(request: AjaxRequest, abortSignal?: AbortSignal) {
		const response = await fetch(request.url!.toString());
        const data = await response.json();
        return {status: response.status, data};
	}

	destroy() {}
}

export const mockApi = 
    jest.mock('@brandup/ui-ajax', () => {
        const originalModule = jest.requireActual('@brandup/ui-ajax');

        return {
            __esModule: true,
            ...originalModule,
            AjaxQueue: MockedAjaxQueue
          };
    });