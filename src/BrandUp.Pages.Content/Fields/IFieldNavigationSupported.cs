namespace BrandUp.Pages.Content.Fields
{
    public interface IFieldNavigationSupported
    {
        bool IsList { get; }
        object Navigate(object value, int index);
    }
}