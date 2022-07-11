# brandup-pages-ui

## Setup

```
import { host } from "brandup-ui-website";
import { PagesMiddleware } from "brandup-ui-pages";

host.start({
    pageTypes: {
        "content": () => import("brandup-pages-ui/source/pages/content")
    }
}, (builder) => {
        builder.useMiddleware(new PagesMiddleware());
    });
```