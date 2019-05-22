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
        private readonly Administration.IAdministrationManager administrationManager;

        public PageContentService(IPageContentRepository editSessionRepository,
            IPageService pageService,
            Administration.IAdministrationManager administrationManager)
        {
            this.editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.administrationManager = administrationManager ?? throw new ArgumentNullException(nameof(administrationManager));
        }

        public async Task<IPageEdit> BeginEditAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var userId = await GetUserIdAsync(cancellationToken);

            return await editSessionRepository.CreateEditAsync(page, userId, cancellationToken);
        }
        public Task<IPageEdit> FindEditByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return editSessionRepository.FindEditByIdAsync(id, cancellationToken);
        }
        public async Task<IPageEdit> FindEditByUserAsync(IPage page, CancellationToken cancellationToken = default)
        {
            var userId = await GetUserIdAsync(cancellationToken);

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

        private async Task<string> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            var userId = await administrationManager.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException();
            return userId;
        }
    }
}