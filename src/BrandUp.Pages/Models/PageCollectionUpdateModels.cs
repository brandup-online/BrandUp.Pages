using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Models
{
	public class PageCollectionUpdateForm : FormModel<PageCollectionUpdateValues>
	{
		public PageCollectionModel PageCollection { get; set; } = null!;
		public List<ComboBoxItem> Sorts { get; set; } = null!;
	}

	public class PageCollectionUpdateValues
	{
		[Required(AllowEmptyStrings = false), MaxLength(150)]
		public string Title { get; set; } = null!;
		[Required]
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PageSortMode Sort { get; set; }
	}
}