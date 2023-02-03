using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;

namespace BrandUp.Pages
{
    [PageContent(Title = ContentTypeTitle)]
    public class TestPageContent
    {
        public const string ContentTypeTitle = "Test page";

        [Title, Text(Title = "Название страницы", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; }

        [Model(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }

        [Model(Title = "Шапки страницы")]
        public List<PageHeaderContent> Headers { get; set; }

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
                Headers = headers == null ? null : new List<PageHeaderContent>(headers)
            };
        }
    }

    [PageContent(Title = "Article"),]
    public class ArticlePageContent : TestPageContent
    {

    }

    [ContentType]
    public class PageHeaderContent
    {
        [Text(Title = "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [Model(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }
    }
}