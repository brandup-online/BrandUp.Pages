namespace BrandUp.Pages.Models
{
	public class ContentTypeModel
	{
		public string Name { get; set; } = null!;
		public string Title { get; set; } = null!;
	}

	public class ContentTypeListModel
	{
		public List<string>? Parents { get; set; }
	}

	public class ContentTypeItemModel
	{
		public string Name { get; set; } = null!;
		public string Title { get; set; } = null!;
		public bool IsAbstract { get; set; }
	}
}