using BrandUp.Pages.Content;
using BrandUp.Pages.Content.Fields;
using System.Collections.Generic;

namespace BrandUp.Pages.ContentModels
{
    [Content(Title = ContentTypeTitle, Description = ContentTypeDescription)]
    public class TestPageContent
    {
        public const string ContentTypeTitle = "Test page";
        public const string ContentTypeDescription = "Test page description";

        [Text(Title = "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";

        [ContentValue(Title = "Шапка страницы")]
        public PageHeaderContent Header { get; set; }

        [ContentList(Title = "Шапки страницы")]
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
                Headers = headers != null ? new List<PageHeaderContent>(headers) : null
            };
        }
    }

    [Content(Title = "Заголовок", Description = "Заголовок страницы")]
    public class PageHeaderContent
    {
        [Text(Title = "Название", IsRequired = true, AllowMultiline = false, Placeholder = "Укажите название")]
        public string Title { get; set; } = "Test";
    }
}