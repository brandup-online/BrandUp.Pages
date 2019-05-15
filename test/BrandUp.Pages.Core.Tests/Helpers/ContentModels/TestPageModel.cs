using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace BrandUp.Pages.ContentModels
{
    [PageContent(Title = ContentTypeTitle)]
    public class TestPageContent
    {
        public const string ContentTypeTitle = "Test page";

        [PageTitle, Text(Title = "Название страницы", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [Content(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }

        [Content(Title = "Шапки страницы")]
        public List<PageHeaderContent> Headers { get; set; }

        [PageCollection(Title = "Pages")]
        public PageCollectionReference<TestPageContent> Pages { get; set; }

        public static TestPageContent CreateWithOnlyTitle(string title)
        {
            return Create(title, null, null);
        }
        public static TestPageContent Create(string title, PageHeaderContent header, IEnumerable<PageHeaderContent> headers)
        {
            return new TestPageContent
            {
                Title = title,
                Header = header,
                Headers = headers != null ? new List<PageHeaderContent>(headers) : null
            };
        }
    }

    [PageContent(Title = "Article")]
    public class ArticlePageContent : TestPageContent
    {

    }

    [ContentType]
    public class PageHeaderContent
    {
        [Text(Title = "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [Content(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }
    }
}