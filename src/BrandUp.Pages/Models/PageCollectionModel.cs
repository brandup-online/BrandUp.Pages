using System.Text.Json.Serialization;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Models
{
	public class PageCollectionModel
	{
		public Guid Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public Guid? PageId { get; set; }
		public string Title { get; set; } = null!;
		public string PageType { get; set; } = null!;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public PageSortMode Sort { get; set; }
		public bool CustomSorting { get; set; }
		public string PageUrl { get; set; } = null!;
	}
}