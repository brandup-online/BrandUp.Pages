using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Models
{
	public class PagePublishForm : FormModel<PagePublishValues>
	{
		public PageModel Page { get; set; }
	}

	public class PagePublishValues
	{
		[Required(AllowEmptyStrings = false), MaxLength(255)]
		public string Header { get; set; }
		[Required(AllowEmptyStrings = false), MaxLength(255)]
		public string UrlPath { get; set; }
	}

	public class PagePublishResult
	{
		public string Url { get; set; }
	}
}