
import { MockResponse } from "../../../mocks/common";
import { Content, IContentHost } from "../../content/content";
import { ContentEditor } from "../../content/editor";
import { contentModel, editId } from "./data";
import { WebsiteApplication } from "@brandup/ui-website";


describe('Content', () => {
    it("Success initialization", () => {
        const content = new Content(new TestHost(), contentModel);
        expect (content).toBeInstanceOf(Content);
        expect (content.fields.size).toEqual(2);
        expect (content.getField("Header")).toBeTruthy();
        expect (() => content.getField("test")).toThrow(new Error(`Not found field "test" for content path "".`));
    })

    it("Validation", async () => {
        const content = new Content(new TestHost(), contentModel);
        const HeaderField = content.getField("Header");
        expect (HeaderField).toBeTruthy();

        MockResponse({ value: "test", errors: ["test error"] });
        await HeaderField?.saveValue("");
        expect(HeaderField?.errors).toEqual(["test error"]);

        expect (content.validate()).toBeTruthy();

        MockResponse({ value: "test", errors: [] });
        await HeaderField?.saveValue("");
        expect(HeaderField?.errors).toEqual([]);
        expect (content.validate()).toBeFalsy();
    })
})

class TestHost implements IContentHost {
    private __editor = ContentEditor.create((null as unknown as WebsiteApplication), editId);
    get editor(): ContentEditor {
        return this.__editor;
    }
    get isList(): boolean {
        return true;
    }
    attach(content: Content): void {
        return;
    }
}