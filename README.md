# BrandUp.Pages

[![Build status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status/BrandUp.Pages)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=8)

## ���������

�������������� ������� ����������� ����� dependency injection.

���������� ���������� **NuGet** ����� [BrandUp.Pages](https://www.nuget.org/packages/BrandUp.Pages)

```
services.AddPages()
    .AddContentTypesFromAssemblies(typeof(Startup).Assembly)
    .AddImageResizer<Infrastructure.ImageResizer>()
    .AddUserAccessProvider<Identity.RoleBasedAccessProvider>(ServiceLifetime.Scoped)
    .AddMongoDb<Models.AppDbContext>()
    .AddRootPages()
    .AddItemPages<Models.BlogPostDocument, BlogPostItemProvider>("/Blog/Post");
```

�� ������ ������ ����� �������� middleware � ��� ��������.

���������� ���������� **NPM** ������:
- [brandup-ui-pages](https://www.npmjs.com/package/brandup-ui-pages)
- [brandup-ui-website](https://www.npmjs.com/package/brandup-ui-website)

```
import { host } from "brandup-ui-website";
import { PagesMiddleware } from "brandup-ui-pages";

host.start({
    pageTypes: {
        "content": () => import("brandup-ui-pages/source/pages/content")
    }
}, (builder) => {
        builder.useMiddleware(new PagesMiddleware());
    });
```

## ������ ������

������ ������ ������� �� ��� ����, ������ ������� � ������ ��������.

### ����������� �������

����� ������� �������������� � ������� IContentTypeLocator.

���� ����� ��������� ����� ������� � �������, �� �������������� �������:

```
services.AddPages()
    .AddContentTypesFromAssemblies(typeof(Startup).Assembly, ...)
```

### ������ �������

������� ��� ������ ��������, ���������� ������� �������� ��� ���� �������������� ����� �������. �������� ��� 
abstract, ����� ��� ������ ���� ������������. ��� ��������� �������� ���� ����������� �� ����.

```
[PageContent(Title = "Base page")]
public abstract class PageContent
{
    [Text(Placeholder = "Input page header"), Title]
    public string Header { get; set; }
}
```

������ ��������� ���� ������ ��������.

```
[PageContent(Title = "Article page")]
public class ArticlePageContent : PageContent
{
    [Text(Placeholder = "Input page sub header")]
    public string SubHeader { get; set; }

    [Model]
    public List<PageBlockContent> Blocks { get; set; }
}
```

��� ������ ������� ������ ���� �������� ��������� **PageContent**.

### ������ ��������

������ �������� ������� ������������ �� ��� �� �����, �� ���������� ��������� **ContentType**.

```
[ContentType]
public abstract class PageBlockContent
{
}

[ContentType(Title = "���� � �������")]
public abstract class TextBlockContent : PageBlockContent
{
    [Html(Placeholder = "������� �����")]
    public string Text { get; set; }
}

[ContentType(Title = "����� � ���������� � �����")]
public class TB3 : TextBlockContent
{
    [Text]
    public string Header { get; set; }
    [Image]
    public ImageValue Background { get; set; }
}
```

## ������������� �������

������ �������������:

```
@inherits ContentRazorPage<TextBlock.TB3>

<content-element tag="div" class="block-text tb3" script="BB1" />

<meta itemprop="image" content="@Model.Background" />
<i content-image="Background" class="image" />
<h2 content-text="Header" class="header" />
<div content-html="Text" class="text" />
```

��� ��������� ���������� ������������ ������� **content-element**:
- tag - ��� ������� ��������.
- class - CSS ������ ��� ����.
- script - ���� �����, �� ����� ���������� ������ � �������.

## ����� �� ��������������

�������������� ��������� ������� � �� �������� ���������� ���������������� � ������ ���� �������� �� ���� �������������� �������������.
��� ���������� ���� ������������ ���������� ����������� ��������� **IAccessProvider**.

```
public interface IAccessProvider
{
    Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default);
}
```

**GetUserIdAsync** - ���������� ������������� �������� ������������.

**CheckAccessAsync** - ��������� ������� ������� � ���������� ��������� ��� �������� ������������.