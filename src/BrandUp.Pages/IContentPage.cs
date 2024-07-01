namespace BrandUp.Pages
{
    public interface IContentPage<TContent>
        where TContent : class
    {
        string ContentKey { get; }
        TContent ContentModel { get; set; }
    }
}