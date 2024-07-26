import { Content } from "../../content/content";
import { ContentEditor } from "../../content/editor"
import { WebsiteApplication } from "@brandup/ui-website";
import { MockProviderValueResponse } from "../../../mocks/common";
import { contentResponse, editId, initBodyHtml } from "./data";

export const createEditor = () => {
    document.body.innerHTML = initBodyHtml;
    document.body.classList.remove("bp-state-design");
    return ContentEditor.create((null as unknown as WebsiteApplication), editId);
}

describe('Editor', () => {
    it("Success initialization", () => {
        const editor = createEditor();
        expect (editor).toBeInstanceOf(ContentEditor);
    })
    
    it("Error start edit", () => {
        const editor = createEditor();
        document.body.classList.add("bp-state-design");
    
        expect(() => editor.edit()).toThrow(new Error("Content editor already started."));
    })
    
    it("Success start edit", async () => {
        const editor = createEditor();
    
        MockProviderValueResponse(contentResponse);
        editor.edit().then(data => {
            expect(data).toBeInstanceOf(Content)
            expect(editor.navigate("Blocks")).toBeTruthy();
            expect(editor.navigate("Header")).toBeTruthy();
        });
    })
})