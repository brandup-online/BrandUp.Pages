namespace BrandUp.Pages
{
    public abstract class PageSetModel<TContent, TItem> : ContentPageModel<TContent>
        where TContent : class
        where TItem : class
    {
    }
}