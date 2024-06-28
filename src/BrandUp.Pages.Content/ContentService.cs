using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Content
{
    public class ContentService(ContentMetadataManager contentMetadataManager, IContentRepository contentRepository, IContentEditRepository contentEditRepository, IDefaultContentDataProvider defaultContentDataProvider)
    {
        public async Task<object> CreateDefaultAsync(ContentMetadataProvider contentMetadata, CancellationToken cancellationToken = default)
        {
            var contentData = await defaultContentDataProvider.GetDefaultAsync(contentMetadata, cancellationToken);
            if (contentData == null)
                return null;

            return contentMetadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task<IContent> FindContentByIdAsync(Guid contentId, CancellationToken cancellationToken = default)
        {
            return await contentRepository.FindByIdAsync(contentId, cancellationToken);
        }

        public async Task<IContent> FindContentByKeyAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);

            return await contentRepository.FindByKeyAsync(websiteId, key, cancellationToken);
        }

        public async Task<IContentData> GetContentAsync(string commitId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(commitId);

            var commitResult = await contentRepository.FindCommitByIdAsync(commitId, cancellationToken);
            if (commitResult == null)
                return null;

            var contentProvider = contentMetadataManager.GetMetadata(commitResult.Type);

            return new ContentData
            {
                Provider = contentProvider,
                Data = contentProvider.ConvertDictionaryToContentModel(commitResult.Data),
                CommitId = commitResult.CommitId
            };
        }

        public async Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await contentEditRepository.FindEditByIdAsync(id, cancellationToken);
        }

        public async Task<IContentEdit> FindEditByUserAsync(IContent content, string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(content);
            ArgumentNullException.ThrowIfNull(userId);

            return await contentEditRepository.FindEditByUserAsync(content.WebsiteId, content.Key, userId, cancellationToken);
        }

        public async Task<IContentEdit> BeginEditAsync(string websiteId, string contentKey, string userId, ContentMetadataProvider contentProvider, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(contentKey);
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(contentProvider);

            var content = await FindContentByKeyAsync(websiteId, contentKey, cancellationToken);
            content ??= await contentRepository.CreateContentAsync(websiteId, contentKey, cancellationToken);

            object contentModel;
            if (content.CommitId == null)
            {
                contentModel = await CreateDefaultAsync(contentProvider, cancellationToken);
                if (contentModel == null)
                    throw new InvalidOperationException($"Not found default data for content type {contentProvider.Name}.");
            }
            else
            {
                var commitResult = await contentRepository.FindCommitByIdAsync(content.CommitId, cancellationToken);

                var commitContentProvider = contentMetadataManager.GetMetadata(commitResult.Type);
                if (!commitContentProvider.IsInheritedOrEqual(contentProvider))
                    throw new InvalidOperationException();
                contentProvider = commitContentProvider;

                contentModel = commitContentProvider.ConvertDictionaryToContentModel(commitResult.Data);
            }

            var contentEditData = contentProvider.ConvertContentModelToDictionary(contentModel);

            return await contentEditRepository.CreateEditAsync(content, userId, contentEditData, cancellationToken);
        }

        public async Task<object> GetEditContentAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            var contentData = await contentEditRepository.GetContentAsync(editSession, cancellationToken);
            if (!contentData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out var contentTypeName))
                throw new InvalidOperationException($"Not found content type name.");

            if (!contentMetadataManager.TryGetMetadata((string)contentTypeName, out var metadata))
                throw new InvalidOperationException($"Not found content type by name {contentTypeName}.");

            return metadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task UpdateEditContentAsync(IContentEdit editSession, object content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);
            ArgumentNullException.ThrowIfNull(content);

            if (!contentMetadataManager.TryGetMetadata(content, out var metadata))
                throw new InvalidOperationException($"Not found content type by type {content.GetType().AssemblyQualifiedName}.");

            var contentData = metadata.ConvertContentModelToDictionary(content);

            await contentEditRepository.UpdateContentAsync(editSession, contentData, cancellationToken);
        }

        public async Task CommitEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            var newContentData = await contentEditRepository.GetContentAsync(editSession, cancellationToken);
            var contentMetadata = contentMetadataManager.GetMetadata(newContentData);
            var newContentModel = contentMetadata.ConvertDictionaryToContentModel(newContentData);
            var contentTitle = contentMetadata.GetContentTitle(newContentModel);

            await contentRepository.CreateCommitAsync(editSession.ContentId, editSession.BaseCommitId, editSession.UserId, contentMetadata.Name, newContentData, contentTitle, cancellationToken);

            await contentEditRepository.DeleteEditAsync(editSession, cancellationToken);
        }

        public Task DiscardEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            return contentEditRepository.DeleteEditAsync(editSession, cancellationToken);
        }

        class ContentData : IContentData
        {
            public ContentMetadataProvider Provider { get; init; }
            public object Data { get; init; }
            public string CommitId { get; init; }
        }
    }

    public interface IContent
    {
        Guid Id { get; }
        string WebsiteId { get; }
        string Key { get; }
        string CommitId { get; }
    }

    public interface IContentData
    {
        string CommitId { get; }
        ContentMetadataProvider Provider { get; }
        object Data { get; }
    }

    public interface IContentEdit
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        int Version { get; }
        string WebsiteId { get; }
        string ContentKey { get; }
        Guid ContentId { get; }
        string BaseCommitId { get; }
        string UserId { get; }
    }
}