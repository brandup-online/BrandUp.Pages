using BrandUp.Pages.Content.Items;

namespace BrandUp.Pages
{
    public interface IContentPage<TContent>
        where TContent : class
    {
        TContent ContentModel { get; set; }
    }

    public interface IStaticContentPage<TContent> : IContentPage<TContent>
        where TContent : class
    {
        string ContentKey { get; }
    }

    public interface IItemContentPage<TItem, TContent> : IContentPage<TContent>
        where TItem : IItemContent
        where TContent : class
    {
        TItem ContentItem { get; }
    }

    internal class ContentPageContext { }
}