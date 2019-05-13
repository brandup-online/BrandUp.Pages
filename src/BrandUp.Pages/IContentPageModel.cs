namespace BrandUp.Pages
{
    public interface IContentPageModel
    {
        string Title { get; }
        string Description { get; }
        string Keywords { get; }
        string CssClass { get; }
        string ScriptName { get; }
    }
}