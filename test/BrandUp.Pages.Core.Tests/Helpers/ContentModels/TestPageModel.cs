using System.ComponentModel.DataAnnotations;
using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Views;

namespace BrandUp.Pages.ContentModels
{
    [PageContent(Title = ContentTypeTitle), View]
    [DefaultValue(nameof(Title), ContentTypeTitle)]
    public class TestPageContent
    {
        public const string ContentTypeTitle = "Test page";

        [Title, Text(Title = "Название страницы", AllowMultiline = false, Placeholder = "Укажите название"), Required]
        public string Title { get; set; }

        [Model(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }

        [Model(Title = "Шапки страницы")]
        public List<PageHeaderContent> Headers { get; set; }

        [Pages(Title = "Pages")]
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

    [PageContent(Title = "Article"), View]
    public class ArticlePageContent : TestPageContent
    {

    }

    [ContentType]
    public class PageHeaderContent
    {
        [Text(Title = "Название", AllowMultiline = false, Placeholder = "Укажите название"), Required]
        public string Title { get; set; } = "Test";

        [Model(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }
    }
}