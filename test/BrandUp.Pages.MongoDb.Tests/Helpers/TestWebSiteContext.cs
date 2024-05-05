using BrandUp.Website;

namespace BrandUp.Pages.MongoDb.Tests
{
	public class TestWebsiteContext : IWebsiteContext
	{
		public IWebsite Website { get; }
		public TimeZoneInfo TimeZone { get; }

		public TestWebsiteContext(string id, string name)
		{
			Website = new TestWebsite() { Id = id, Name = name, Title = name };
			TimeZone = TimeZoneInfo.Local;
		}
	}

	public class TestWebsite : IWebsite
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Title { get; set; }
	}
}