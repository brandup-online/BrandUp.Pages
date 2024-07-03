namespace BrandUp.Pages.Content
{
    public static class ContentMetadataManagerExtensions
    {
        public static ContentMetadata GetMetadata<T>(this ContentMetadataManager contentMetadataManager)
            where T : class
        {
            return contentMetadataManager.GetMetadata(typeof(T));
        }

        public static ContentMetadata GetMetadata(this ContentMetadataManager contentMetadataManager, IDictionary<string, object> contentData)
        {
            ArgumentNullException.ThrowIfNull(contentData);

            if (!contentData.TryGetValue(ContentMetadata.ContentTypeNameDataKey, out var contentTypeName))
                throw new ArgumentException($"Not found content type name.", nameof(contentData));

            return GetMetadata(contentMetadataManager, (string)contentTypeName);
        }

        public static ContentMetadata GetMetadata(this ContentMetadataManager contentMetadataManager, object model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return contentMetadataManager.GetMetadata(model.GetType());
        }

        public static ContentMetadata GetMetadata(this ContentMetadataManager contentMetadataManager, string typeName)
        {
            ArgumentNullException.ThrowIfNull(typeName);

            if (!contentMetadataManager.TryGetMetadata(typeName, out var contentMetadata))
                throw new InvalidOperationException($"Not found content type by name {typeName}.");

            return contentMetadata;
        }

        public static bool TryGetMetadata(this ContentMetadataManager contentMetadataManager, object model, out ContentMetadata contentMetadataProvider)
        {
            ArgumentNullException.ThrowIfNull(model);

            return contentMetadataManager.TryGetMetadata(model.GetType(), out contentMetadataProvider);
        }

        public static ContentMetadata GetMetadataByModelData(this ContentMetadataManager contentMetadataManager, IDictionary<string, object> modelData)
        {
            ArgumentNullException.ThrowIfNull(modelData);

            if (!modelData.TryGetValue(ContentMetadata.ContentTypeNameDataKey, out object typeNameValue))
                return null;

            contentMetadataManager.TryGetMetadata((string)typeNameValue, out ContentMetadata contentMetadata);
            return contentMetadata;
        }

        public static void ApplyInjections(this ContentMetadataManager contentMetadataManager, object model, IServiceProvider serviceProvider, bool injectInnerModels)
        {
            ArgumentNullException.ThrowIfNull(model);
            ArgumentNullException.ThrowIfNull(serviceProvider);
            if (!contentMetadataManager.TryGetMetadata(model, out ContentMetadata contentMetadataProvider))
                throw new ArgumentException($"Type {model.GetType().AssemblyQualifiedName} is not registered as content type.");

            contentMetadataProvider.ApplyInjections(model, serviceProvider, injectInnerModels);
        }
    }
}