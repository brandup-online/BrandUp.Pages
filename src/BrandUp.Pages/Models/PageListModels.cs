namespace BrandUp.Pages.Models
{
	public class PageListModel
	{
		public List<PagePathModel> Parents { get; set; } = null!;
		public List<PageCollectionModel> Collections { get; set; } = null!;
	}

	public class PagePathModel
	{
		public Guid Id { get; set; }
		public string Header { get; set; } = null!;
		public string Url { get; set; } = null!;
	}
}