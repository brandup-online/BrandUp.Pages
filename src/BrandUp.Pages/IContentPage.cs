using BrandUp.Pages.Content.Items;

namespace BrandUp.Pages
{
    public interface IContentPage<TContent>
        where TContent : class
    {
        string ContentKey { get; }
        TContent ContentModel { get; set; }
    }

    public interface IContentPage<TItem, TContent>
        where TItem : IItemContent
        where TContent : class
    {
        TItem ContentItem { get; }
        TContent ContentModel { get; set; }
    }

    internal class ContentPageContext { }
}