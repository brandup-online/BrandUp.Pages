namespace BrandUp.Pages.Url
{
    public interface IPageUrlHelper
    {
        string NormalizeUrlPath(string urlPath);
        Result ValidateUrlPath(string urlPath);
        string GetDefaultPagePath();
        bool IsDefaultUrlPath(string urlPath);
        string ExtendUrlPath(string urlPath, string urlPathName);
    }
}