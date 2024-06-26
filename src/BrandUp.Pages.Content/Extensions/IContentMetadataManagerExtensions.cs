namespace BrandUp.Pages.Content
{
    public static class IContentMetadataManagerExtensions
    {
        public static ContentMetadataProvider GetMetadata<T>(this ContentMetadataManager contentMetadataManager)
            where T : class
        {
            return contentMetadataManager.GetMetadata(typeof(T));
        }

        public static ContentMetadataProvider GetMetadata(this ContentMetadataManager contentMetadataManager, IDictionary<string, object> contentData)
        {
            ArgumentNullException.ThrowIfNull(contentData);

            if (!contentData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out var contentTypeName))
                throw new ArgumentException($"Not found content type name.", nameof(contentData));

            if (!contentMetadataManager.TryGetMetadata((string)contentTypeName, out var metadata))
                throw new InvalidOperationException($"Not found content type by name {contentTypeName}.");

            return metadata;
        }

        public static ContentMetadataProvider GetMetadata(this ContentMetadataManager contentMetadataManager, object model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return contentMetadataManager.GetMetadata(model.GetType());
        }

        public static bool TryGetMetadata(this ContentMetadataManager contentMetadataManager, object model, out ContentMetadataProvider contentMetadataProvider)
        {
            ArgumentNullException.ThrowIfNull(model);

            return contentMetadataManager.TryGetMetadata(model.GetType(), out contentMetadataProvider);
        }

        public static ContentMetadataProvider GetMetadataByModelData(this ContentMetadataManager contentMetadataManager, IDictionary<string, object> modelData)
        {
            ArgumentNullException.ThrowIfNull(modelData);

            if (!modelData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out object typeNameValue))
                return null;

            contentMetadataManager.TryGetMetadata((string)typeNameValue, out ContentMetadataProvider contentMetadata);
            return contentMetadata;
        }

        public static void ApplyInjections(this ContentMetadataManager contentMetadataManager, object model, IServiceProvider serviceProvider, bool injectInnerModels)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(serviceProvider);
            if (!contentMetadataManager.TryGetMetadata(model, out ContentMetadataProvider contentMetadataProvider))
                throw new ArgumentException($"Type {model.GetType().AssemblyQualifiedName} is not registered as content type.");

            contentMetadataProvider.ApplyInjections(model, serviceProvider, injectInnerModels);
        }
    }
}