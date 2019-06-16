using BrandUp.Pages.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageContentService : IPageContentService
    {
        private readonly IPageContentRepository editSessionRepository;
        private readonly IPageService pageService;
        private readonly Identity.IAccessProvider signInManager;

        public PageContentService(IPageContentRepository editSessionRepository,
            IPageService pageService,
            Identity.IAccessProvider signInManager)
        {
            this.editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public async Task<IPageEdit> BeginEditAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var editorId = await GetEditorIdAsync(cancellationToken);

            return await editSessionRepository.CreateEditAsync(page, editorId, cancellationToken);
        }
        public Task<IPageEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return editSessionRepository.FindEditByIdAsync(id, cancellationToken);
        }
        public async Task<IPageEdit> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var userId = await GetEditorIdAsync(cancellationToken);

            return await editSessionRepository.FindEditByUserAsync(page, userId, cancellationToken);
        }
        public async Task<object> GetContentAsync(IPageEdit editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadataProvider = await pageService.GetPageTypeAsync(page);
            var pageContentData = await editSessionRepository.GetContentAsync(editSession, cancellationToken);

            return pageMetadataProvider.ContentMetadata.ConvertDictionaryToContentModel(pageContentData);
        }
        public async Task SetContentAsync(IPageEdit editSession, object content, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageService.GetPageTypeAsync(page);

            var contentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(content);

            await editSessionRepository.SetContentAsync(editSession, contentData, cancellationToken);
        }
        public async Task CommitEditAsync(IPageEdit editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageService.GetPageTypeAsync(page);
            var newContentData = await editSessionRepository.GetContentAsync(editSession);
            var pageContentModel = pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(newContentData);

            await pageService.SetPageContentAsync(page, pageContentModel);

            await editSessionRepository.DeleteEditAsync(editSession, cancellationToken);
        }
        public Task DiscardEditAsync(IPageEdit editSession, CancellationToken cancellationToken = default)
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