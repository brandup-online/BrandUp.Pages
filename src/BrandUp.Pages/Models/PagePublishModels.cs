using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Models
{
	public class PagePublishForm : FormModel<PagePublishValues>
	{
		public PageModel Page { get; set; } = null!;
	}

	public class PagePublishValues
	{
		[Required(AllowEmptyStrings = false), MaxLength(255)]
		public string Header { get; set; } = null!;
		[Required(AllowEmptyStrings = false), MaxLength(255)]
		public string UrlPath { get; set; } = null!;
	}

	public class PagePublishResult
	{
		public string Url { get; set; } = null!;
	}
}