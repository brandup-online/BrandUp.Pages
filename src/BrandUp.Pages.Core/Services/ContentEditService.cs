using BrandUp.Pages.Content;
using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Services
{
    public class ContentEditService(IContentEditRepository editSessionRepository, IPageService pageService, Identity.IAccessProvider accessProvider, ContentService contentService, IContentMetadataManager contentMetadataManager) : IContentEditService
    {
        readonly IContentEditRepository editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
        readonly IPageService pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        readonly ContentService contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        readonly Identity.IAccessProvider accessProvider = accessProvider ?? throw new ArgumentNullException(nameof(accessProvider));

        public async Task<IContentEdit> BeginEditAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var editorId = await GetEditorIdAsync(cancellationToken);
            var pageContent = await contentService.GetContentAsync(page.WebsiteId, await pageService.GetContentKeyAsync(page.Id, cancellationToken), cancellationToken);

            if (!contentMetadataManager.TryGetMetadata(pageContent, out var metadata))
                throw new InvalidOperationException();
            var contentData = metadata.ConvertContentModelToDictionary(pageContent);

            return await editSessionRepository.CreateEditAsync(page, editorId, contentData, cancellationToken);
        }

        public Task<IContentEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return editSessionRepository.FindEditByIdAsync(id, cancellationToken);
        }

        public async Task<IContentEdit> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(page);

            var userId = await GetEditorIdAsync(cancellationToken);

            return await editSessionRepository.FindEditByUserAsync(page, userId, cancellationToken);
        }

        public async Task<object> GetContentAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            var page = await pageService.FindPageByIdAsync(editSession.PageId, cancellationToken);
            var pageMetadataProvider = await pageService.GetPageTypeAsync(page, cancellationToken);
            var pageContentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);

            return pageMetadataProvider.ContentMetadata.ConvertDictionaryToContentModel(pageContentData);
        }

        public async Task SetContentAsync(IContentEdit editSession, object content, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);
            ArgumentNullException.ThrowIfNull(content);

            var page = await pageService.FindPageByIdAsync(editSession.PageId, cancellationToken);
            var pageMetadata = await pageService.GetPageTypeAsync(page, cancellationToken);

            var contentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(content);

            await editSessionRepository.SetContentAsync(editSession, contentData, cancellationToken);
        }

        public async Task CommitEditAsync(IContentEdit editSession, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(editSession);

            var page = await pageService.FindPageByIdAsync(editSession.PageId, cancellationToken);
            var pageMetadata = await pageService.GetPageTypeAsync(page, cancellationToken);
            var newContentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);
            var pageContentModel = pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(newContentData);

            var pageContentKey = await pageService.GetContentKeyAsync(page.Id, cancellationToken);
            await contentService.SetContentAsync(page.WebsiteId, pageContentKey, pageContentModel, cancellationToken);

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