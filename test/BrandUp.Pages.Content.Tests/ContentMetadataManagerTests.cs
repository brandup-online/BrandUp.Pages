using BrandUp.Pages.ContentModels;
using Microsoft.Extensions.DependencyInjection;

namespace BrandUp.Pages.Content
{
	public class ContentMetadataManagerTests
	{
		private readonly ContentMetadataManager metadataManager;

		public ContentMetadataManagerTests()
		{
			var contentTypeResolver = new Infrastructure.AssemblyContentTypeLocator(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

			metadataManager = new ContentMetadataManager(contentTypeResolver);
		}

		#region Test methods

		[Fact]
		public void IsRegisterdContentType()
		{
			Assert.True(metadataManager.IsRegisterdContentType(typeof(TestPageContent)));
		}

		[Fact]
		public void GetMetadata()
		{
			var contentType = typeof(TestPageContent);
			var contentMetadata = metadataManager.GetMetadata(contentType);

			Assert.NotNull(contentMetadata);
			Assert.Equal(contentMetadata.ModelType, contentType);
			Assert.Equal("TestPage", contentMetadata.Name);
			Assert.Equal(TestPageContent.ContentTypeTitle, contentMetadata.Title);
		}

		[Fact]
		public void GetMetadata_Generic()
		{
			var contentMetadata = metadataManager.GetMetadata<TestPageContent>();

			Assert.NotNull(contentMetadata);
			Assert.Equal(typeof(TestPageContent), contentMetadata.ModelType);
			Assert.Equal("TestPage", contentMetadata.Name);
			Assert.Equal(TestPageContent.ContentTypeTitle, contentMetadata.Title);
		}

		[Fact]
		public void TryGetMetadata()
		{
			var contentType = typeof(TestPageContent);
			var contentMetadata = metadataManager.GetMetadata(contentType);

			Assert.True(metadataManager.TryGetMetadata(contentType, out ContentMetadataProvider contentMetadata2));
			Assert.Equal(contentMetadata, contentMetadata2);
		}

		[Fact]
		public void GetAllMetadata()
		{
			var contentType = typeof(TestPageContent);
			var contentMetadata = metadataManager.GetMetadata(contentType);

			var metadatas = metadataManager.MetadataProviders;

			Assert.NotNull(metadatas);
			Assert.True(metadatas.Any());
			Assert.Contains(contentMetadata, metadatas);
		}

		[Fact]
		public void GetDerivedMetadataWithHierarhy()
		{
			var contentType = typeof(TestPageContent);
			var contentMetadata = metadataManager.GetMetadata(contentType);

			var contentMetadatas = contentMetadata.GetDerivedMetadataWithHierarhy(true).ToList();
			Assert.True(contentMetadatas.Count > 0);
			Assert.Contains(contentMetadata, contentMetadatas);
		}

		[Fact]
		public void ApplyInjections()
		{
			var services = new ServiceCollection().AddScoped<TestService>();
			using var serviceProvider = services.BuildServiceProvider();
			using var serviceScope = serviceProvider.CreateScope();
			var page = TestPageContent.Create("test", new PageHeaderContent(), new List<PageHeaderContent> { new PageHeaderContent() });

			metadataManager.ApplyInjections(page, serviceScope.ServiceProvider, false);

			Assert.NotNull(page.Service);
			Assert.Null(page.Header.Service);
			Assert.Null(page.Headers[0].Service);
		}

		[Fact]
		public void ApplyInjections_WithInnerModels()
		{
			var services = new ServiceCollection().AddScoped<TestService>();
			using var serviceProvider = services.BuildServiceProvider();
			using var serviceScope = serviceProvider.CreateScope();
			var page = TestPageContent.Create("test", new PageHeaderContent(), new List<PageHeaderContent> { new PageHeaderContent() });

			metadataManager.ApplyInjections(page, serviceScope.ServiceProvider, true);

			Assert.NotNull(page.Service);
			Assert.NotNull(page.Header.Service);
			Assert.NotNull(page.Headers[0].Service);
		}

		#endregion
	}
}