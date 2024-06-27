using BrandUp.Pages.Content.Repositories;

namespace BrandUp.Pages.Content
{
    public class ContentEditService(IContentEditRepository editSessionRepository, ContentService contentService, ContentMetadataManager contentMetadataManager)
    {
        public async Task<IContentEdit> BeginEditAsync(string websiteId, string key, string userId, ContentMetadataProvider contentMetadata, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(contentMetadata);

            object contentModel;
            var contentData = await contentService.GetContentAsync(websiteId, key, cancellationToken);
            if (contentData == null)
            {
                contentModel = await contentService.CreateDefaultAsync(contentMetadata, cancellationToken);
                if (contentModel == null)
                    throw new InvalidOperationException($"Not found default data for content type {contentMetadata.Name}.");
            }
            else
            {
                if (!contentMetadata.IsInheritedOrEqual(contentData.Provider))
                    throw new InvalidOperationException();

                contentModel = contentData.Data;
            }

            var contentEditData = contentMetadata.ConvertContentModelToDictionary(contentModel);

            return await editSessionRepository.CreateEditAsync(websiteId, key, contentData?.Version, userId, contentEditData, cancellationToken);
        }

        public Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return editSessionRepository.FindEditByIdAsync(id, cancellationToken);
        }

        public async Task<IContentEdit> FindEditByUserAsync(string websiteId, string key, string userId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(userId);

            return await editSessionRepository.FindEditByUserAsync(websiteId, key, userId, cancellationToken);
        }

        public async Task<object> GetContentAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            var contentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);
            if (!contentData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out var contentTypeName))
                throw new InvalidOperationException($"Not found content type name.");

            if (!contentMetadataManager.TryGetMetadata((string)contentTypeName, out var metadata))
                throw new InvalidOperationException($"Not found content type by name {contentTypeName}.");

            return metadata.ConvertDictionaryToContentModel(contentData);
        }

        public async Task SetContentAsync(IContentEdit editSession, object content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);
            ArgumentNullException.ThrowIfNull(content);

            if (!contentMetadataManager.TryGetMetadata(content, out var metadata))
                throw new InvalidOperationException($"Not found content type by type {content.GetType().AssemblyQualifiedName}.");

            var contentData = metadata.ConvertContentModelToDictionary(content);

            await editSessionRepository.SetContentAsync(editSession, contentData, cancellationToken);
        }

        public async Task CommitAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            var currentContent = await contentService.FindByKeyAsync(editSession.WebsiteId, editSession.ContentKey, cancellationToken);

            var newContentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);
            var contentMetadata = contentMetadataManager.GetMetadata(newContentData);

            var newContentModel = contentMetadata.ConvertDictionaryToContentModel(newContentData);

            await contentService.SetContentAsync(editSession, newContentModel, cancellationToken);

            await editSessionRepository.DeleteEditAsync(editSession, cancellationToken);
        }

        public Task DiscardAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            return editSessionRepository.DeleteEditAsync(editSession, cancellationToken);
        }
    }

    public interface IContentEdit
    {
        Guid Id { get; }
        DateTime CreatedDate { get; }
        string WebsiteId { get; }
        string ContentKey { get; }
        string ContentVersion { get; }
        string UserId { get; }
    }
}