export const set_Text_Value_Request_Mock = () => {
    fetchMock.mockResponseOnce(JSON.stringify({
        "value": "    test123   \r\n   ",
        "errors": []
    }));
}

export const set_Text_Error_Value_Request_Mock = () => {
    fetchMock.mockResponseOnce(JSON.stringify({
        "value": "test123",
        "errors": ["test error"]
    }));
}