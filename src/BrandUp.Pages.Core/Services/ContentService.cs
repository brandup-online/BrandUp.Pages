using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Services
{
    public class ContentService(ContentMetadataManager contentMetadataManager, IContentRepository contentRepository)
    {
        public async Task<object> GetContentAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);

            var contentData = await contentRepository.GetDataAsync(websiteId, key, cancellationToken);
            if (contentData == null)
                return null;

            var contentMetadata = contentMetadataManager.GetMetadata(contentData);

            return contentMetadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task SetContentAsync(string websiteId, string key, object contentModel, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(contentModel);

            if (!contentMetadataManager.TryGetMetadata(contentModel, out var contentMetadata))
                throw new InvalidOperationException($"Not found content type by type {contentModel.GetType().AssemblyQualifiedName}.");

            var contentTitle = contentMetadata.GetContentTitle(contentModel);
            var contentData = contentMetadata.ConvertContentModelToDictionary(contentModel);

            await contentRepository.SetDataAsync(websiteId, key, contentMetadata.Name, contentTitle, contentData, cancellationToken);
        }
    }
}