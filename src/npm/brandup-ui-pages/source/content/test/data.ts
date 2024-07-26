export const editId = "77ace56a-0429-4258-a342-0e61159f082d";

export const initBodyHtml = `<div class="page-article root-designer" 
    data-content-edit-id="780c4915-0f79-43bb-a77e-5e6582cf36f4" data-content-path="" 
    data-content-root="website-page-650fe519-c190-4f1f-af96-11d882c5dfef"
    data-content-title="Common page" data-content-type="CommonPage"></div>`;

export const contentResponse = {
  contentKey: "website-page-650fe519-c190-4f1f-af96-11d882c5dfef",
  path: "",
  contents: [
    {
      parentPath: null,
      parentField: null,
      path: "",
      index: -1,
      typeName: "CommonPage",
      typeTitle: "Common page",
      fields: [
        {
          type: "Text",
          name: "Header",
          title: "Header",
          isRequired: true,
          options: {
            allowMultiline: false,
            placeholder: "Input page header",
          },
          value: "Home page",
          errors: [],
        },
        {
          type: "Model",
          name: "Blocks",
          title: "Blocks",
          isRequired: false,
          options: {
            isListValue: true,
            addText: null,
            itemType: null,
            itemTypes: [
              {
                name: "TB1",
                title: "Только текст",
              },
              {
                name: "TB2",
                title: "Текст с заголовком",
              },
              {
                name: "TB3",
                title: "Текст с заголовком и фоном",
              },
              {
                name: "BB1",
                title: "Слайдер баннеров",
              },
            ],
          },
          value: {
            items: [
              {
                title: "Текст с заголовком",
                type: {
                  name: "TB2",
                  title: "Текст с заголовком",
                },
              },
            ],
          },
          errors: [],
        },
      ],
    },
    {
      parentPath: "",
      parentField: "Blocks",
      path: "Blocks[0]",
      index: 0,
      typeName: "TB2",
      typeTitle: "Текст с заголовком",
      fields: [
        {
          type: "Html",
          name: "Text",
          title: "Text",
          isRequired: true,
          options: {
            placeholder: "Введите текст",
          },
          value: "<p>Test test test test test</p>",
          errors: [],
        },
        {
          type: "Text",
          name: "Header",
          title: "Header",
          isRequired: true,
          options: {
            allowMultiline: false,
            placeholder: null,
          },
          value: "Test",
          errors: [],
        },
      ],
    },
  ],
};

export const contentModel = {
  parentPath: "null",
  parentField: "null",
  path: "",
  index: -1,
  typeName: "CommonPage",
  typeTitle: "Common page",
  fields: [
    {
      type: "Text",
      name: "Header",
      title: "Header",
      isRequired: true,
      options: {
        allowMultiline: false,
        placeholder: "Input page header",
      },
      value: "Home page",
      errors: [],
    },
    {
      type: "Model",
      name: "Blocks",
      title: "Blocks",
      isRequired: false,
      options: {
        isListValue: true,
        addText: null,
        itemType: null,
        itemTypes: [
          {
            name: "TB1",
            title: "Только текст",
          },
          {
            name: "TB2",
            title: "Текст с заголовком",
          },
          {
            name: "TB3",
            title: "Текст с заголовком и фоном",
          },
          {
            name: "BB1",
            title: "Слайдер баннеров",
          },
        ],
      },
      value: {
        items: [
          {
            title: "Текст с заголовком",
            type: {
              name: "TB2",
              title: "Текст с заголовком",
            },
          },
        ],
      },
      errors: [],
    },
  ],
};
