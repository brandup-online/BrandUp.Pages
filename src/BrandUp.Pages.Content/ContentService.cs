using BrandUp.Pages.Content.Infrastructure;
using BrandUp.Pages.Content.Items;
using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Content
{
    public class ContentService(ContentMetadataManager contentMetadataManager, IContentRepository contentRepository, IContentEditRepository contentEditRepository, IDefaultContentDataProvider defaultContentDataProvider, IServiceProvider serviceProvider)
    {
        public async Task<object> CreateDefaultAsync(ContentMetadata contentMetadata, CancellationToken cancellationToken = default)
        {
            var contentData = await defaultContentDataProvider.GetDefaultAsync(contentMetadata, cancellationToken);
            if (contentData == null)
                return null;

            return contentMetadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task<IContent> CreateAsync(string itemType, string itemId, string contentKey, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(itemType);
            ArgumentNullException.ThrowIfNull(itemId);
            ArgumentNullException.ThrowIfNull(contentKey);

            return await contentRepository.CreateContentAsync(itemType, itemId, contentKey, cancellationToken);
        }

        public async Task<IContent> FindContentAsync(Guid contentId, CancellationToken cancellationToken = default)
        {
            return await contentRepository.FindByIdAsync(contentId, cancellationToken);
        }

        public async Task<IContent> FindContentAsync(string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(key);

            return await contentRepository.FindByKeyAsync(key, cancellationToken);
        }

        public async Task<IContent> FindContentAsync<TItem>(TItem item, CancellationToken cancellationToken = default)
            where TItem : IItemContent
        {
            ArgumentNullException.ThrowIfNull(item);

            var itemContentProvider = serviceProvider.GetContentMappingProvider<TItem>();
            var contentKey = await itemContentProvider.GetContentKeyAsync(item, cancellationToken);

            return await contentRepository.FindByKeyAsync(contentKey, cancellationToken);
        }

        public async Task<IContentData> GetContentAsync(string commitId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(commitId);

            var commitResult = await contentRepository.FindCommitByIdAsync(commitId, cancellationToken);
            if (commitResult == null)
                throw new InvalidOperationException($"Not found content by commit {commitId}.");

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

            return await contentEditRepository.FindEditByUserAsync(content.Id, userId, cancellationToken);
        }

        public async Task<IContentEdit> BeginEditAsync(string contentKey, string userId, ContentMetadata contentProvider, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(contentKey);
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(contentProvider);

            var content = await FindContentAsync(contentKey, cancellationToken);
            if (content == null)
                throw new ArgumentException($"Not found content by key \"{contentKey}\".", nameof(contentKey));
            //content ??= await contentRepository.CreateContentAsync(contentKey, cancellationToken);

            var contentEvents = serviceProvider.GetContentEvents(content.ItemType);

            object contentModel;
            if (content.CommitId == null)
            {
                contentModel = await CreateDefaultAsync(contentProvider, cancellationToken);
                if (contentModel == null)
                    throw new InvalidOperationException($"Not found default data for content type {contentProvider.Name}.");

                await contentEvents.OnDefaultFactoryAsync(content.ItemId, contentModel, cancellationToken);
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
            if (!contentData.TryGetValue(ContentMetadata.ContentTypeNameDataKey, out var contentTypeName))
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

            var content = await contentRepository.FindByIdAsync(editSession.ContentId, cancellationToken);
            if (content == null)
                throw new InvalidOperationException();

            var contentEvents = serviceProvider.GetContentEvents(content.ItemType);

            var contentData = await contentEditRepository.GetContentAsync(editSession, cancellationToken);
            var contentMetadata = contentMetadataManager.GetMetadata(contentData);
            var contentModel = contentMetadata.ConvertDictionaryToContentModel(contentData);
            var contentTitle = contentMetadata.GetContentTitle(contentModel);

            await contentRepository.CreateCommitAsync(editSession.ContentId, editSession.BaseCommitId, editSession.UserId, contentMetadata.Name, contentData, contentTitle, cancellationToken);

            await contentEditRepository.DeleteEditAsync(editSession, cancellationToken);

            await contentEvents.OnUpdatedContentAsync(content.ItemId, contentModel, cancellationToken);
        }

        public Task DiscardEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            return contentEditRepository.DeleteEditAsync(editSession, cancellationToken);
        }

        class ContentData : IContentData
        {
            public ContentMetadata Provider { get; init; }
            public object Data { get; init; }
            public string CommitId { get; init; }
        }
    }

    public interface IContent
    {
        Guid Id { get; }
        string ItemType { get; set; }
        string ItemId { get; set; }
        string Key { get; }
        string CommitId { get; }
    }

    public interface IContentData
    {
        string CommitId { get; }
        ContentMetadata Provider { get; }
        object Data { get; }
    }

    public interface IContentEdit
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        int Version { get; }
        string ContentKey { get; }
        Guid ContentId { get; }
        string BaseCommitId { get; }
        string UserId { get; }
    }
}