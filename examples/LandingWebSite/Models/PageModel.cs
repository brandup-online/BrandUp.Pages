using System;
using System.ComponentModel.DataAnnotations;

namespace LandingWebSite.Models
{
    public class PageModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
    }

    public class PageCreateModel
    {
        [Required]
        public string PageType { get; set; }
        [Required(AllowEmptyStrings = false), MaxLength(255)]
        public string Title { get; set; }
    }
}