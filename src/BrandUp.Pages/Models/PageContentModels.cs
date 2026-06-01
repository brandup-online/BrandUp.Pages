namespace BrandUp.Pages.Models
{
	public class PageContentForm
	{
		public PageContentPath Path { get; set; } = null!;
		public List<ContentFieldModel> Fields { get; } = new List<ContentFieldModel>();
		public Dictionary<string, object?> Values { get; } = new Dictionary<string, object?>();
	}

	public class PageContentPath
	{
		public PageContentPath? Parent { get; set; }
		public string ModelPath { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Title { get; set; } = null!;
		public int Index { get; set; }
	}

	public class ContentFieldModel
	{
		public string Type { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Title { get; set; } = null!;
		public object? Options { get; set; }
	}
}