namespace BrandUp.Pages.Content.Fields
{
    public interface IFieldNavigationSupported
    {
        bool IsListValue { get; }
        object Navigate(object value, int index);
    }
}