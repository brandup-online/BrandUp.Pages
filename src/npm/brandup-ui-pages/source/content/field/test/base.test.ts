import { DOM } from "@brandup/ui-dom";
import { MockedHtmlProvider, MockedHyperlinkProvider, MockedImageProvider, MockedModelProvider, MockedTextProvider } from "../../../../mocks/content/provider";
import TextContent from "../text";
import HtmlContent from "../html";
import ImageContent from "../image";
import HyperLinkContent from "../hyperlink";
import ModelField from "../model";

describe('Form fields', () => {
    it("Text", () => {
        const provider = MockedTextProvider();
        const field = new TextContent("test", {}, provider);
        const elem = DOM.tag("div");
        field.render(elem);

        expect (field).toBeInstanceOf(TextContent);
        expect (field.getValue()).toEqual("mocked provider value");
        field.raiseUpdateValue("test");
        expect (field.getValue()).toEqual("test");
    })

    it("HTML", () => {
        // const provider = MockedHtmlProvider();
        // const field = new HtmlContent("test", { placeholder: "test" }, provider);
        // const elem = DOM.tag("div");
        // field.render(elem);

        // expect (field).toBeInstanceOf(HtmlContent);
        // expect (field.getValue()).toEqual("<p>mocked provider value</p>");
        // field.raiseUpdateValue("<p>test</p>");
        // expect (field.getValue()).toEqual("<p>test</p>");
    })

    it("Image", () => {
        const provider = MockedImageProvider();
        const field = new ImageContent("test", {}, provider);
        const elem = DOM.tag("div");
        field.render(elem);

        expect (field).toBeInstanceOf(ImageContent);
        expect (field.getValue()).toEqual({ valueType: "Url", value: "test", previewUrl: "test" });
        field.raiseUpdateValue({ valueType: "Page", value: "test123", previewUrl: "test123" });
        expect (field.getValue()).toEqual({ valueType: "Page", value: "test123", previewUrl: "test123" });
    })

    it("Hyperlink", () => {
        const provider = MockedHyperlinkProvider();
        const field = new HyperLinkContent("test", { valueType: "Url", value: "test" }, provider);
        const elem = DOM.tag("div");
        field.render(elem);

        expect (field).toBeInstanceOf(HyperLinkContent);
        expect (field.getValue()).toEqual({ valueType: "Url", value: "test" });
        field.raiseUpdateValue({ valueType: "Page", value: "test123", pageTitle: "test page" });
        expect (field.getValue()).toEqual({ valueType: "Page", value: "test page", pageTitle: "test page" });
    })

    it("Model", () => {
        const options = {
            isListValue: true,
            addText: "",
            itemType: {name: "", title: ""},
            itemTypes: [
              {
                name: "BB1_Item",
                title: "Баннер 1/3"
              },
              {
                name: "BB1_Item2",
                title: "Баннер 2/3"
              }
            ]
        };

        const provider = MockedModelProvider();
        const field = new ModelField("test", options, provider);
        const elem = DOM.tag("div");
        field.render(elem);

        expect (field).toBeInstanceOf(ModelField);
        expect (field.getValue().items.length).toEqual(2);
        field.raiseUpdateValue({items: [{
            title: "Test test test",
            type: {
              name: "BB1_Item",
              title: "Баннер 1/3"
            }
          }]});
          expect (field.getValue().items.length).toEqual(1);
    })
})