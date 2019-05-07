namespace BrandUp.Pages.Models
{
    public class FormModel<TValues>
        where TValues : class, new()
    {
        public TValues Values { get; set; } = new TValues();
    }
}