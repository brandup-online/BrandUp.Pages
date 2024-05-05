using System.ComponentModel.DataAnnotations;

namespace BrandUp.Pages.Models
{
	public class PageCreateForm : FormModel<PageCreateValues>
	{
		public PageCollectionModel PageCollection { get; set; }
		public List<ComboBoxItem> PageTypes { get; set; }
	}

	public class PageCreateValues
	{
		[Required]
		public string PageType { get; set; }
		[Required(AllowEmptyStrings = false), MaxLength(255)]
		public string Header { get; set; }
	}
}