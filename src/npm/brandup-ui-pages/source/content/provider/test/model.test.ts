import { ModelFieldProvider } from "../model";
import { MockedContent } from "../../../../mocks/content/content";
import { MockProviderValueResponse } from "../../../../mocks/content/provider/common";

const value = {
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
}

const contentModel = {
    type: "Model",
    name: "Banners",
    title: "Banners",
    isRequired: true,
    options: {
      isListValue: true,
      addText: null,
      itemType: null,
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
    },
    value: value,
    errors: []
};

const createProvider = () => {
    const content = new MockedContent();
    const provider = new ModelFieldProvider(content, contentModel);
    return provider;
}

describe('Model provider', () => {
    it("Success initialization", () => {
        const provider = createProvider();
        expect (provider).toBeInstanceOf(ModelFieldProvider);
    })

    it("Change value not implemented", async () => {
        const provider = createProvider();

        expect(provider.getValue()).toEqual(value);

        await expect(provider.saveValue("test")).rejects
        .toThrow(new Error("method saveValue not implemented in ModelFieldProvider"));
    })

    it("Items manipulations", async () => {
        const provider = createProvider();
        for (let i = 0; i < 3; i++) {
          const content = new MockedContent();
          (content as any).path = `test[${i}]`;
          provider.attach(content);
        }

        expect(provider.getItem(1).path).toEqual("test[1]");
        expect(() => provider.getItem(10)).toThrow(new Error("content by index 10 not found"));
    })
})
    

