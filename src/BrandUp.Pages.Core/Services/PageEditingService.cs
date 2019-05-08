using BrandUp.Pages.Interfaces;
using System;
using System.Threading.Tasks;

namespace BrandUp.Pages.Services
{
    public class PageEditingService : IPageEditingService
    {
        private readonly IPageEditSessionRepository editSessionRepository;
        private readonly IPageService pageService;

        public PageEditingService(IPageEditSessionRepository editSessionRepository,
            IPageService pageRepositiry,
            Content.IContentMetadataManager contentMetadataManager)
        {
            this.editSessionRepository = editSessionRepository ?? throw new ArgumentNullException(nameof(editSessionRepository));
            pageService = pageRepositiry ?? throw new ArgumentNullException(nameof(pageRepositiry));
        }

        public async Task<IPageEditSession> BeginEditAsync(IPage page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            var contentData = await pageService.GetPageContentAsync(page);
            var pageMetadata = await pageService.GetPageTypeAsync(page);
            var pageData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(contentData);

            return await editSessionRepository.CreateEditSessionAsync(page.Id, "test", new PageContent(1, pageData));
        }
        public Task<IPageEditSession> FindEditSessionById(Guid id)
        {
            return editSessionRepository.FindEditSessionByIdAsync(id);
        }
        public async Task<object> GetContentAsync(IPageEditSession editSession)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadataProvider = await pageService.GetPageTypeAsync(page);
            var pageContent = await editSessionRepository.GetContentAsync(editSession.Id);

            return pageMetadataProvider.ContentMetadata.ConvertDictionaryToContentModel(pageContent.Data);
        }
        public async Task SetContentAsync(IPageEditSession editSession, object content)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageService.GetPageTypeAsync(page);

            var contentData = pageMetadata.ContentMetadata.ConvertContentModelToDictionary(content);

            await editSessionRepository.SetContentAsync(editSession.Id, new PageContent(1, contentData));
        }
        public async Task CommitEditSessionAsync(IPageEditSession editSession)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            var page = await pageService.FindPageByIdAsync(editSession.PageId);
            var pageMetadata = await pageService.GetPageTypeAsync(page);
            var newContent = await editSessionRepository.GetContentAsync(editSession.Id);
            var pageContentModel = pageMetadata.ContentMetadata.ConvertDictionaryToContentModel(newContent.Data);

            await pageService.SetPageContentAsync(page, pageContentModel);

            await editSessionRepository.DeleteEditSession(editSession.Id);
        }
        public Task DiscardEditSession(IPageEditSession editSession)
        {
            if (editSession == null)
                throw new ArgumentNullException(nameof(editSession));

            return editSessionRepository.DeleteEditSession(editSession.Id);
        }
    }
}