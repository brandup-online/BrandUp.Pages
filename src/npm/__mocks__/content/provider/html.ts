export const set_Html_Value_Request_Mock = () => {
    fetchMock.mockResponseOnce(JSON.stringify({
        "value": "<p>test123</p>",
        "errors": []
    }));
}

export const set_Html_Error_Value_Request_Mock = () => {
    fetchMock.mockResponseOnce(JSON.stringify({
        "value": "<p>test123</p>",
        "errors": ["test error"]
    }));
}