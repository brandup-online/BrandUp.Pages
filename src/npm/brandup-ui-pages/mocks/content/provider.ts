import { ImageFieldProvider } from "../../source/content/provider/image";
import { HtmlFieldProvider } from "../../source/content/provider/html";
import { TextFieldProvider } from "../../source/content/provider/text";
import { HyperlinkFieldProvider } from "../../source/content/provider/hyperlink";
import { ModelFieldProvider } from "../../source/content/provider/model";

jest.mock('../../source/content/provider/text', () => {
    return {
        TextFieldProvider: jest.fn().mockImplementation(() => {
            return {
                errors: [],
                getValue: () => "mocked provider value",
                saveValue: () => {},
            };
        })
    };
});
export const MockedTextProvider = <jest.Mock<TextFieldProvider>>TextFieldProvider;

jest.mock('../../source/content/provider/html', () => {
    return {
        HtmlFieldProvider: jest.fn().mockImplementation(() => {
            return {
                errors: [],
                getValue: () => "<p>mocked provider value</p>",
                saveValue: () => {},
            };
        })
    };
});
export const MockedHtmlProvider = <jest.Mock<HtmlFieldProvider>>HtmlFieldProvider;

jest.mock('../../source/content/provider/image', () => {
    return {
        ImageFieldProvider: jest.fn().mockImplementation(() => {
            return {
                errors: [],
                getValue: () => ({ valueType: "Url", value: "test", previewUrl: "test" }),
                saveValue: () => {},
            };
        })
    };
});
export const MockedImageProvider = <jest.Mock<ImageFieldProvider>>ImageFieldProvider;

jest.mock('../../source/content/provider/hyperlink', () => {
    return {
        HyperlinkFieldProvider: jest.fn().mockImplementation(() => {
            return {
                errors: [],
                getValue: () => ({ valueType: "Url", value: "test" }),
                saveValue: () => {},
            };
        })
    };
});
export const MockedHyperlinkProvider = <jest.Mock<HyperlinkFieldProvider>>HyperlinkFieldProvider;

jest.mock('../../source/content/provider/model', () => {
    return {
        ModelFieldProvider: jest.fn().mockImplementation(() => {
            return {
                errors: [],
                getValue: () => ({
                    "items": [
                      {
                        title: "Test test test",
                        type: {
                          name: "BB1_Item",
                          title: "Баннер 1/3"
                        }
                      },
                      {
                        title: "Test test test",
                        type: {
                          name: "BB1_Item2",
                          title: "Баннер 2/3"
                        }
                      }
                    ]
                  }),
                saveValue: () => {},
            };
        })
    };
});
export const MockedModelProvider = <jest.Mock<ModelFieldProvider>>ModelFieldProvider;