namespace BrandUp.Pages.Models
{
	public class PageListModel
	{
		public List<PagePathModel> Parents { get; set; }
		public List<PageCollectionModel> Collections { get; set; }
	}

	public class PagePathModel
	{
		public Guid Id { get; set; }
		public string Header { get; set; }
		public string Url { get; set; }
		public string Type { get; set; }
	}
}