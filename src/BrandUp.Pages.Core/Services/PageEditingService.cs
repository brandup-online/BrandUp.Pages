using BrandUp.Pages.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageEditingService : IPageEditingService
    {
        private readonly IPageEditSessionRepository editSessionRepository;
        private readonly IPageService pageService;
        private readonly Administration.IAdministrationManager administrationManager;

        public PageEditingService(IPageEditSessionRepository editSessionRepository,
            IPageService pageService,
            Content.IContentMetadataManager contentMetadataManager,
            Administration.IAdministrationManager administrationManager)
        {
            this.editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
            this.pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            this.administrationManager = administrationManager ?? throw new ArgumentNullException(nameof(administrationManager));
        }

        public async Task<IPageEditSession> BeginEditAsync(IPage page, CancellationToken cancellationToken = default)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var userId = await administrationManager.GetUserIdAsync(cancellationToken);
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException();
            var contentData = await pageService.GetPageContentAsync(page);
            var pageMetadata = await pageService.GetPageTypeAsync(page);
            var pageData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(contentData);

            return await editSessionRepository.CreateEditSessionAsync(page.Id, userId, new PageContent(1, pageData));
        }
        public Task<IPageEditSession> FindEditSessionById(Guid id, CancellationToken cancellationToken = default)
        {
            return editSessionRepository.FindEditSessionByIdAsync(id);
        }
        public async Task<object> GetContentAsync(IPageEditSession editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadataProvider = await pageService.GetPageTypeAsync(page);
            var pageContent = await editSessionRepository.GetContentAsync(editSession.Id);

            return pageMetadataProvider.ContentMetadata.ConvertDictionaryToContentModel(pageContent.Data);
        }
        public async Task SetContentAsync(IPageEditSession editSession, object content, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageService.GetPageTypeAsync(page);

            var contentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(content);

            await editSessionRepository.SetContentAsync(editSession.Id, new PageContent(1, contentData));
        }
        public async Task CommitEditSessionAsync(IPageEditSession editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageService.GetPageTypeAsync(page);
            var newContent = await editSessionRepository.GetContentAsync(editSession.Id);
            var pageContentModel = pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(newContent.Data);

            await pageService.SetPageContentAsync(page, pageContentModel);

            await editSessionRepository.DeleteEditSessionAsync(editSession.Id);
        }
        public Task DiscardEditSession(IPageEditSession editSession, CancellationToken cancellationToken = default)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            return editSessionRepository.DeleteEditSessionAsync(editSession.Id);
        }
    }
}