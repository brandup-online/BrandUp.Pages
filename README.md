# BrandUp.Pages

[![Build status](https://dev.azure.com/brandup/BrandUp%20Core/_apis/build/status/BrandUp.Pages)](https://dev.azure.com/brandup/BrandUp%20Core/_build/latest?definitionId=8)

Система управления контентом (CMS) для сайтов на ASP.NET Core: строго типизированный
контент страниц, редактор контента прямо в браузере, коллекции страниц, SEO-поля,
работа с файлами и изображениями, хранение в MongoDB.

Бэкенд — набор .NET-библиотек, редактор — TypeScript-пакет. В репозитории есть
пример сайта (`LandingWebSite`), который связывает всё вместе.

## Требования

- .NET SDK **10.0**
- Node.js (LTS) и npm
- MongoDB — для запуска примера сайта. Локального инстанса достаточно;
  интеграционные тесты поднимают собственный временный MongoDB (EphemeralMongo).

## Структура репозитория

```
src/
  BrandUp.Pages.Core        — домен: страницы, коллекции, метаданные контента, URL
  BrandUp.Pages.Content     — модель контента, поля, JSON-сериализация
  BrandUp.Pages             — интеграция с ASP.NET Core: контроллеры, tag-helpers, Razor
  BrandUp.Pages.MongoDb     — репозитории MongoDB и хранение файлов в GridFS
  BrandUp.Pages.Testing     — in-memory фейки для модульных тестов
  LandingWebSite            — пример сайта
test/
  BrandUp.Pages.Core.Tests
  BrandUp.Pages.Content.Tests
  BrandUp.Pages.MongoDb.Tests   — использует EphemeralMongo (внешний MongoDB не нужен)
  BrandUp.Pages.Tests
npm/
  brandup-ui-pages          — пакет редактора (@brandup/ui-pages, TypeScript)
  brandup-ui-pages-example  — webpack-сборка редактора для примера сайта
```

## Быстрый старт

### 1. Бэкенд

```bash
dotnet restore
dotnet build
```

### 2. Фронтенд

```bash
# корень: установка воркспейсов и сборка пакета @brandup/ui-pages
npm install
npm run build

# сборка бандла для примера сайта (webpack)
cd npm/brandup-ui-pages-example
npm install
npm run build      # production-бандл
npm run watch      # пересборка при изменениях (разработка)
```

### 3. Запуск примера сайта

Строка подключения к MongoDB настраивается в `src/LandingWebSite/appsettings.json`
(секция `MongoDb`).

```bash
dotnet run --project src/LandingWebSite
```

В VS Code есть конфигурация запуска **«Run LandingWebSite (+ client watch)»** —
она стартует webpack watch, а затем запускает сайт.

### Тесты

```bash
dotnet test    # .NET (MongoDb-тесты используют временный сервер)
npm test       # фронтенд (jest)
```

## Установка через NuGet/NPM

**NuGet:** [BrandUp.Pages](https://www.nuget.org/packages/BrandUp.Pages)
**NPM:** [@brandup/ui-pages](https://www.npmjs.com/package/@brandup/ui-pages),
[@brandup/ui-website](https://www.npmjs.com/package/@brandup/ui-website)

### Регистрация сервисов

Инфраструктура страниц подключается через dependency injection
(полный пример — `src/LandingWebSite/Program.cs`):

```csharp
services.AddPages()
    .AddRazorContentPage()
    .AddContentTypesFromAssemblies(typeof(Program).Assembly)
    .AddImageResizer<Infrastructure.ImageResizer>()
    .AddUserAccessProvider<Identity.RoleBasedAccessProvider>(ServiceLifetime.Scoped)
    .AddMongoDb<Models.AppDbContext>();
```

### Фронтенд

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

## Модели данных

Модели делятся на два типа: **модели страниц** и **модели контента**.
Поиск моделей выполняется через `IContentTypeLocator`; для поиска в сборках:

```csharp
services.AddPages()
    .AddContentTypesFromAssemblies(typeof(Program).Assembly, ...);
```

### Модели страниц

Базовый тип определяет общие свойства для всех страниц и обычно объявляется
`abstract`. Все модели страниц помечаются атрибутом `[PageContent]`.

```csharp
[PageContent(Title = "Base page")]
public abstract class PageContent
{
    [Text(Placeholder = "Input page header"), Title]
    public string Header { get; set; }
}

[PageContent(Title = "Article page")]
public class ArticlePageContent : PageContent
{
    [Text(Placeholder = "Input page sub header")]
    public string SubHeader { get; set; }

    [Model]
    public List<PageBlockContent> Blocks { get; set; }
}
```

### Модели контента

Определяются по той же схеме, но помечаются атрибутом `[ContentType]`.

```csharp
[ContentType]
public abstract class PageBlockContent { }

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

Доступные поля контента: `Text`, `Html`, `Image`, `HyperLink`, `Model`
(вложенные модели), `Pages` (ссылки на страницы).

## Представления (Razor)

Инициализация представлений:

```csharp
services.AddPages()
    .AddRazorContentPage();
```

Пример представления:

```cshtml
@inherits ContentPage<TextBlock.TB3>

<content-element tag="div" class="block-text tb3" script="BB1" />

<meta itemprop="image" content="@Model.Background" />
<i content-image="Background" class="image" />
<h2 content-text="Header" class="header" />
<div content-html="Text" class="text" />
```

Элемент `content-element` настраивает рендеринг обёртки контента:
- `tag` — тег обёртки;
- `class` — CSS-классы;
- `script` — подключаемый скрипт с логикой (опционально).

## Права на редактирование

Редактирование структуры и контента доступно не всем авторизованным
пользователям. Для управления доступом реализуйте `IAccessProvider`:

```csharp
public interface IAccessProvider
{
    Task<string> GetUserIdAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckAccessAsync(CancellationToken cancellationToken = default);
}
```

- `GetUserIdAsync` — идентификатор текущего пользователя;
- `CheckAccessAsync` — есть ли у текущего пользователя доступ к управлению контентом.

Провайдер регистрируется через `.AddUserAccessProvider<T>(...)`.

## Лицензия

Распространяется под лицензией [Apache-2.0](LICENSE).
