using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Services
{
    public class ContentService(IContentRepository contentRepository, IContentMetadataManager contentMetadataManager)
    {
        readonly IContentRepository contentRepository = contentRepository ?? throw new ArgumentNullException(nameof(contentRepository));
        readonly IContentMetadataManager contentMetadataManager = contentMetadataManager ?? throw new ArgumentNullException(nameof(contentMetadataManager));

        public async Task<object> GetContentAsync(string key, CancellationToken cancellationToken = default) // todo : придумать как хранить тег в бд.
        {
            ArgumentNullException.ThrowIfNull(key);

            key = NormalizeAndValidateKey(key);

            var contentData = await contentRepository.GetContentAsync(key, cancellationToken);
            if (contentData == null)
                throw new InvalidOperationException($"Not found content by key {key}.");

            if (!contentData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out var contentTypeName))
                throw new InvalidOperationException($"Not found content type name.");

            if (!contentMetadataManager.TryGetMetadata(contentTypeName, out var metadata))
                throw new InvalidOperationException($"Not found content type by name {contentTypeName}.");

            return metadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task SetContentAsync(string key, object contentModel, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(contentModel);

            key = NormalizeAndValidateKey(key);

            if (!contentMetadataManager.TryGetMetadata(contentModel, out var metadata))
                throw new InvalidOperationException($"Not found content type by type {contentModel.GetType().AssemblyQualifiedName}.");

            var contentData = metadata.ConvertContentModelToDictionary(contentModel);

            await contentRepository.SetContentAsync(key, contentData, cancellationToken);
        }

        static string NormalizeAndValidateKey(string key)
        {
            var result = key.Trim().ToLower();
            if (string.IsNullOrEmpty(result))
                throw new InvalidOperationException($"Invalid key \"{key}\".");
            return result;
        }
    }
}