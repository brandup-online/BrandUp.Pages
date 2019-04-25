using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace BrandUp.Pages.ContentModels
{
    [PageContentModel(Title = ContentTypeTitle)]
    public class TestPageContent
    {
        public const string ContentTypeTitle = "Test page";

        [PageTitle, Text(title: "Название страницы", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [ContentValue(title: "Шапка страницы")]
        public PageHeaderContent Header { get; set; }

        [ContentList(title: "Шапки страницы")]
        public List<PageHeaderContent> Headers { get; set; }

        [PageCollection("Pages")]
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

    [ContentModel]
    public class PageHeaderContent
    {
        [Text(title: "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [ContentValue(title: "Шапка страницы")]
        public PageHeaderContent Header { get; set; }
    }
}