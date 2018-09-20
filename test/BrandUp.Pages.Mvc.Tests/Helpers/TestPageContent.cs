using BrandUp.Pages.Content.Fields;
using BrandUp.Pages.Services;

namespace BrandUp.Pages.Mvc.Helpers
{
    [PageModel]
    public class TestPageContent
    {
        [View]
        public string ViewName { get; set; }
        [PageTitle, Text("Название", IsRequired = true)]
        public string Title { get; set; }
    }
}