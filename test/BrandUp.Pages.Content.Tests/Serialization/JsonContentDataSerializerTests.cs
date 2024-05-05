using BrandUp.Pages.ContentModels;

namespace BrandUp.Pages.Content.Serialization
{
	public class JsonContentDataSerializerTests
	{
		private readonly IContentMetadataManager metadataManager;

		public JsonContentDataSerializerTests()
		{
			var contentTypeResolver = new Infrastructure.AssemblyContentTypeLocator(new System.Reflection.Assembly[] { typeof(TestPageContent).Assembly });

			metadataManager = new ContentMetadataManager(contentTypeResolver);
		}

		#region Test methods

		[Fact]
		public void SerializeToString()
		{
			var content = new TestPageContent
			{
				Title = "page",
				Header = new PageHeaderContent { Title = "header" },
				Headers = new List<PageHeaderContent> { new PageHeaderContent { Title = "header" } }
			};
			var contentMetadata = metadataManager.GetMetadata(content.GetType());
			var contentData = contentMetadata.ConvertContentModelToDictionary(content);

			var json = JsonContentDataSerializer.SerializeToString(contentData);
			Assert.NotNull(json);
		}

		[Fact]
		public void DeserializeFromString()
		{
			var content = new TestPageContent
			{
				Title = "page",
				Header = new PageHeaderContent { Title = "header" },
				Headers = new List<PageHeaderContent> { new PageHeaderContent { Title = "header" } }
			};
			var contentMetadata = metadataManager.GetMetadata(content.GetType());
			var contentData = contentMetadata.ConvertContentModelToDictionary(content);
			var json = JsonContentDataSerializer.SerializeToString(contentData);

			var deserializedContentData = JsonContentDataSerializer.DeserializeFromString(json);
			var deserializedContent = (TestPageContent)contentMetadata.ConvertDictionaryToContentModel(deserializedContentData);

			Assert.NotNull(deserializedContent);
			Assert.Equal(deserializedContent.Title, content.Title);
			Assert.Equal(deserializedContent.Header.Title, content.Header.Title);
			Assert.Equal(deserializedContent.Headers[0].Title, content.Headers[0].Title);
		}

		#endregion
	}
}