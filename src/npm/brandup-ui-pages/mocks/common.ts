export const MockResponse = (body?: any, init: ResponseInit = { headers: {"content-type": "application/json"} }) => {
    /*@ts-ignore */
    global.fetch = jest.fn(() => Promise.resolve(
        new Response(JSON.stringify(body), init)
    ));
}