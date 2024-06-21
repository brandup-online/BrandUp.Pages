using BrandUp.Pages.Interfaces;

namespace BrandUp.Pages.Services
{
    public class EditSessionService(IEditSessionRepository editSessionRepository, IPageService pageService, Identity.IAccessProvider signInManager) : IEditSessionService
    {
        readonly IEditSessionRepository editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
        readonly IPageService pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        readonly Identity.IAccessProvider signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));

        public async Task<IEditSession> BeginEditAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var editorId = await GetEditorIdAsync(cancellationToken);

            return await editSessionRepository.CreateEditAsync(page, editorId, cancellationToken);
        }
        public Task<IEditSession> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return editSessionRepository.FindEditByIdAsync(id, cancellationToken);
        }
        public async Task<IEditSession> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var userId = await GetEditorIdAsync(cancellationToken);

            return await editSessionRepository.FindEditByUserAsync(page, userId, cancellationToken);
        }
        public async Task<object> GetContentAsync(IEditSession editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId, cancellationToken);
            var pageMetadataProvider = await pageService.GetPageTypeAsync(page, cancellationToken);
            var pageContentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);

            return pageMetadataProvider.ContentMetadata.ConvertDictionaryToContentModel(pageContentData);
        }
        public async Task SetContentAsync(IEditSession editSession, object content, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId, cancellationToken);
            var pageMetadata = await pageService.GetPageTypeAsync(page, cancellationToken);

            var contentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(content);

            await editSessionRepository.SetContentAsync(editSession, contentData, cancellationToken);
        }
        public async Task CommitEditAsync(IEditSession editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId, cancellationToken);
            var pageMetadata = await pageService.GetPageTypeAsync(page, cancellationToken);
            var newContentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);
            var pageContentModel = pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(newContentData);

            await pageService.SetPageContentAsync(page, pageContentModel, cancellationToken);

            await editSessionRepository.DeleteEditAsync(editSession, cancellationToken);
        }
        public Task DiscardEditAsync(IEditSession editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            return editSessionRepository.DeleteEditAsync(editSession, cancellationToken);
        }

        private async Task<string> GetEditorIdAsync(CancellationToken cancellationToken = default)
        {
            var userId = await signInManager.GetUserIdAsync(cancellationToken);
            if (userId == null)
                throw new InvalidOperationException();
            return userId;
        }
    }
}