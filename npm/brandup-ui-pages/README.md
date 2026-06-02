# @brandup/ui-pages

UI редактора контента для [BrandUp.Pages](https://github.com/brandup-online/BrandUp.Pages).

## Установка

```bash
npm i @brandup/ui-pages @brandup/ui-website
```

## Использование

```ts
import { WEBSITE } from "@brandup/ui-website";
import { ContentPage, pagesMiddleware } from "@brandup/ui-pages";

WEBSITE.run(
    {
        pages: {
            "content": { factory: () => Promise.resolve({ default: ContentPage }) }
        }
    },
    (builder) => builder.useMiddleware(pagesMiddleware));
```

DOM-хелперы импортируются из `@brandup/ui` (пакет `@brandup/ui-dom` объединён в `@brandup/ui`):

```ts
import { DOM } from "@brandup/ui";

const el = DOM.tag("div", { class: "box" }, "Hello");
```