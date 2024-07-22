# BrandUp.Pages

[![Build status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status/BrandUp.Pages)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=8)

## Установка

Инфраструктура страниц добавляется через dependency injection.

Необходимо установить **NuGet** пакет [BrandUp.Pages](https://www.nuget.org/packages/BrandUp.Pages)

```
services.AddPages()
    .AddRazorContentPage()
    .AddContentTypesFromAssemblies(typeof(Startup).Assembly)
    .AddImageResizer<Infrastructure.ImageResizer>()
    .AddUserAccessProvider<Identity.RoleBasedAccessProvider>(ServiceLifetime.Scoped)
    .AddMongoDb<Models.AppDbContext>();
```

На строне фронта нужно добавить middleware и тип страницы.

Необходимо установить **NPM** пакеты:
- [brandup-ui-pages](https://www.npmjs.com/package/brandup-ui-pages)
- [@brandup/ui-website](https://www.npmjs.com/package/@brandup/ui-website)

```
import { host } from "@brandup/ui-website";
import { PagesMiddleware } from "brandup-ui-pages";

host.start({
    pageTypes: {
        "content": () => import("brandup-ui-pages/source/pages/content")
    }
}, (builder) => {
        builder.useMiddleware(new PagesMiddleware());
    });
```

## Модели данных

Модели данных делятся на два типа, модели страниц и модели контента.

## Регистрация моделей

Поиск моделей осуществляется с помощью IContentTypeLocator.

Если нужно выполнить поиск моделей в сборках, то воспользуйтесь методом:

```
services.AddPages()
    .AddContentTypesFromAssemblies(typeof(Startup).Assembly, ...)
```

### Модели страниц

Базовый тип модели страницы, определяет базовые свойства для всех унаследовавших типов страниц. Определён как 
abstract, чтобы его нельзя было использовать. Все остальные конечные типы наследуются от него.

```
[PageContent(Title = "Base page")]
public abstract class PageContent
{
    [Text(Placeholder = "Input page header"), Title]
    public string Header { get; set; }
}
```

Пример конечного типа модели страницы.

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

Все модели страниц должны быть помечены атрибутом **PageContent**.

### Модели контента

Модели контента страниц определяются по той же схеме, но помечаются атрибутом **ContentType**.

```
[ContentType]
public abstract class PageBlockContent
{
}

[ContentType(Title = "Блок с текстом")]
public abstract class TextBlockContent : PageBlockContent
{
    [Html(Placeholder = "Введите текст")]
    public string Text { get; set; }
}

[ContentType(Title = "Текст с заголовком и фоном")]
public class TB3 : TextBlockContent
{
    [Text]
    public string Header { get; set; }
    [Image]
    public ImageValue Background { get; set; }
}
```

## Представления моделей

Инициализация Razor представлений:

```
services.AddPages()
    .AddRazorContentPage();
``` 

Пример представления:

```
@inherits ContentPage<TextBlock.TB3>

<content-element tag="div" class="block-text tb3" script="BB1" />

<meta itemprop="image" content="@Model.Background" />
<i content-image="Background" class="image" />
<h2 content-text="Header" class="header" />
<div content-html="Text" class="text" />
```

Для настройки рендеринга используется элемент **content-element**:
- tag - тег обертки контента.
- class - CSS классы для тега.
- script - если нужно, то можно подключить скрипт с логикой.

## Права на редактирование

Редактирование структуры страниц и их контента происходит персонализованно и должно быть доступно не всем авторизованным пользователям.
Для управления этой возможностью необходимо реализовать интерфейс **IAccessProvider**.

```
public interface IAccessProvider
{
    Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default);
}
```

**GetUserIdAsync** - вовзращает идентификатор текущего пользователя.

**CheckAccessAsync** - проверяет наличие доступа к управлению контентом для текущего пользователя.