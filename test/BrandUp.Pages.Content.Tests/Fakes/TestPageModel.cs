using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace BrandUp.Pages.ContentModels
{
    [ContentType(Title = ContentTypeTitle, Description = ContentTypeDescription)]
    public class TestPageContent
    {
        public const string ContentTypeTitle = "Test page";
        public const string ContentTypeDescription = "Test page description";

        [Title, Text(Title = "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [Model(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }

        [Model(Title = "Шапки страницы")]
        public List<PageHeaderContent> Headers { get; set; }

        [ContentInject]
        public TestService Service { get; set; }

        public static TestPageContent CreateWithOnlyTitle(string title)
        {
            return Create(title, null, null);
        }
        public static TestPageContent Create(string title, PageHeaderContent header = null, IEnumerable<PageHeaderContent> headers = null)
        {
            return new TestPageContent
            {
                Title = title,
                Header = header,
                Headers = headers != null ? new List<PageHeaderContent>(headers) : null
            };
        }
    }

    [ContentType(Title = "Article")]
    public class ArticlePage : TestPageContent
    {

    }

    [ContentType(Title = "News")]
    public class NewsPage : ArticlePage
    {

    }

    [ContentType(Title = "Заголовок", Description = "Заголовок страницы")]
    public class PageHeaderContent
    {
        [Text(Title = "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [ContentInject]
        public TestService Service { get; set; }
    }
}