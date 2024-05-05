namespace BrandUp.Pages.Content
{
	public static class IContentMetadataManagerExtensions
	{
		public static ContentMetadataProvider GetMetadata<T>(this IContentMetadataManager contentMetadataManager)
			where T : class
		{
			return contentMetadataManager.GetMetadata(typeof(T));
		}

		public static ContentMetadataProvider GetMetadata(this IContentMetadataManager contentMetadataManager, object model)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));
			return contentMetadataManager.GetMetadata(model.GetType());
		}

		public static bool TryGetMetadata(this IContentMetadataManager contentMetadataManager, object model, out ContentMetadataProvider contentMetadataProvider)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));
			return contentMetadataManager.TryGetMetadata(model.GetType(), out contentMetadataProvider);
		}

		public static ContentMetadataProvider GetMetadataByModelData(this IContentMetadataManager contentMetadataManager, IDictionary<string, object> modelData)
		{
			if (modelData == null)
				throw new ArgumentNullException(nameof(modelData));
			if (!modelData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out object typeNameValue))
				return null;

			contentMetadataManager.TryGetMetadata((string)typeNameValue, out ContentMetadataProvider contentMetadata);
			return contentMetadata;
		}

		public static void ApplyInjections(this IContentMetadataManager contentMetadataManager, object model, IServiceProvider serviceProvider, bool injectInnerModels)
		{
			if (model == null)
				throw new ArgumentNullException(nameof(model));
			if (serviceProvider == null)
				throw new ArgumentNullException(nameof(serviceProvider));
			if (!contentMetadataManager.TryGetMetadata(model, out ContentMetadataProvider contentMetadataProvider))
				throw new ArgumentException();

			contentMetadataProvider.ApplyInjections(model, serviceProvider, injectInnerModels);
		}
	}
}