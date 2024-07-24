export const set_Hyperlink_PageValue_Request_Mock = () => {
    fetchMock.mockResponseOnce(JSON.stringify({
        "value": {
          "valueType": "Page",
          "value": "test",
          "pageTitle": "test"
        },
        "errors": []
      }));
}

export const set_Hyperlink_UrlValue_Request_Mock = () => {
    fetchMock.mockResponseOnce(JSON.stringify({
        "value": {
          "valueType": "Url",
          "value": "123",
          "pageTitle": null
        },
        "errors": []
      }));
}

export const set_Hyperlink_Error_Value_Request_Mock = () => {
    fetchMock.mockResponseOnce(JSON.stringify({
        "value": {
          "valueType": "Url",
          "value": "123",
          "pageTitle": null
        },
        "errors": ["test error"]
    }));
}