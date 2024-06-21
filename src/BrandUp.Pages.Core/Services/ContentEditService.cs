﻿using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Services
{
    public class ContentEditService(IContentEditRepository editSessionRepository, Identity.IAccessProvider accessProvider, ContentService contentService, IContentMetadataManager contentMetadataManager) : IContentEditService
    {
        public async Task<IContentEdit> BeginEditAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);

            var contentModel = await contentService.GetContentAsync(websiteId, key, cancellationToken);
            if (contentModel == null)
                return null;

            if (!contentMetadataManager.TryGetMetadata(contentModel, out var metadata))
                throw new InvalidOperationException();

            var editorId = await GetEditorIdAsync(cancellationToken);

            var contentData = metadata.ConvertContentModelToDictionary(contentModel);

            return await editSessionRepository.CreateEditAsync(websiteId, key, editorId, contentData, cancellationToken);
        }

        public Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return editSessionRepository.FindEditByIdAsync(id, cancellationToken);
        }

        public async Task<IContentEdit> FindEditByUserAsync(string websiteId, string key, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(websiteId);
            ArgumentNullException.ThrowIfNull(key);

            var userId = await GetEditorIdAsync(cancellationToken);

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

        public async Task CommitEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            var newContentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);
            if (!newContentData.TryGetValue(ContentMetadataProvider.ContentTypeNameDataKey, out var contentTypeName))
                throw new InvalidOperationException($"Not found content type name.");

            if (!contentMetadataManager.TryGetMetadata((string)contentTypeName, out var metadata))
                throw new InvalidOperationException($"Not found content type by name {contentTypeName}.");

            var newContentModel = metadata.ConvertDictionaryToContentModel(newContentData);

            await contentService.SetContentAsync(editSession.WebsiteId, editSession.ContentKey, newContentModel, cancellationToken);

            await editSessionRepository.DeleteEditAsync(editSession, cancellationToken);
        }

        public Task DiscardEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            return editSessionRepository.DeleteEditAsync(editSession, cancellationToken);
        }

        private async Task<string> GetEditorIdAsync(CancellationToken cancellationToken = default)
        {
            var userId = await accessProvider.GetUserIdAsync(cancellationToken);
            if (userId == null)
                throw new InvalidOperationException();
            return userId;
        }
    }
}