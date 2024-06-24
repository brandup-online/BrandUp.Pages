using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Services
{
    public class ContentService(IContentRepository contentRepository, IContentMetadataManager contentMetadataManager)
    {
        readonly IContentRepository contentRepository = contentRepository ?? throw new ArgumentNullException(nameof(contentRepository));
        readonly IContentMetadataManager contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));

        public async Task<object> GetContentAsync(string websiteId, string key, CancellationToken cancellationToken = default) // todo : придумать как хранить тег в бд.
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);

            var contentData = await contentRepository.GetContentAsync(websiteId, key, cancellationToken);
            if (contentData == null)
                return null; // throw new InvalidOperationException($"Not found content by key {key}.");

            if (!contentData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out var contentTypeName))
                throw new InvalidOperationException($"Not found content type name.");

            if (!contentMetadataManager.TryGetMetadata((string)contentTypeName, out var metadata))
                throw new InvalidOperationException($"Not found content type by name {contentTypeName}.");

            return metadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task SetContentAsync(string websiteId, string key, object contentModel, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(contentModel);

            if (!contentMetadataManager.TryGetMetadata(contentModel, out var metadata))
                throw new InvalidOperationException($"Not found content type by type {contentModel.GetType().AssemblyQualifiedName}.");

            var contentData = metadata.ConvertContentModelToDictionary(contentModel);

            await contentRepository.SetContentAsync(websiteId, key, contentData, cancellationToken);
        }
    }
}