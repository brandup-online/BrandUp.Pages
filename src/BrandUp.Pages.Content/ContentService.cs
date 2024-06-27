using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Content
{
    public class ContentService(ContentMetadataManager contentMetadataManager, IContentRepository contentRepository, IDefaultContentDataProvider defaultContentDataProvider)
    {
        public async Task<object> CreateDefaultAsync(ContentMetadataProvider contentMetadata, CancellationToken cancellationToken = default)
        {
            var contentData = await defaultContentDataProvider.GetDefaultAsync(contentMetadata);
            if (contentData == null)
                return null;

            return contentMetadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task<IContent> FindByKeyAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);

            return await contentRepository.FindByKeyAsync(websiteId, key, cancellationToken);
        }

        public async Task<IContentData> GetContentAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);

            var contentDataSource = await contentRepository.GetDataAsync(websiteId, key, cancellationToken);
            if (contentDataSource == null)
                return null;

            var contentProvider = contentMetadataManager.GetMetadata(contentDataSource.Type);

            return new ContentData
            {
                Provider = contentProvider,
                Data = contentProvider.ConvertDictionaryToContentModel(contentDataSource.Data),
                Version = contentDataSource.Version
            };
        }

        internal async Task SetContentAsync(IContentEdit contentEdit, object contentModel, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contentEdit);
            ArgumentNullException.ThrowIfNull(contentModel);

            if (!contentMetadataManager.TryGetMetadata(contentModel, out var contentMetadata))
                throw new InvalidOperationException($"Not found content type by type {contentModel.GetType().AssemblyQualifiedName}.");

            var contentTitle = contentMetadata.GetContentTitle(contentModel);
            var contentData = contentMetadata.ConvertContentModelToDictionary(contentModel);

            var currentData = await contentRepository.GetDataAsync(contentEdit.WebsiteId, contentEdit.ContentKey, cancellationToken);

            await contentRepository.SetDataAsync(
                contentEdit.WebsiteId, contentEdit.ContentKey, contentEdit.ContentVersion,
                contentMetadata.Name,
                contentTitle, contentData,
                cancellationToken);
        }

        class ContentData : IContentData
        {
            public ContentMetadataProvider Provider { get; init; }
            public object Data { get; init; }
            public string Version { get; init; }
        }
    }

    public interface IContent
    {
        Guid Id { get; }
        string WebsiteId { get; }
        string Key { get; }
        string Type { get; }
        string Title { get; }
        string Version { get; }
    }

    public interface IContentData
    {
        ContentMetadataProvider Provider { get; }
        object Data { get; }
        string Version { get; }
    }
}