using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Models
{
    public class PageSeoForm : FormModel<PageSeoValues>
    {
        public PageModel Page { get; set; }
    }

    public class PageSeoValues
    {
        [MaxLength(255)]
        public string Title { get; set; }
        [MaxLength(255)]
        public string Description { get; set; }
        public string[] Keywords { get; set; }
    }
}